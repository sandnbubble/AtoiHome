package com.atoihome.oneclick;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.EditTextPreference;
import android.preference.PreferenceCategory;
import android.preference.PreferenceFragment;
import android.preference.PreferenceManager;
import android.preference.PreferenceScreen;
import android.preference.SwitchPreference;
import android.widget.BaseAdapter;

public class SettingsFragment extends PreferenceFragment {
    SharedPreferences Prefs;
    PreferenceScreen PreferenceScreen = (PreferenceScreen) findPreference("PfScreen");
    public EditTextPreference PrefEmail, PrefPassword, PrefAccessToken, PrefAuthServer, PrefServiceHost;
    public SwitchPreference PrefAutomaticUpload, PrefUploadCameraShot, PrefServiceAutoStart;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        addPreferencesFromResource(R.xml.prefs);
        try
        {
            PreferenceCategory AuthInfoCategory = (PreferenceCategory) findPreference("NetworkInfo");
            PrefEmail = (EditTextPreference)findPreference("Email");
            PrefPassword = (EditTextPreference)findPreference("Password");
            PrefAccessToken = (EditTextPreference)findPreference(getString(R.string.app_AccessToken));
            AuthInfoCategory.removePreference(PrefEmail);
            AuthInfoCategory.removePreference(PrefPassword);
//            AuthInfoCategory.removePreference(PrefAccessToken);
            PrefAuthServer = (EditTextPreference)findPreference("AuthServer");
            PrefServiceHost = (EditTextPreference)findPreference("ServiceHost");
            PrefAutomaticUpload = (SwitchPreference)findPreference("AutomaticUpload");
            PrefUploadCameraShot = (SwitchPreference)findPreference("UploadCameraShot");
            PrefServiceAutoStart = (SwitchPreference)findPreference("AutomaticStart");
            Prefs = PreferenceManager.getDefaultSharedPreferences(getActivity());

//            PrefEmail.setSummary(PrefEmail.getText());
//            int iLenPassword = PrefPassword.getText().length();
//            String strHiddenPassword = String.format("%0" + iLenPassword + "d", 0).replace('0', '*');
//            PrefPassword.setSummary(strHiddenPassword);
            PrefAuthServer.setSummary(getString(R.string.AuthServer));
            PrefServiceHost.setSummary(PrefServiceHost.getText());

            if(Prefs.getBoolean("AutomaticUpload", true)){
                PrefAutomaticUpload.setSummary("use");
            }else{
                PrefAutomaticUpload.setSummary("not use");
            }
            if(Prefs.getBoolean("UploadCameraShot", true)){
                PrefUploadCameraShot.setSummary("use");
            }else{
                PrefUploadCameraShot.setSummary("not use");
            }

            if(Prefs.getBoolean("ServiceStatus", true)){
                PrefServiceAutoStart.setSummary("use");
            }else{
                PrefServiceAutoStart.setSummary("not use");
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
//            if (key.equals("Email")){
//                PrefEmail.setSummary(PrefEmail.getText());
//            }
//            if (key.equals("Password")){
//                int iLenPassword = PrefPassword.getText().length();
//                String strHiddenPassword = String.format("%0" + iLenPassword + "d", 0).replace('0', '*');
//                PrefPassword.setSummary(strHiddenPassword);
//            }
            if (key.equals("ServiceHost")){
                PrefServiceHost.setSummary(PrefServiceHost.getText());
            }

            if(key.equals("AutomaticUpload")){
                if(Prefs.getBoolean("AutomaticUpload", false)){
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

            if(key.equals("AutomaticStart")){
                if(Prefs.getBoolean("AutomaticStart", false)){
                    PrefServiceAutoStart.setSummary("use");

                }else{
                    PrefServiceAutoStart.setSummary("not use");
                }
                ((BaseAdapter)getPreferenceScreen().getRootAdapter()).notifyDataSetChanged();
            }

        }
    };
}
