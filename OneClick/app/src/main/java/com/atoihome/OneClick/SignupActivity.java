package com.atoihome.oneclick;

import android.app.ProgressDialog;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import butterknife.BindView;
import butterknife.ButterKnife;

public class SignupActivity extends AppCompatActivity {
    private static final String TAG = "SignupActivity";

//    @BindView(R.id.input_name) EditText _nameText;
//    @BindView(R.id.input_address) EditText _addressText;
//    @BindView(R.id.input_mobile) EditText _mobileText;
    @BindView(R.id.input_email) EditText _emailText;
    @BindView(R.id.input_password) EditText _passwordText;
    @BindView(R.id.input_reEnterPassword) EditText _reEnterPasswordText;
    @BindView(R.id.btn_signup) Button _signupButton;
    @BindView(R.id.link_login) TextView _loginLink;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_signup);
        ButterKnife.bind(this);

//        _nameText.setText("tester");
//        _addressText.setText("ROK");
//        _mobileText.setText("000-0000-0000");
        _emailText.setText("tester@atoihome.site");
        _passwordText.setText("1234567890");
        _reEnterPasswordText.setText("1234567890");

        _signupButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                signup();
            }
        });

        _loginLink.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Finish the registration screen and return to the Login activity
                Intent intent = new Intent(getApplicationContext(),LoginActivity.class);
                startActivity(intent);
                finish();
                overridePendingTransition(R.anim.push_left_in, R.anim.push_left_out);
            }
        });
    }

    public void signup() {
        Log.d(TAG, "Signup");

        if (!validate()) {
            onSignupFailed();
            return;
        }

        _signupButton.setEnabled(false);

//        String name = _nameText.getText().toString();
//        String address = _addressText.getText().toString();
//        String mobile = _mobileText.getText().toString();
        String email = _emailText.getText().toString();
        String password = _passwordText.getText().toString();
        String reEnterPassword = _reEnterPasswordText.getText().toString();

        // TODO: Implement your own signup logic here.
        String UserInfo = "{\"Email\":\""+email+"\",\"Password\":\"" + password+"\",\"ConfirmPassword\":\""+ reEnterPassword+"\"}";
        SignUp InvokeSignUp = new SignUp();
        InvokeSignUp.execute("https://www.atoihome.site/api/restaccount/signup", UserInfo);
    }


    public void onSignupSuccess() {
        _signupButton.setEnabled(true);
        setResult(RESULT_OK, null);

        // 여기에 계정생성이 성공한 후 처리를 한다. SignUp에서 할 수도 있지만
        // SignIn, Signup을 하나로 통할 할 경우를 종속성을 갖지 않아야한다.
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(this);
        SharedPreferences.Editor editor = Prefs.edit();
        editor.putString("Email", _emailText.getText().toString());
        editor.putString("Password", _passwordText.getText().toString());
        editor.putString("HostURL", "www.atoihome.site");
        editor.commit();
        finish();
    }

    public void onSignupFailed() {
        Toast.makeText(getBaseContext(), "Creating failed", Toast.LENGTH_LONG).show();
        _signupButton.setEnabled(true);
    }

    public boolean validate() {
        boolean valid = true;

        String email = _emailText.getText().toString();
        String password = _passwordText.getText().toString();
        String reEnterPassword = _reEnterPasswordText.getText().toString();
//        String name = _nameText.getText().toString();
//        String address = _addressText.getText().toString();
//        String mobile = _mobileText.getText().toString();

//        if (name.isEmpty() || name.length() < 3) {
//            _nameText.setError("at least 3 characters");
//            valid = false;
//        } else {
//            _nameText.setError(null);
//        }

//        if (address.isEmpty()) {
//            _addressText.setError("Enter Valid Address");
//            valid = false;
//        } else {
//            _addressText.setError(null);
//        }

//        if (mobile.isEmpty()) {
//            _mobileText.setError("Enter Valid Mobile Number");
//            valid = false;
//        } else {
//            _mobileText.setError(null);
//        }


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

        if (reEnterPassword.isEmpty() || reEnterPassword.length() < 4 || reEnterPassword.length() > 14 || !(reEnterPassword.equals(password))) {
            _reEnterPasswordText.setError("Password Do not match");
            valid = false;
        } else {
            _reEnterPasswordText.setError(null);
        }
        return valid;
    }
    public class SignUp extends AsyncTask<String, String, Integer> {
        final ProgressDialog progressDialog = new ProgressDialog(SignupActivity.this, R.style.AppTheme_Dark_Dialog);
        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override protected void onPreExecute() {
            super.onPreExecute();
            progressDialog.setIndeterminate(true);
            progressDialog.setMessage("Creating account...");
            progressDialog.show();
        }

        @Override protected void onProgressUpdate(String... progress) {
        }

        @Override
        protected void onPostExecute(Integer iRet) {
            super.onPostExecute(iRet);
            if (iRet == 0) {
                onSignupSuccess();
            }
            else{
                onSignupFailed();
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
                connection.setRequestProperty("Content-Type", "application/json");
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
                    iRet = 0;
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