package com.atoihome.oneclick;

import android.app.ProgressDialog;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;

import android.content.Intent;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import butterknife.BindView;
import butterknife.ButterKnife;

import static android.widget.Toast.LENGTH_SHORT;

public class LoginActivity extends AppCompatActivity {
    private static final String TAG = "LoginActivity";
    private static final int REQUEST_SIGNUP = 0;

    @BindView(R.id.input_email) EditText _emailText;
    @BindView(R.id.input_password) EditText _passwordText;
    @BindView(R.id.btn_login) Button _loginButton;
    @BindView(R.id.link_signup) TextView _signupLink;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        ButterKnife.bind(this);

        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        _emailText.setText(Prefs.getString("Email", ""));
        _passwordText.setText(Prefs.getString("Password", ""));
        _loginButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                login();
            }
        });
        _signupLink.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                // Start the Signup activity
                Intent intent = new Intent(getApplicationContext(), SignupActivity.class);
                startActivityForResult(intent, REQUEST_SIGNUP);
                finish();
                overridePendingTransition(R.anim.push_left_in, R.anim.push_left_out);
            }
        });
    }

    public void login() {
        Log.d(TAG, "Login");

        if (!validate()) {
            onLoginFailed();
            return;
        }

        _loginButton.setEnabled(false);

        String email = _emailText.getText().toString();
        String password = _passwordText.getText().toString();

        // TODO: Implement your own authentication logic here.
        String UserInfo = "grant_type=password&username="+email+"&password="+password;
        NetworkUtils.AsyncResponse asyncResponse =    new NetworkUtils.AsyncResponse(){
            @Override
            public void processFinish(Object msgRet) {
                try{
                    NetworkUtils.HttpMessage ResponseMsg = (NetworkUtils.HttpMessage)msgRet;
                    Toast.makeText(getApplicationContext(), "RetCode = " + ResponseMsg.iRet + " "+ResponseMsg.Message, LENGTH_SHORT).show();
                    if (ResponseMsg.iRet == 200){
                        JSONObject jObject = new JSONObject(ResponseMsg.Message);
                        String AccessToken = jObject.getString("access_token");
                        String TokenType = jObject.getString("token_type");
                        String ExpiresIn = jObject.getString("expires_in");
//                        억세스 토큰을 SharePreference에 저장
                        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(getBaseContext());
                        SharedPreferences.Editor editor = Prefs.edit();
                        editor.putString("AccessToken", jObject.getString("access_token"));
                        editor.commit();
                        onLoginSuccess();
                    }else{
                        onLoginFailed();
                    }
                }catch (Exception e){
                    Toast.makeText(getBaseContext(), e.getMessage(), LENGTH_SHORT);
                }
            }
        };
        NetworkUtils networkUtils = new NetworkUtils();
        networkUtils.getTokenRequest(UserInfo, this, asyncResponse);
    }


    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == REQUEST_SIGNUP) {
            if (resultCode == RESULT_OK) {

                // TODO: Implement successful signup logic here
                // By default we just finish the Activity and log them in automatically
                this.finish();
            }
            else{
            }
        }
    }

    @Override
    public void onBackPressed() {
        // Disable going back to the MainActivity
        //현재 Task가 백그라운드로 이동하게된다.
        moveTaskToBack(true);
    }

    public void onLoginSuccess() {
        _loginButton.setEnabled(true);

        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        SharedPreferences.Editor editor = Prefs.edit();
        editor.putString("Email", _emailText.getText().toString());
        editor.putString("Password", _passwordText.getText().toString());
        editor.putString("AuthServer", getString(R.string.AuthServer));
        editor.commit();
        com.atoihome.oneclick.MainActivity.startProcess(true);

        finish();
    }

    public void onLoginFailed() {
        Toast.makeText(getBaseContext(), "Login failed", Toast.LENGTH_LONG).show();
        _loginButton.setEnabled(true);
        com.atoihome.oneclick.MainActivity.startProcess(false);
    }

    public boolean validate() {
        boolean valid = true;
        String email = _emailText.getText().toString();
        String password = _passwordText.getText().toString();

        if (email.isEmpty() || !android.util.Patterns.EMAIL_ADDRESS.matcher(email).matches()) {
            _emailText.setError("enter a valid email address");
            valid = false;
        } else {
            _emailText.setError(null);
        }

        if (password.isEmpty() || password.length() < 4 || password.length() > 14) {
            _passwordText.setError("between 4 and 14 alphanumeric characters");
            valid = false;
        } else {
            _passwordText.setError(null);
        }

        return valid;
    }
}
