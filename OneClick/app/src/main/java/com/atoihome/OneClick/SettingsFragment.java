package com.atoihome.OneClick;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.EditTextPreference;
import android.preference.PreferenceFragment;
import android.preference.PreferenceManager;
import android.preference.SwitchPreference;
import android.widget.BaseAdapter;

public class SettingsFragment extends PreferenceFragment {
    SharedPreferences Prefs;
    public EditTextPreference PrefEmail, PrefPassword, PrefHostURL;
    public SwitchPreference PrefAutomaticUpload, PrefUploadCameraShot, PrefServiceStartus ;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        addPreferencesFromResource(R.xml.prefs);
        try
        {
            PrefEmail = (EditTextPreference)findPreference("Email");
            PrefPassword = (EditTextPreference)findPreference("Password");
            PrefHostURL = (EditTextPreference)findPreference("HostURL");
            PrefAutomaticUpload = (SwitchPreference)findPreference("AutomaticUpload");
            PrefUploadCameraShot = (SwitchPreference)findPreference("UploadCameraShot");
            PrefServiceStartus = (SwitchPreference)findPreference("ServiceStatus");
            Prefs = PreferenceManager.getDefaultSharedPreferences(getActivity());

            PrefEmail.setSummary(PrefEmail.getText());
            int iLenPassword = PrefPassword.getText().length();
            String strHiddenPassword = String.format("%0" + iLenPassword + "d", 0).replace('0', '*');
            PrefPassword.setSummary(strHiddenPassword);
            PrefHostURL.setSummary(PrefHostURL.getText());

            if(Prefs.getBoolean("AutomaticUpload", true)){
                PrefAutomaticUpload.setSummary("use");
            }else{
                PrefAutomaticUpload.setSummary("not use");
            }
            if(Prefs.getBoolean("UploadCameraShot", false)){
                PrefUploadCameraShot.setSummary("use");
            }else{
                PrefUploadCameraShot.setSummary("not use");
            }

            if(Prefs.getBoolean("ServiceStatus", true)){
                PrefUploadCameraShot.setSummary("use");
            }else{
                PrefUploadCameraShot.setSummary("not use");
            }

            Prefs.registerOnSharedPreferenceChangeListener(prefListener);
        } catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    SharedPreferences.OnSharedPreferenceChangeListener prefListener = new SharedPreferences.OnSharedPreferenceChangeListener() {
        @Override
        public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {
            if (key.equals("Email")){
                PrefEmail.setSummary(PrefEmail.getText());
            }
            if (key.equals("Password")){
                int iLenPassword = PrefPassword.getText().length();
                String strHiddenPassword = String.format("%0" + iLenPassword + "d", 0).replace('0', '*');
                PrefPassword.setSummary(strHiddenPassword);
            }
            if (key.equals("HostURL")){
                PrefHostURL.setSummary(PrefHostURL.getText());
            }

            if(key.equals("AutomaticUpload")){
                if(Prefs.getBoolean("AutomaticUpload", true)){
                    PrefAutomaticUpload.setSummary("use");

                }else{
                    PrefAutomaticUpload.setSummary("not use");
                }
                ((BaseAdapter)getPreferenceScreen().getRootAdapter()).notifyDataSetChanged();
            }

            if(key.equals("UploadCameraShot")){
                if(Prefs.getBoolean("UploadCameraShot", false)){
                    PrefUploadCameraShot.setSummary("use");

                }else{
                    PrefUploadCameraShot.setSummary("not use");
                }
                ((BaseAdapter)getPreferenceScreen().getRootAdapter()).notifyDataSetChanged();
            }
        }
    };
}
