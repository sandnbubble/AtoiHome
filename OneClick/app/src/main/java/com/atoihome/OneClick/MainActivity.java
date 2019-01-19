package com.atoihome.OneClick;

import android.Manifest;
import android.app.ActivityManager;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.Switch;
import android.widget.Toast;
import java.io.File;

import static android.widget.Toast.LENGTH_SHORT;

public class MainActivity extends AppCompatActivity  implements View.OnClickListener, ServiceCallbacks {

    // Google 인증

    private static final String TAG = MainActivity.class.getSimpleName();

    private static final int REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION = 3009;
    private BackPressCloseHandler backPressCloseHandler;
    private Intent intentForService;
    private Intent intentForSetting;

    public String strScreenshotImagePath;
    private ScreenshotDetectionService sdService;

    private boolean bound = false;
    private boolean bIsVisibleActivity = false;

    private boolean bServiceStarted = false;

//    Callbacks for service binding, passed to bindService()
//    serviceConnection은 ScreenshotDetectionService와 life cycle이 동일함
    private ServiceConnection serviceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName className, IBinder service) {
            // cast the IBinder and get sdService instance
            ScreenshotDetectionService.LocalBinder binder = (ScreenshotDetectionService.LocalBinder) service;
            sdService = binder.getService();
            bound = true;
            sdService.setCallbacks(MainActivity.this); // register
        }

//      비정상적으로 바인딩이 끊어지는 경우 호출, unbindService는 이 콜백함수를 호출하지않음
        @Override
        public void onServiceDisconnected(ComponentName arg0)
        {
//            Toast.makeText(getApplicationContext(), "Terminated bind service by system.", LENGTH_SHORT).show();
            bound = false;
        }
    };


    @Override
    protected void onCreate(Bundle savedInstanceState) {
//        Toast.makeText(this, "invoked oncreate", LENGTH_SHORT).show();
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Intent intent = new Intent(getApplicationContext(), LoginActivity.class);
        startActivity(intent);
//        Intent intent = new Intent(getApplicationContext(), SignupActivity.class);
//        startActivity(intent);

        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        checkReadExternalStoragePermission();

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        getSupportActionBar().setDisplayShowCustomEnabled(true);
        getSupportActionBar().setDisplayShowTitleEnabled(true);

        ImageButton buttonUpload = (ImageButton) findViewById(R.id.buttonUpload);
        buttonUpload.setOnClickListener(this);

        backPressCloseHandler = new BackPressCloseHandler(this);
        intentForService = new Intent(this, ScreenshotDetectionService.class);

        if (isServiceRunning(ScreenshotDetectionService.class)){
            bServiceStarted = true;
        }
        Log.d("Debug", "***************Started service in onCreate");
    }

    Switch switchCtrlService;

    @Override
    public boolean  onPrepareOptionsMenu(Menu menu) {
        super.onPrepareOptionsMenu(menu);
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        if (bServiceStarted){
//            Toast.makeText(this, "Invoked onPrepareOutionsMenu (bServiceStarted is true)", LENGTH_SHORT).show();
            switchCtrlService.setChecked(true);
            return true;
        }
        else if(Prefs.getBoolean("AutomaticStart", false) ) {
            switchCtrlService.setChecked(true);
        }
        else{
//            Toast.makeText(this, "Invoked onPrepareOutionsMenu (bServiceStarted is false)", LENGTH_SHORT).show();
        }
        return true;
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        //return super.onCreateOptionsMenu(menu);
        try {
            getMenuInflater().inflate(R.menu.menu, menu);
            MenuItem item = (MenuItem) menu.findItem(R.id.action_ctrlservice);
            item.setActionView(R.layout.switch_layout);
            switchCtrlService = item.getActionView().findViewById(R.id.switchActionBar);

            switchCtrlService.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    if (isChecked) {
                        startService(intentForService);
                        if (!bound) {
                            bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
                        }
                        bServiceStarted = true;
//                        Toast.makeText(getApplication(), "ON", Toast.LENGTH_SHORT).show();
                    } else {
                        if (bound) {
                            sdService.setCallbacks(null); // unregister
                            unbindService(serviceConnection);
                            bound = false;
                        }
                        stopService(intentForService);
                        bServiceStarted = false;
//                          Toast.makeText(getApplication(), "OFF", Toast.LENGTH_SHORT).show();
                    }
                }
            });
            return true;
        } catch (Exception e){
            e.printStackTrace();
        }
        return false;
    }



    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        //return super.onOptionsItemSelected(item);
        switch (item.getItemId()) {
            case R.id.action_settings:
                intentForSetting = new Intent(this, SettingActivity.class);
                startActivity(intentForSetting);
                return true;
            default:
                // If we got here, the user's action was not recognized.
                // Invoke the superclass to handle it.
                Toast.makeText(getApplicationContext(), "Unknown menu was selected", Toast.LENGTH_LONG).show();
                return super.onOptionsItemSelected(item);
        }
    }

    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            case R.id.buttonUpload:
                if (strScreenshotImagePath == null){
                    return;
                }
                UploadFileToServer uploadFile = new UploadFileToServer(this, true);
                uploadFile.uploadFileToServer(sdService.getHostServiceURL(), strScreenshotImagePath);
                break;
            default:
                break;
        }
    }

    @Override
    protected void onStart() {

        super.onStart();

        bIsVisibleActivity = true;
        if (strScreenshotImagePath != null) {
            drawImage(strScreenshotImagePath);
        }
//        Toast.makeText(this, "Invoked onStart", LENGTH_SHORT).show();
    }

    @Override
    protected void onStop() {
        super.onStop();
        bIsVisibleActivity = false;
//        Toast.makeText(this, "Invoked onStart", LENGTH_SHORT).show();
    }

    @Override
    public void onResume()
    {
        super.onResume();
//        Toast.makeText(this, "Invoked onResume", LENGTH_SHORT).show();
    }

    @Override
    public void onBackPressed() {
        //super.onBackPressed(); 앱종료가 두번 수행되서는 안됨
        if (backPressCloseHandler.onBackPressed() == true) {
//            서비스와의 바인딩이 끊어지지 않으면 stopService가 종료되지 않음
            if (bound) {
                sdService.setCallbacks(null); // unregister
                unbindService(serviceConnection);
                bound = false;
            }
            stopService(intentForService);
            finish();
//            Toast.makeText(this, "Invoked exit", LENGTH_SHORT).show();
        }
    }


    protected boolean drawImage(String strPath) {
        File imgFile = new File(strPath);

        if (imgFile.exists()) {
            Bitmap bitmapScreenshotImage = BitmapFactory.decodeFile(imgFile.getAbsolutePath());
            ImageView imageview = (ImageView) findViewById(R.id.imageView);
            imageview.setImageBitmap(bitmapScreenshotImage);
        }
        return false;
    }



    @Override
//    Service와 ManinActivity간의 IPC를 bind의 callback으로 구현
//    Service에서 이미지파일의 경로를 MainActivity로 전달
    public void setScreenshotImagePath(String strImagePath) {
        strScreenshotImagePath = strImagePath;
        if (bIsVisibleActivity) {
            drawImage(strImagePath);
        }
    }



    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        switch (requestCode) {
            case REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION:
                if (grantResults[0] == PackageManager.PERMISSION_DENIED) {
                    showReadExternalStoragePermissionDeniedMessage();
                }
                break;
            default:
                super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    private void checkReadExternalStoragePermission() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED) {
            requestReadExternalStoragePermission();
        }
    }

    private void requestReadExternalStoragePermission() {
        ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.READ_EXTERNAL_STORAGE}, REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION);
    }

    private void showReadExternalStoragePermissionDeniedMessage() {
        Toast.makeText(this, "Read external storage permission has denied", LENGTH_SHORT).show();
    }

    private boolean isServiceRunning(Class<?> serviceClass) {
        ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
        for (ActivityManager.RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
            if (serviceClass.getName().equals(service.service.getClassName())) {
                return true;
            }
        }
        return false;
    }
}