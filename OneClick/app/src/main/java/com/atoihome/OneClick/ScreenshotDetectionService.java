package com.atoihome.OneClick;

import android.Manifest;
import android.app.Service;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.database.ContentObserver;
import android.database.Cursor;
import android.net.Uri;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.provider.MediaStore;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.widget.Toast;

import static android.widget.Toast.LENGTH_SHORT;

public class ScreenshotDetectionService extends Service {
    // service에서 사용하는 mHostServiceURL의 설정을 bind된 activity할 경우
    // application이 시스템에 의해 강제로 종료될 경우 service는 살아있지만
    // activity에서 설정한 service의 속성값들이 null로 된다.
    // 아래와 같이 선언부에서 초기화해서 data segment memory에 저장하면
    // mHostServiceURL의 값은 safe하지만 하드코딩이기 때문에 변경도 할 수 없다.
    // SharedPreference를 사용하는 것으로 교체해서 해결.
    // Binder given to clients
    private final IBinder binder = new LocalBinder();
    // Registered callbacks
    private ServiceCallbacks serviceCallbacks;

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
    public IBinder onBind(Intent intent) {
        return binder;
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        startScreenshotDetection();
        Log.d("debug", "Started service in ScreenshotDetectionService.onStartCommand");
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

    private void onScreenshotCaptured(String strFilePath) {
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        if (Prefs.getBoolean("AutomaticUpload", false)){
            UploadFileToServer uploadImage = new UploadFileToServer(getApplicationContext(), false);
            uploadImage.uploadFileToServer(getHostServiceURL(), strFilePath);
            Toast.makeText(getApplicationContext(), getHostServiceURL()+strFilePath, LENGTH_SHORT).show();
        }
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

    private boolean isReadExternalStoragePermissionGranted() {
        return ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED;
    }

    private boolean isScreenshotPath(String path) {
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        if (Prefs.getBoolean("UploadCameraShot", false)) {
            return path != null && (path.toLowerCase().contains("screenshots") || path.toLowerCase().contains("camera")) ;
        } else {
            return path != null && (path.toLowerCase().contains("screenshots")) ;
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

    public String getHostServiceURL() {
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        String strHostURL = Prefs.getString("HostURL", "");
        return strHostURL + "/OneClickShot/WEB";
    }
}
