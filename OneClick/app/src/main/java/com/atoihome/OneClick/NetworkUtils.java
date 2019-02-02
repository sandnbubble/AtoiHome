package com.atoihome.oneclick;

import android.app.ProgressDialog;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.preference.PreferenceManager;
import android.util.Log;
import android.widget.Toast;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import static android.widget.Toast.LENGTH_SHORT;

public class NetworkUtils {
    Context mContext;
    String mAuthServer, mServiceHost, mAccessToken;


    public NetworkUtils (Context pContext)
    {
        mContext = pContext;
        SharedPreferences Prefs = PreferenceManager.getDefaultSharedPreferences(mContext);
        mAuthServer = mContext.getString(R.string.AuthServer);
        mServiceHost = Prefs.getString("ServiceHost", "");
        mAccessToken = Prefs.getString("AccessToken", "");
    }

    public interface AsyncResponse{
        void processFinish(Object HttpResponseMessage);
    }

    public class HttpMessage{
        int iRet;
        String Message;
    }

    protected  void validateTokenRequest(AsyncResponse asnycResponse)
    {
        HttpGetRequest InvokeHttpGetRequest = new HttpGetRequest(asnycResponse);
        InvokeHttpGetRequest.execute("https://"+mAuthServer+"/api/RestAccount/ValidateToken");
    }

    protected  void getTokenRequest(String strUserInfo, AsyncResponse asnycResponse)
    {
        HttpPostRequest InvokeHttpPostRequest = new HttpPostRequest(asnycResponse);
        InvokeHttpPostRequest.execute("https://"+mAuthServer+"/token", strUserInfo);
    }

    protected  void signupRequest(String strUserInfo, AsyncResponse asnycResponse)
    {
        HttpPostRequest InvokeHttpPostRequest = new HttpPostRequest(asnycResponse);
        InvokeHttpPostRequest.execute("https://"+mAuthServer+"/api/restaccount/signup", strUserInfo);
    }

    protected  void uploadFileRequest(String strFilename, AsyncResponse asnycResponse)
    {
        UploadFileRequest invokeUploadFiletRequest = new UploadFileRequest(asnycResponse);
        invokeUploadFiletRequest.execute("http://"+ mServiceHost + "/oneclickshot/web/uploadimage/", strFilename);
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
                String strAuthorization = "bearer " +  mAccessToken;
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

    protected class HttpPostRequest extends AsyncTask<String, String, Object> {
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

    // POST 메서드를 사용해서 파일을 업로드한다. HttpPostRequest와 통합하는 것이 쉽지 않다 :(
    // ProgressDlg는 디폴트를 사용해서 업로드한 퍼센트를 보여주는 것으로 한다.
    public class UploadFileRequest extends AsyncTask<String, String, Object> {
        public UploadFileRequest(AsyncResponse asyncResponse){
            delegate = asyncResponse;
        }
        public AsyncResponse delegate = null;
        private ProgressDialog ProgressDlg;

        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override
        protected void onPreExecute() {
            super.onPreExecute();
            ProgressDlg = new ProgressDialog(mContext);
            ProgressDlg.setProgressStyle(ProgressDialog.STYLE_HORIZONTAL);
            ProgressDlg.setMessage("Uploading...");
            ProgressDlg.setCancelable(false);
        }

        @Override
        protected void onProgressUpdate(String... progress) {
            if (progress[0].equals("progress")) {
                ProgressDlg.setProgress(Integer.parseInt(progress[1]));
                ProgressDlg.setMessage(progress[2]);
            } else if (progress[0].equals("max")) {
                ProgressDlg.setMax(Integer.parseInt(progress[1]));
            }
        }

        @Override
        protected void onPostExecute(Object object) {
            super.onPostExecute(object);
            delegate.processFinish(object);
            ProgressDlg.dismiss();
        }

        @Override
        protected HttpMessage doInBackground(String... params) {
            HttpMessage msgResponse = new HttpMessage();
            HttpURLConnection connection;
            DataOutputStream dataOutputStream;

            int bytesRead, bytesAvailable, bufferSize;
            byte[] buffer;
            int maxBufferSize = 1024 * 1024;
            File selectedFile = new File(params[1]);

            String[] parts = params[1].split("/");
            final String strfileName = parts[parts.length - 1];

            try {
                URL url = new URL(params[0] + strfileName);
                connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);//Allow Inputs
                connection.setDoOutput(true);//Allow Outputs
                connection.setUseCaches(false);//Don't use a cached Copy
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/octet-stream");
                connection.setRequestProperty("Upload-Filename", strfileName);

                connection.setRequestProperty("Authorization", "bearer " + mAccessToken);
                connection.setConnectTimeout(10000);

                FileInputStream fileInputStream = new FileInputStream(selectedFile);
                //creating new dataoutputstream
                dataOutputStream = new DataOutputStream(connection.getOutputStream());

                //returns no. of bytes present in fileInputStream
                bytesAvailable = fileInputStream.available();
                //selecting the buffer size as minimum of available bytes or 1 MB
                bufferSize = Math.min(bytesAvailable, maxBufferSize);

                int nMaxLoopCount = bytesAvailable / bufferSize;
                //setting the buffer as byte array of size of bufferSize
                buffer = new byte[bufferSize];
                //reads bytes from FileInputStream(from 0th index of buffer to buffersize)
                bytesRead = fileInputStream.read(buffer, 0, bufferSize);

                //loop repeats till bytesRead = -1, i.e., no bytes are left to read
                int iProgress = 0, iCurCount = 0;
                while (bytesRead > 0) {
                    //write the bytes read from inputstream
                    dataOutputStream.write(buffer, 0, bufferSize);
                    bytesAvailable = fileInputStream.available();
                    bufferSize = Math.min(bytesAvailable, maxBufferSize);
                    bytesRead = fileInputStream.read(buffer, 0, bufferSize);
                    iCurCount += 1;
                    // 카메라에서 스크린 샷을 하면 150%까지 나오는 경우가 있어서 일단 회피함
                    iProgress = (int) Math.min(((double) iCurCount / (double) nMaxLoopCount) * 100.0, 100);
                    publishProgress("progress", Integer.toString(iProgress), "Transfered" + Integer.toString(iProgress) + "%");
                }
                fileInputStream.close();
                dataOutputStream.flush();
                dataOutputStream.close();

                // 서버 응답 처리
                InputStream is;
                msgResponse.iRet = connection.getResponseCode();
                if (msgResponse.iRet == 200) {
                    is = connection.getInputStream();
                } else {
                    /* error from server */
                    is = connection.getErrorStream();
                }
                BufferedReader br = new BufferedReader(new InputStreamReader(is));
                String readLine = null;
                StringBuilder sbInput = new StringBuilder();

                while ((readLine = br.readLine()) != null) {
                    System.out.println(readLine);
                    sbInput.append(readLine + "\n");
                }
                is.close();
                msgResponse.Message = sbInput.toString();
    //                final String serverResponseMessage = connection.getResponseMessage();
            } catch (FileNotFoundException e) {
                msgResponse.iRet = -1;
                msgResponse.Message = e.getMessage();
            } catch (MalformedURLException e) {
                msgResponse.iRet = -2;
                msgResponse.Message = e.getMessage();                e.printStackTrace();
            } catch (IOException e) {
                msgResponse.iRet = -3;
                msgResponse.Message = e.getMessage();                e.printStackTrace();
            } catch (Exception e) {
                msgResponse.iRet = -4;
                msgResponse.Message = e.getMessage();                e.printStackTrace();
            }
            return msgResponse;
        }
    }
}
