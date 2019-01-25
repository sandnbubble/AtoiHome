package com.atoihome.oneclick;

import android.app.ProgressDialog;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.preference.PreferenceManager;
import android.util.Log;
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

public class NetworkUtils {

    public interface AsyncResponse{
        void processFinish(Object HttpResponseMessage);
    }

    public class HttpMessage{
        int iRet;
        String Message;
    }

    Context mContext;
    protected  void validateTokenRequest(String strAccessToken, Context pContext, AsyncResponse asnycResponse)
    {
        mContext = pContext;
        HttpGetRequest InvokeHttpGetRequest = new HttpGetRequest(asnycResponse);
        InvokeHttpGetRequest.execute("https://www.atoihome.site/api/RestAccount/ValidateToken", strAccessToken);
    }

    protected  void getTokenRequest(String strUserInfo, Context pContext, AsyncResponse asnycResponse)
    {
        mContext = pContext;
        HttpPostRequest InvokeHttpPostRequest = new HttpPostRequest(asnycResponse);
        InvokeHttpPostRequest.execute("https://www.atoihome.site/token", strUserInfo);
    }

    protected  void signupRequest(String strUserInfo, Context pContext, AsyncResponse asnycResponse)
    {
        mContext = pContext;
        HttpPostRequest InvokeHttpPostRequest = new HttpPostRequest(asnycResponse);
        InvokeHttpPostRequest.execute("https://www.atoihome.site/api/restaccount/signup", strUserInfo);
    }

    protected class  HttpGetRequest extends AsyncTask<String, String, Object> {
        //생성자
        public HttpGetRequest(AsyncResponse asyncResponse){
            delegate = asyncResponse;
        }

        public AsyncResponse delegate = null;
        final ProgressDialog progressDialog = new ProgressDialog(mContext, R.style.AppTheme_Dark_Dialog);
    @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override
        protected void onPreExecute(){
            super.onPreExecute();
            progressDialog.setIndeterminate(true);
            progressDialog.setMessage("Authenticating...");
            progressDialog.show();
        }

        @Override
        protected void onPostExecute(Object msgRet)
        {
            super.onPostExecute(msgRet);
            delegate.processFinish(msgRet);
            progressDialog.dismiss();
        }

        @Override
        protected HttpMessage doInBackground(String... params) {
            HttpMessage msgResponse = new HttpMessage();
            try {
                URL url = new URL(params[0]);
                HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                connection.setRequestMethod("GET");
                connection.setDoInput(true);
                connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
                String strAuthorization = "bearer " +  params[1];
                connection.setRequestProperty("Authorization", strAuthorization);
                connection.setConnectTimeout(10000);

                InputStream is;
                msgResponse.iRet = connection.getResponseCode();
                if ( msgResponse.iRet == 200) {
                    is = connection.getInputStream();
                } else {
                    /* error from server */
                    is = connection.getErrorStream();
                }

//                InputStream is = connection.getInputStream();
                BufferedReader br = new BufferedReader(new InputStreamReader(is));

                String readLine = null;
                StringBuilder sbInput = new StringBuilder();

                while ((readLine = br.readLine()) != null) {
                    System.out.println(readLine);
                    sbInput.append(readLine+"\n");
                }
                br.close();
                msgResponse.Message = sbInput.toString();
            } catch (MalformedURLException e) {
                // TODO Auto-generated catch block
                msgResponse.iRet = -2;
                msgResponse.Message = e.getMessage();
                e.printStackTrace();
            } catch (IOException e) {
                // TODO Auto-generated catch block
                msgResponse.iRet = -3;
                msgResponse.Message = e.getMessage();
                e.printStackTrace();
            }catch (Exception e){
                msgResponse.iRet = -4;
                msgResponse.Message = e.getMessage();
                e.printStackTrace();
            }
            return msgResponse;
        }
    }

    public class HttpPostRequest extends AsyncTask<String, String, Object> {
        public HttpPostRequest(AsyncResponse asyncResponse){
            delegate = asyncResponse;
        }
        public AsyncResponse delegate = null;
        final ProgressDialog progressDialog = new ProgressDialog(mContext, R.style.AppTheme_Dark_Dialog);
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
        protected void onPostExecute(Object object) {
            super.onPostExecute(object);
            delegate.processFinish(object);
            progressDialog.dismiss();
        }

        @Override
        protected HttpMessage doInBackground(String... params) {
            HttpMessage msgResponse = new HttpMessage();
            HttpURLConnection connection;
            try {
                URL url = new URL(params[0]);
                connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);//Allow Inputs
                connection.setDoOutput(true);//Allow Outputs
                connection.setUseCaches(false);//Don't use a cached Copy
                connection.setRequestMethod("POST");
//                connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
                connection.setRequestProperty("Content-Type", "application/json");
                connection.setConnectTimeout(10000);

                OutputStream os = connection.getOutputStream();
                BufferedWriter writer = new BufferedWriter(new OutputStreamWriter(os, "UTF-8"));

                writer.write(params[1]);
                writer.flush();
                writer.close();
                os.close();

                InputStream is;
                msgResponse.iRet = connection.getResponseCode();
                if ( msgResponse.iRet == 200) {
                    is = connection.getInputStream();
                }else {
                /* error from server */
                is = connection.getErrorStream();
                }
                BufferedReader br = new BufferedReader(new InputStreamReader(is));
                String readLine = null;
                StringBuilder sbInput = new StringBuilder();

                while ((readLine = br.readLine()) != null) {
                    System.out.println(readLine);
                    sbInput.append(readLine+"\n");
                }
                msgResponse.Message = sbInput.toString();

//                    JSONObject jObject = new JSONObject(result);
//                    String AccessToken = jObject.getString("access_token");
//                    String TokenType = jObject.getString("token_type");
//                    String ExpiresIn = jObject.getString("expires_in");
                    // 억세스 토큰을 SharePreference에 저장
//                    SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(mContext);
//                    SharedPreferences.Editor editor = Prefs.edit();
//                    editor.putString("AccessToken", jObject.getString("access_token"));
//                    editor.commit();
            } catch (FileNotFoundException e) {
                msgResponse.iRet = -1;
                msgResponse.Message = e.getMessage();
            } catch (MalformedURLException e) {
                msgResponse.iRet = -2;
                msgResponse.Message = e.getMessage();
            } catch (IOException e) {
                msgResponse.iRet = -3;
                msgResponse.Message = e.getMessage();
            } catch (Exception e) {
                msgResponse.iRet = -4;
                msgResponse.Message = e.getMessage();
            }
            return msgResponse;
        }
    }
}
