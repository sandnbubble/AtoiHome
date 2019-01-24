package com.atoihome.oneclick;

import android.Manifest;
import android.app.ActivityManager;
import android.app.Service;
import android.app.usage.UsageStats;
import android.app.usage.UsageStatsManager;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.database.ContentObserver;
import android.database.Cursor;
import android.net.Uri;
import android.os.Binder;
import android.os.Build;
import android.os.Handler;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.provider.MediaStore;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.widget.Toast;

import java.util.List;
import java.util.SortedMap;
import java.util.TreeMap;

import static android.widget.Toast.LENGTH_SHORT;

public class ScreenshotDetectionService extends Service {
    // Binder given to clients
    private final IBinder binder = new LocalBinder();
    // Registered callbacks
    private ServiceCallbacks serviceCallbacks;
    private static String AccessToken = "";

    public void SetAccessString (String accessToken) {
        AccessToken = accessToken;
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
//            Toast.makeText(getApplicationContext(), getHostServiceURL()+strFilePath, LENGTH_SHORT).show();
        }
        else{
            try {
                Intent sendIntent = new Intent();
                sendIntent.setAction(Intent.ACTION_SEND);
                sendIntent.putExtra(Intent.EXTRA_STREAM, Uri.parse(strFilePath));
                sendIntent.setType("image/png");

                Intent chooser = Intent.createChooser(sendIntent, "OneClickShot");
                // Verify that the intent will resolve to an activity
                if (sendIntent.resolveActivity(getPackageManager()) != null) {
                    startActivity(chooser);
                }
            } catch (Exception e){
                e.printStackTrace();
            }
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
        return "http://"+Prefs.getString("ServiceHost", "") + "/OneClickShot/WEB";
    }
}
