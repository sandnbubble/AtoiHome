package com.atoihome.oneclick;
import android.app.Activity;
import android.widget.Toast
        ;
public class BackPressCloseHandler {

    private long backKeyPressedTime = 0;
    private Toast toast;
    private Activity activity;

    public BackPressCloseHandler(Activity context) {
        this.activity = context;
    }

    public boolean onBackPressed() {
        if (System.currentTimeMillis() > backKeyPressedTime + 2000) {
            backKeyPressedTime = System.currentTimeMillis();
            showGuide();
            return false;
        }
        if (System.currentTimeMillis() <= backKeyPressedTime + 2000) {
//            activity.finish();
//            toast.cancel();
            return true;
        }
        return false;
    }

    public void showGuide() {
        Toast.makeText(activity, "\'뒤로\'버튼을 한번 더 누르시면 종료됩니다.", Toast.LENGTH_SHORT).show();
    }

    public interface exitDelegate {
        void onExit();
        void onShow();
    }
}

