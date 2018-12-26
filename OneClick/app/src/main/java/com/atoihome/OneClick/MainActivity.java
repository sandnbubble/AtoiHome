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
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.Toast;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;


import static android.widget.Toast.LENGTH_SHORT;

public class MainActivity extends AppCompatActivity  implements View.OnClickListener, ServiceCallbacks {

    private static final String TAG = MainActivity.class.getSimpleName();
    public String strHostURL= "http://";
    public String strServiceURL = "/OneClickShot/WEB";

    private static final int REQUEST_CODE_READ_EXTERNAL_STORAGE_PERMISSION = 3009;
    private BackPressCloseHandler backPressCloseHandler;
    private Intent intentForService;
    public String strScreenshotImagePath;

    private ScreenshotDetectionService sdService;
    private boolean bound = false;
    private boolean bIsVisibleActivity = false;
    /** Callbacks for service binding, passed to bindService() */
//    serviceConnection은 ScreenshotDetectionService와 life cycle이 동일함
    private ServiceConnection serviceConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName className, IBinder service) {
            // cast the IBinder and get sdService instance
            ScreenshotDetectionService.LocalBinder binder = (ScreenshotDetectionService.LocalBinder) service;
            sdService = binder.getService();
            bound = true;
            sdService.setCallbacks(MainActivity.this); // register
//          Service에 종속된 속성변수 값을 여기서 설정할 수 있음 Strong reference이므로 종료처리시 주의
            if (strHostURL != null && strServiceURL != null) {
                sdService.setHostServiceURL(strHostURL + strServiceURL);
            }
        }

        //        비정상적으로 바인딩이 끊어지는 경우 호출, unbindService는 이 콜백함수를 호출하지않음
        @Override
        public void onServiceDisconnected(ComponentName arg0)
        {
            Toast.makeText(getApplicationContext(), "Terminated bind service by system.", LENGTH_SHORT).show();
            bound = false;
        }
    };


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        try {
            FileInputStream InputSettings = openFileInput("Settings");
            if (InputSettings != null) {
                int nLength = InputSettings.available();
                byte[] ReadData = new byte[nLength];
                InputSettings.read (ReadData, 0, nLength);
                strHostURL = new String(ReadData, 0, nLength);
                InputSettings.close();
            }
            else
            {
                FileOutputStream fos = openFileOutput("Settings", Context.MODE_PRIVATE);
                fos.write(strHostURL.getBytes());
                fos.close();
            }
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }


        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        checkReadExternalStoragePermission();
        setContentView(R.layout.activity_main);
        backPressCloseHandler = new BackPressCloseHandler(this);
        intentForService = new Intent(this, ScreenshotDetectionService.class);

        Button buttonStartService = findViewById(R.id.buttonStartService);
        Button buttonStopService = findViewById(R.id.buttonStopService);
        Button buttonExit = findViewById(R.id.buttonExit);
        Button buttonUploadImage = findViewById((R.id.buttonUploadImage));
        EditText edURL = findViewById(R.id.edURL);
        edURL.setText(strHostURL);

        buttonStartService.setOnClickListener(this);
        buttonStopService.setOnClickListener(this);
        buttonExit.setOnClickListener(this);
        buttonUploadImage.setOnClickListener(this);

        edURL.addTextChangedListener(new TextChangedListener<EditText>(edURL) {
            @Override
            public void onTextChanged(EditText target, Editable s) {
                strHostURL = target.getText().toString();
                if (bound == false) {
                    bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
                }
                else
                {
                    sdService.setHostServiceURL(strHostURL+strServiceURL);
                    try {
                            FileOutputStream fos = openFileOutput("Settings", Context.MODE_PRIVATE);
                            fos.write(strHostURL.getBytes());
                            fos.close();
                    } catch (FileNotFoundException e) {
                        e.printStackTrace();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            }
        });

        bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
        Log.d("Debug", "***************Started service in onCreate");
    }

    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            case R.id.buttonStartService:
                startService(intentForService);
                if (!bound)
                {
                    bindService(intentForService, serviceConnection, Context.BIND_AUTO_CREATE);
                }
                break;
            case R.id.buttonStopService:
                if (bound) {
                    sdService.setCallbacks(null); // unregister
                    unbindService(serviceConnection);
                    bound = false;
                }
                stopService(intentForService);

                break;
            case R.id.buttonUploadImage:
                // UpoladImage클라스의 mContext에 ApplicationContext를 주면 사망함
                UploadFileToServer uploadFile = new UploadFileToServer(this, true);
                uploadFile.uploadFileToServer(strHostURL+strServiceURL, strScreenshotImagePath);
                break;
            case R.id.buttonExit:
                stopService(intentForService);
                finish();
                break;
        }
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


    //    edURL 컨트롤의 값이 변경되면 호출되는 callback method가 사용할 추상클라스
    public abstract class TextChangedListener<T> implements TextWatcher {
        private T target;

        public TextChangedListener(T target) {
            this.target = target;
        }

        @Override
        public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

        @Override
        public void onTextChanged(CharSequence s, int start, int before, int count) {}

        @Override
        public void afterTextChanged(Editable s) {
            this.onTextChanged(target, s);
        }

        public abstract void onTextChanged(T target, Editable s);
    }
}
