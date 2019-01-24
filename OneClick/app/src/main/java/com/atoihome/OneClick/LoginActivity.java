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
        HttpPost InvokeSignIn = new HttpPost();
        InvokeSignIn.execute("https://www.atoihome.site/token", UserInfo);
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

        finish();
    }

    public void onLoginFailed() {
        Toast.makeText(getBaseContext(), "Login failed", Toast.LENGTH_LONG).show();
        _loginButton.setEnabled(true);
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

        if (password.isEmpty() || password.length() < 4 || password.length() > 10) {
            _passwordText.setError("between 4 and 10 alphanumeric characters");
            valid = false;
        } else {
            _passwordText.setError(null);
        }

        return valid;
    }

    protected class  HttpGetTest extends AsyncTask<String, Void, String> {
        private String result;

        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override protected void onPreExecute() { super.onPreExecute(); }

        @Override
        protected void onPostExecute(String result)
        {
            super.onPostExecute(result);

            if(result != null){
                Log.d("ASYNC", "result = " + result);
            }
        }

        @Override
        protected String doInBackground(String... params) {
            try {
                URL url = new URL(params[0]+"GetData?arg=this is echo string written by atoi");
                HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                connection.setRequestMethod("GET");
                connection.setDoInput(true);
                InputStream is = connection.getInputStream();
                BufferedReader br = new BufferedReader(new InputStreamReader(is));

                String readLine = null;
                StringBuilder sbInput = new StringBuilder();

                while ((readLine = br.readLine()) != null) {
                    System.out.println(readLine);
                    sbInput.append(readLine+"\n");
                }
                result = sbInput.toString();
                br.close();
                return result;

            } catch (MalformedURLException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            } catch (IOException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }
            return null;
        }
    }
    public class HttpPost extends AsyncTask<String, String, Integer> {
        final ProgressDialog progressDialog = new ProgressDialog(LoginActivity.this, R.style.AppTheme_Dark_Dialog);
        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override protected void onPreExecute() {
            super.onPreExecute();
            progressDialog.setIndeterminate(true);
            progressDialog.setMessage("Authenticating...");
            progressDialog.show();
        }

        @Override protected void onProgressUpdate(String... progress) {
        }

        @Override
        protected void onPostExecute(Integer iRet) {
            super.onPostExecute(iRet);
            if (iRet == 0) {
                onLoginSuccess();
            }
            else{
                onLoginFailed();
            }
            progressDialog.dismiss();
        }

        @Override
        protected Integer doInBackground(String... params) {

            Integer iRet=0;
            int serverResponseCode = 0;

            HttpURLConnection connection;
            DataOutputStream dataOutputStream;

            try {
                URL url = new URL(params[0]);
                connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);//Allow Inputs
                connection.setDoOutput(true);//Allow Outputs
                connection.setUseCaches(false);//Don't use a cached Copy
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
                connection.setConnectTimeout(10000);

                OutputStream os = connection.getOutputStream();
                BufferedWriter writer = new BufferedWriter(new OutputStreamWriter(os, "UTF-8"));

                writer.write(params[1]);
                writer.flush();
                writer.close();
                os.close();

                serverResponseCode = connection.getResponseCode();
                final String serverResponseMessage = connection.getResponseMessage();

                Log.i("Debug", "Server Response is: " + serverResponseMessage + ": " + serverResponseCode);

                //response code of 200 indicates the server status OK
                if (serverResponseCode == 200) {
                    InputStream is = connection.getInputStream();
                    BufferedReader br = new BufferedReader(new InputStreamReader(is));

                    String readLine = null;
                    StringBuilder sbInput = new StringBuilder();

                    while ((readLine = br.readLine()) != null) {
                        System.out.println(readLine);
                        sbInput.append(readLine+"\n");
                    }
                    String result = sbInput.toString();
                    JSONObject jObject = new JSONObject(result);
//                    String AccessToken = jObject.getString("access_token");
//                    String TokenType = jObject.getString("token_type");
//                    String ExpiresIn = jObject.getString("expires_in");
                    // 억세스 토큰을 SharePreference에 저장
                    SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(getApplicationContext());
                    SharedPreferences.Editor editor = Prefs.edit();
                    editor.putString("AccessToken", jObject.getString("access_token"));
                    editor.commit();
                    iRet = 0;
//                    Log.d("OneClickDebug", AccessToken+TokenType+ExpiresIn);
                }
                else {
                    iRet = -1;
                }
            } catch (FileNotFoundException e) {
                iRet = -1;
            } catch (MalformedURLException e) {
                e.printStackTrace();
                iRet = -2;
            } catch (IOException e) {
                iRet = -3;
                e.printStackTrace();
            } catch (Exception e) {
                iRet = -4;
                e.printStackTrace();
            }
            return iRet;
        }
    }
}
