package com.atoihome.OneClick;

import android.Manifest;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.pm.ActivityInfo;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.IBinder;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;
import java.io.File;


import static android.widget.Toast.LENGTH_SHORT;

public class MainActivity extends AppCompatActivity  implements View.OnClickListener, ServiceCallbacks {

    private static final String TAG = MainActivity.class.getSimpleName();

    private static final int REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION = 3009;
    private BackPressCloseHandler backPressCloseHandler;
    private Intent intentForService;
    private Intent intentForSetting;

    public String strScreenshotImagePath;
    private ScreenshotDetectionService sdService;

    private boolean bound = false;
    private boolean bIsVisibleActivity = false;

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
            Toast.makeText(getApplicationContext(), "Terminated bind service by system.", LENGTH_SHORT).show();
            bound = false;
        }
    };

    //추가된 소스, ToolBar에 menu.xml을 인플레이트함
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        //return super.onCreateOptionsMenu(menu);
        MenuInflater menuInflater = getMenuInflater();
        menuInflater.inflate(R.menu.menu, menu);
        return true;
    }

    //추가된 소스, ToolBar에 추가된 항목의 select 이벤트를 처리하는 함수
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        //return super.onOptionsItemSelected(item);
        switch (item.getItemId()) {
            case R.id.action_settings:
//                User chose the "Settings" item, show the app settings UI...
//                Toast.makeText(getApplicationContext(), "환경설정 버튼 클릭됨", Toast.LENGTH_LONG).show();
                intentForSetting = new Intent(this, SettingActivity.class);
                startActivity(intentForSetting);
                return true;
            case R.id.action_start:
                startService(intentForService);
                if (!bound)
                {
                    bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
                }
                return true;
            case R.id.action_stop:
                if (bound) {
                    sdService.setCallbacks(null); // unregister
                    unbindService(serviceConnection);
                    bound = false;
                }
                stopService(intentForService);
                return true;
            case R.id.action_exit:
                stopService(intentForService);
                finish();
                return true;
            default:
                // If we got here, the user's action was not recognized.
                // Invoke the superclass to handle it.
                Toast.makeText(getApplicationContext(), "나머지 버튼 클릭됨", Toast.LENGTH_LONG).show();
                return super.onOptionsItemSelected(item);

        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        checkReadExternalStoragePermission();
        setContentView(R.layout.activity_main);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        ActionBar actionBar = getSupportActionBar();
        actionBar.setDisplayShowCustomEnabled(true);
        actionBar.setDisplayShowTitleEnabled(true);

        backPressCloseHandler = new BackPressCloseHandler(this);
        intentForService = new Intent(this, ScreenshotDetectionService.class);

        bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
        Log.d("Debug", "***************Started service in onCreate");
    }

    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            default:
                break;
        }
    }

    public void ImageView_Clicked (View view) {
        // UpoladImage클라스의 mContext에 ApplicationContext를 주면 사망함
        if (strScreenshotImagePath == null){
            return;
        }
        UploadFileToServer uploadFile = new UploadFileToServer(this, true);
        uploadFile.uploadFileToServer(sdService.getmHostServiceURL(), strScreenshotImagePath);
    }

    @Override
    protected void onStart() {

        Log.d("debug", "Invoked MainActivity.onStart ");
        super.onStart();
        bIsVisibleActivity = true;
        if (strScreenshotImagePath != null) {
            drawImage(strScreenshotImagePath);
        }
    }

    @Override
    protected void onStop() {
        super.onStop();
        bIsVisibleActivity = false;
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
        }
    }

    protected boolean drawImage(String strPath) {
        File imgFile = new File(strPath);

        if (imgFile.exists()) {
            Bitmap bitmapScreenshotImage = BitmapFactory.decodeFile(imgFile.getAbsolutePath());
            ImageView imageview = (ImageView) findViewById(R.id.imageView);
            imageview.setImageBitmap(bitmapScreenshotImage);
            return true;
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
}