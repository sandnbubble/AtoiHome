package com.atoihome.OneClick;

import android.Manifest;
import android.app.Service;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.database.ContentObserver;
import android.database.Cursor;
import android.net.Uri;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.provider.MediaStore;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.widget.Toast;

import static android.app.PendingIntent.getActivity;
import static android.widget.Toast.LENGTH_SHORT;

public class ScreenshotDetectionService extends Service {
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
    private static String mHostServiceURL="http://211.212.200.192/TextTransfer/WEB";
    // Binder given to clients
    private final IBinder binder = new LocalBinder();
    // Registered callbacks
    private ServiceCallbacks serviceCallbacks;


    protected void setHostServiceURL (String strURL) {
        mHostServiceURL = strURL;
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

    private void onScreenshotCaptured(String strFilePath) {
        UploadFileToServer uploadImage = new UploadFileToServer(getApplicationContext(), false);
        uploadImage.uploadFileToServer(mHostServiceURL, strFilePath);
        Toast.makeText(getApplicationContext(), mHostServiceURL+strFilePath, LENGTH_SHORT).show();
        if (serviceCallbacks != null) {
            serviceCallbacks.setScreenshotImagePath(strFilePath);
        }
    }

    private void onScreenshotCapturedWithDeniedPermission() {
            Toast.makeText(this, "Read external storage permission has denied", LENGTH_SHORT).show();
    }

    private ContentObserver contentObserver = new ContentObserver(new Handler()) {
        @Override
        public boolean deliverSelfNotifications() {
            return super.deliverSelfNotifications();
        }

        @Override
        public void onChange(boolean selfChange) {
            super.onChange(selfChange);
        }

        @Override
        public void onChange(boolean selfChange, Uri uri) {
            super.onChange(selfChange, uri);
            if (isReadExternalStoragePermissionGranted()) {
                String path = getFilePathFromContentResolver(uri);
                if (isScreenshotPath(path)) {
                    onScreenshotCaptured(path);
                }
            } else {
                onScreenshotCapturedWithDeniedPermission();
            }
        }
    };


    @Override
    public IBinder onBind(Intent intent) {
        return binder;
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

//    MediaPlayer player;
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        startScreenshotDetection();
        Log.d("debug", "Started service in ScreenshotDetectionService.onStartCommand");
//        player = MediaPlayer.create(this, R.raw.kalimba);
//        player.setLooping(true);
//        player.start();
        return super.onStartCommand(intent, flags, startId);
    }

    @Override
    public void onDestroy() {
        stopScreenshotDetection();
        Log.d("debug", "Terminated service in ScreenshotDetectionService.onDestroy");
//        player.stop();
        super.onDestroy();
    }
    public void startScreenshotDetection() {
        getContentResolver().registerContentObserver(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, true, contentObserver);
    }

    public void stopScreenshotDetection() {
        getContentResolver().unregisterContentObserver(contentObserver);
    }

    private boolean isReadExternalStoragePermissionGranted() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED;
    }

    private boolean isScreenshotPath(String path) {
        return path != null && (path.toLowerCase().contains("screenshots") || path.toLowerCase().contains("camera")) ;
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
}
