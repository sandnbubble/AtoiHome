package com.atoihome.oneclick;

import android.app.ProgressDialog;
import android.content.Context;
import android.os.AsyncTask;
import android.os.Handler;
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

public final class NetworkUtils {
    public interface AsyncResponse{
        void processFinish(int output);
    }


    public static Integer ValidateTokenRequest(String strAccessToken, Context pContext)
    {
        mContext = pContext;
        HttpGetRequest InvokeHttpGetRequest = new HttpGetRequest(new AsyncResponse(){
            @Override
            public void processFinish(int output) {
                if (output == 200){
                }else{
                }
            }
        });
        try{
            int nRet = InvokeHttpGetRequest.execute("https://www.atoihome.site/api/RestAccount/ValidateToken", strAccessToken).get();
            return nRet;
        }catch(Exception e){
        }
        return -1;
    }

    static Context mContext;
    protected static class  HttpGetRequest extends AsyncTask<String, String, Integer> {
        public AsyncResponse delegate = null;
        public HttpGetRequest(AsyncResponse asyncResponse){
            delegate = asyncResponse;
        }
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
        protected void onPostExecute(Integer iRet)
        {
            super.onPostExecute(iRet);
            delegate.processFinish(iRet);
            if (iRet == 200) {
            }
            else{
            }
            progressDialog.dismiss();
        }

        @Override
        protected Integer doInBackground(String... params) {
            int iRet=200;
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
                int retCode = connection.getResponseCode();
                if ( retCode == 200) {
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
                iRet = retCode;
            } catch (MalformedURLException e) {
                // TODO Auto-generated catch block
                iRet = -2;
                e.printStackTrace();
            } catch (IOException e) {
                // TODO Auto-generated catch block
                iRet = -3;
                e.printStackTrace();
            }catch (Exception e){
                iRet = -4;
                e.printStackTrace();
            }
            return iRet;
        }
    }

    public static class HttpPostRequest extends AsyncTask<String, String, Integer> {
        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override protected void onPreExecute() {
            super.onPreExecute();
        }

        @Override protected void onProgressUpdate(String... progress) {
        }

        @Override
        protected void onPostExecute(Integer iRet) {
            super.onPostExecute(iRet);
            if (iRet == 0) {
            }
            else{
            }
        }
        @Override
        protected Integer doInBackground(String... params) {

            Integer iRet = 0;
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
                        sbInput.append(readLine + "\n");
                    }
                    String result = sbInput.toString();
                    JSONObject jObject = new JSONObject(result);
//                    String AccessToken = jObject.getString("access_token");
//                    String TokenType = jObject.getString("token_type");
//                    String ExpiresIn = jObject.getString("expires_in");
                    iRet = 0;
//                    Log.d("OneClickDebug", AccessToken+TokenType+ExpiresIn);
                } else {
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
