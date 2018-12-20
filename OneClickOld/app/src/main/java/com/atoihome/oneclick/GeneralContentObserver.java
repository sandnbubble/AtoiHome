package com.atoihome.oneclick;

import android.database.ContentObserver;
import android.net.Uri;
import android.os.Handler;
import android.util.Log;

public class GeneralContentObserver extends ContentObserver {

    private ContentObserverCallback contentObserverCallback;

    public GeneralContentObserver(Handler handler, ContentObserverCallback onGeneralCallback) {
        // null is totally fine here
        super(handler);
        contentObserverCallback = onGeneralCallback;
    }

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
        contentObserverCallback.onUpdate(uri);
        Log.d("GeneralContentObserver", "Content changed");
    }
}