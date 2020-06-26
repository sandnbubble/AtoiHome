package com.atoihome.oneclick;

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
    private static final String TAG = MainActivity.class.getSimpleName();
    private static final int REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION = 3009;
    private BackPressCloseHandler backPressCloseHandler;
    private Intent intentForService;

    public String strScreenshotImagePath;
    private ScreenshotDetectionService sdService;

    private boolean bound = false;

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

    private static boolean mbSigned = false;
    public static void startProcess(boolean bSigned){
        mbSigned = bSigned;
        if (bSigned){
            CtrlServiceInActionBar.setChecked(true);
        }else{
            CtrlServiceInActionBar.setChecked(false);
        }
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Toast.makeText(this, "invoked oncreate", LENGTH_SHORT).show();
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        //환경설정
        checkReadExternalStoragePermission();
        backPressCloseHandler = new BackPressCloseHandler(this);
        intentForService = new Intent(this, ScreenshotDetectionService.class);

        //메인화면구성
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        ImageButton buttonUpload = (ImageButton) findViewById(R.id.buttonUpload);
        buttonUpload.setOnClickListener(this);

        //툴바를 액션바로 교체사용
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar().setDisplayShowCustomEnabled(true);
        getSupportActionBar().setDisplayShowTitleEnabled(true);

        // 캡처 모니터서비스가 실행 중이라면
        // 로그인하여 서비스를 사용중인 상태에서 시스템이나 사용자에 의해
        // 메인엑티비티가 종료된 경우를 체크하기 위함
        if (isServiceRunning(ScreenshotDetectionService.class)){
            // 이 변수를 InstanceState를 사용해 자동저장하도록 할 예정
            bServiceStarted = true;
        }
        else {
            // 앱이 처음 실행된 경우 억세스토큰이 있으면 인증서버로 ValidateToken을 요청해서
            // 토큰의 유효성 검증
            // 유효성 검증이 완료된 후 실행되는 delagator
            NetworkUtils.AsyncResponse asyncResponse =    new NetworkUtils.AsyncResponse(){
                @Override
                public void processFinish(Object msgRet) {
                    NetworkUtils.HttpMessage ResponseMsg = (NetworkUtils.HttpMessage)msgRet;
                    Toast.makeText(getApplicationContext(), "RetCode = " + ResponseMsg.iRet + " "+ResponseMsg.Message, LENGTH_SHORT).show();
                    // 토큰이 유효하면 서비스를 시작한다.
                    if (ResponseMsg.iRet == 200){
                        startProcess(true);
                    }else{
                        // 토큰이 유효하지 않거나 네트워크 장애가 발생하면 로그인 엑티비티를 실행한다.
                        Intent intent = new Intent(getApplicationContext(), LoginActivity.class);
                        startActivity(intent);
                    }
                }
            };
            NetworkUtils networkUtils = new NetworkUtils(this);
            networkUtils.validateTokenRequest(asyncResponse);
        }
        Log.d("Debug", "***************Started service in onCreate");
    }

    public String GetPreferenceValue(String strKey){
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        return Prefs.getString(strKey, "");
    }

    static Switch  CtrlServiceInActionBar;
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        //return super.onCreateOptionsMenu(menu);
        try {
            getMenuInflater().inflate(R.menu.menu, menu);
            MenuItem item = (MenuItem) menu.findItem(R.id.action_ctrlservice);
            item.setActionView(R.layout.ctrlservice_layout);
            CtrlServiceInActionBar = item.getActionView().findViewById(R.id.CtrlServiceActionBar);
            CtrlServiceInActionBar.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                @Override
                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                    if (mbSigned){
                        if (isChecked) {
                            startScreenshotService();
                        } else {
                            stopScreenshotService();
                        }
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
    public boolean  onPrepareOptionsMenu(Menu menu) {
        super.onPrepareOptionsMenu(menu);
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        if (bServiceStarted){
//            Toast.makeText(this, "Invoked onPrepareOutionsMenu (bServiceStarted is true)", LENGTH_SHORT).show();
            CtrlServiceInActionBar.setChecked(true);
            return true;
        }
        else {
            if (Prefs.getBoolean("AutomaticStart", false)) {
                CtrlServiceInActionBar.setChecked(true);
            } else {
                CtrlServiceInActionBar.setChecked(false);
            }
        }
        return true;
    }


    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        //return super.onOptionsItemSelected(item);
        switch (item.getItemId()) {
            case R.id.action_settings:
                startActivity(new Intent(this, SettingActivity.class));
                return true;
            default:
                // If we got here, the user's action was not recognized.
                // Invoke the superclass to handle it.
                Toast.makeText(getApplicationContext(), "Unknown menu was selected", Toast.LENGTH_LONG).show();
                return super.onOptionsItemSelected(item);
        }
    }

    // 메인화면 엘리먼트의 클릭 이벤트 처리
    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            //최근 캡처된 이미지를 수동으로 업로드 할 때 사용하는 버튼의 클릭 이벤트 처리
            //설정에서 자동업로드 옵션을 사용하지 않도록 설정했들 때 카톡등의 서비스제공자로
            //이미지를 전송하고 수동으로 업로드할 수도 있다.
            case R.id.buttonUpload:
                if (strScreenshotImagePath == null){
                    return;
                }
                NetworkUtils.AsyncResponse asyncResponse =    new NetworkUtils.AsyncResponse(){
                    @Override
                    public void processFinish(Object msgRet) {
                        try{
                            NetworkUtils.HttpMessage ResponseMsg = (NetworkUtils.HttpMessage)msgRet;
                            Toast.makeText(getApplicationContext(), "RetCode = " + ResponseMsg.iRet + " "+ResponseMsg.Message, LENGTH_SHORT).show();
                            if (ResponseMsg.iRet == 200){
                            }else{
                            }
                        }catch (Exception e){
                            Toast.makeText(getApplicationContext(), e.getMessage(), LENGTH_SHORT);
                        }
                    }
                };
                NetworkUtils networkUtils = new NetworkUtils(this);
                networkUtils.uploadFileRequest(strScreenshotImagePath, asyncResponse);
                break;
            default:
                break;
        }
    }

    @Override
    protected void onStart() {

        super.onStart();

//        bIsVisibleActivity = true;
        if (strScreenshotImagePath != null) {
            drawImage(strScreenshotImagePath);
        }
//        Toast.makeText(this, "Invoked onStart", LENGTH_SHORT).show();
    }

    @Override
    protected void onStop() {
        super.onStop();
//        bIsVisibleActivity = false;
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
            stopScreenshotService();
            finish();
//            Toast.makeText(this, "Invoked exit", LENGTH_SHORT).show();
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        stopScreenshotService();
    }

    private void startScreenshotService(){
        startService(intentForService);
        if (!bound) {
            bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
        }
        bServiceStarted = true;
    }

    private void stopScreenshotService(){
        if (bound) {
            sdService.setCallbacks(null); // unregister
            unbindService(serviceConnection);
            bound = false;
        }
        stopService(intentForService);
        bServiceStarted = false;
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
        ActivityManager mActivityManager = (ActivityManager) this.getSystemService(Context.ACTIVITY_SERVICE);
        String strClassName = mActivityManager.getRunningTasks(1).get(0).topActivity.getClassName();
//        Toast.makeText(this, strClassName, LENGTH_SHORT).show();
        if (strClassName.equals(getString(R.string.MainActivity))) {
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