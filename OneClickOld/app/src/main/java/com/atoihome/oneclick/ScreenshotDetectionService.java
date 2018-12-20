package com.atoihome.oneclick;

import android.Manifest;
import android.app.Service;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.database.ContentObserver;
import android.database.Cursor;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.provider.MediaStore;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.widget.Toast;

import java.net.URI;

import static android.app.PendingIntent.getActivity;
import static android.widget.Toast.LENGTH_LONG;
import static android.widget.Toast.LENGTH_SHORT;

public class ScreenshotDetectionService extends Service implements ContentObserverCallback {
    // service에서 사용하는 mHostServiceURL의 설정을 bind된 activity에서 하는데
    // application의 시스템에 의해 강제로 종료될 경우 service는 살아있지만
    // 속성값들이 null로 된다. 아래와 같이 선언부에서 초기화를 하면 heap이 아니라
    // data segment memory에 저장되는 것 같다. 오래되서 기억이 가물가물
    // 이렇게 하면 activity가 죽어도 mHostServiceURL의 값은 safe하다.
    // 이해가 되지 않는 것은 bindService에서 사용하는 mHostServiceURL과 mHostServiceURL이
    // 다른 메모리를 참조한다는 것이다.
    // 시스템이 MainActivity를 kill하면 이 클라스의 정적으로 선언된 mHostServiceURL의 초기값을
    // 사용한다는 것이다. 이해가 안됨
    // 차라리 스토리지에서 값을 얻어 오는 것이 안전한 것으로 보인다. 수정해야함.
    private static String mHostServiceURL = "http://211.212.200.192/TextTransfer/WEB";
    // Binder given to clients
    private final IBinder binder = new LocalBinder();
    // Registered callbacks
    private ServiceCallbacks serviceCallbacks;

    protected void setHostServiceURL(String strURL) {
        mHostServiceURL = strURL;
    }

    @Override
    public IBinder onBind(Intent intent) {
        return binder;
    }

    // Class used for the client Binder.
    public class LocalBinder extends Binder {
        ScreenshotDetectionService getService() {
            // Return this instance of MyService so clients can call public methods
            return ScreenshotDetectionService.this;
        }
    }

    public void setCallbacks(ServiceCallbacks callbacks) {
        serviceCallbacks = callbacks;
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

    @Override
    public void onDestroy() {
        stopScreenshotDetection();
        Log.d("debug", "Terminated service in ScreenshotDetectionService.onDestroy");
//        player.stop();
        super.onDestroy();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        startScreenshotDetection();
        Log.d("debug", "Started service in ScreenshotDetectionService.onStartCommand");
//        player = MediaPlayer.create(this, R.raw.kalimba);
//        player.setLooping(true);
//        player.start();
        return super.onStartCommand(intent, flags, startId);
    }
    // Handler를 사용하셔 UploadFile에서 ProgressDlg를 사용할 수 있도록 설정
    Handler handler = new Handler();
    GeneralContentObserver ScreenshotObserver = new GeneralContentObserver(handler, this);
    public void startScreenshotDetection() {
        getContentResolver().registerContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, true, ScreenshotObserver);
    }

    public void stopScreenshotDetection() {
        getContentResolver().unregisterContentObserver(ScreenshotObserver);
    }

    // Content 제공자에서 변경이벤트가 발생하면 ContentObserver에서 호출되는 함수
    // ContentObserver와 strong하게 연결되어 있어서 신경이 쓰임
    public void onUpdate(Uri uri) {
        try {
            String strFilePath;

            if (isReadExternalStoragePermissionGranted()) {
                strFilePath = getFilePathFromContentResolver(uri);
                if (strFilePath == null) {
                    Toast.makeText(getApplicationContext(), "Path is null", LENGTH_LONG).show();
                }
                if (isScreenshotPath(strFilePath)) {
                    UploadFileToServer uploadImage = new UploadFileToServer(getApplicationContext(), false);
                    uploadImage.uploadFileToServer(mHostServiceURL, strFilePath);
                    Toast.makeText(getApplicationContext(), mHostServiceURL + strFilePath, LENGTH_SHORT).show();
                    // bind 되어있는 MainActivity에 캡처된 이미지파일의 경로를 알려주는 Callback method
                    if (serviceCallbacks != null) {
                        serviceCallbacks.setScreenshotImagePath(strFilePath);
                    }
                } else {
                    onScreenshotCapturedWithDeniedPermission();
                }
            }
        }catch (Exception e) {
            Log.d ("MYDEBUG", e.getMessage());
        }
    }

    private String getFilePathFromContentResolver(Uri uri) {
        try {
            Cursor cursor = getContentResolver().query(uri, new String[]{
                    MediaStore.Images.Media.DISPLAY_NAME,
                    MediaStore.Images.Media.DATA
            }, null, null, null);
            if (cursor != null && cursor.moveToFirst()) {
                String path = cursor.getString(cursor.getColumnIndex(MediaStore.Images.Media.DATA));
                cursor.close();
                return path;
            }
        } catch (IllegalStateException ignored) {
        }
        return null;
    }

    private void onScreenshotCapturedWithDeniedPermission() {
        Toast.makeText(this, "Read external storage permission has denied", LENGTH_SHORT).show();
    }

    private boolean isReadExternalStoragePermissionGranted() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED;
    }

    private boolean isScreenshotPath(String path) {
        return path != null && path.toLowerCase().contains("screenshots");
    }
}

