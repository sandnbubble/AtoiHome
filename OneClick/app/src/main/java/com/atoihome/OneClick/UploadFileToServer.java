package com.atoihome.OneClick;


import android.app.ProgressDialog;
import android.content.Context;
import android.os.AsyncTask;

import android.util.Log;
import android.widget.Toast;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import static android.widget.Toast.LENGTH_SHORT;


public class UploadFileToServer  {
    Context mContext;
    boolean mbShowProgressDialog = false;

    UploadFileToServer(Context context, boolean bShowProgressDialog) {
        mContext = context;
        mbShowProgressDialog = bShowProgressDialog;
    }

    public void uploadFileToServer(String strHostServiceURL, String strFilePath) {
//        사용자 인증 rest api를 call. UploadFile안에서 처리하는 것이 바람직함
//        validateUser InvokeGetMethod = new validateUser();
//        InvokeGetMethod.execute(strURL);
        if (strFilePath != null) {
            new UploadFile().execute(strHostServiceURL, strFilePath);
        } else {
            Toast.makeText(mContext, "전송할 이미지가 없습니다.", LENGTH_SHORT).show();
        }
    }

    public class UploadFile extends AsyncTask<String, String, Integer> {
        private ProgressDialog mProgressDlg;

        public void upoadFile() {
        }
        @Override
        protected void onCancelled() {
            super.onCancelled();
        }

        @Override protected void onPreExecute() {
            super.onPreExecute();
            mProgressDlg = new ProgressDialog(mContext);
            mProgressDlg.setProgressStyle(ProgressDialog.STYLE_HORIZONTAL);
            mProgressDlg.setMessage("작업 시작");
            mProgressDlg.setCancelable (false);
//            mProgressDlg.setIndeterminate (true);
//            MainActivity에서 호출할 경우 progressdlg를  출력하고 서비스에서 호출할 경우는 progressdlg를 출력할 수도 없고 해서도 안된다.
//            Toast는 어플리케이션 컨텍스를 사용하여 출력할 수 있지만 progressdlg는 출력되지 않고 exception error가 발생한다.
            if (mbShowProgressDialog == true)
                mProgressDlg.show();
        }

        @Override protected void onProgressUpdate(String... progress) {
            if (progress[0].equals("progress")) {
                mProgressDlg.setProgress(Integer.parseInt(progress[1]));
                mProgressDlg.setMessage(progress[2]);
            }
            else if (progress[0].equals("max")) {
                mProgressDlg.setMax(Integer.parseInt(progress[1]));
            }
        }

        @Override
        protected void onPostExecute(Integer iRet) {
            super.onPostExecute(iRet);
            mProgressDlg.dismiss();
            mProgressDlg = null;
            switch (iRet) {
                case 0:
                    Toast.makeText(mContext, "스크린샷 이미지를 업로드 하였습니다", LENGTH_SHORT).show();
                    break;
                case -1:
                    Toast.makeText(mContext, "처리할 수 없는 요청입니다 ", LENGTH_SHORT).show();
                    break;
                case -2:
                    Toast.makeText(mContext, "잘못된 URL입니다", LENGTH_SHORT).show();
                    break;
                case -3:
                    Toast.makeText(mContext, "서버와 연결할 수 없습니다", LENGTH_SHORT).show();
                    break;
                case -4:
                    Toast.makeText(mContext, "파일을 전송하지 못했습니다", LENGTH_SHORT).show();
                    break;
            }
        }

        @Override
        protected Integer doInBackground(String... params) {

            Integer iRet=0;
            int serverResponseCode = 0;

            HttpURLConnection connection;
            DataOutputStream dataOutputStream;

            int bytesRead, bytesAvailable, bufferSize;
            byte[] buffer;
            int maxBufferSize = 1024*1024;
            File selectedFile = new File(params[1]);

            String[] parts = params[1].split("/");
            final String strfileName = parts[parts.length - 1];

            try {
                FileInputStream fileInputStream = new FileInputStream(selectedFile);
//                강제로 mainactivity가 종료되면 URL정보를 얻지 못하는 문제가 생겨서 임시로 하드코딩함
                URL url = new URL(params[0] + "/uploadimage/"+strfileName);
                //URL url = new URL("http://211.212.200.192/TextTransfer/web/uploadimage/"+strfileName);
                connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);//Allow Inputs
                connection.setDoOutput(true);//Allow Outputs
                connection.setUseCaches(false);//Don't use a cached Copy
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/octet-stream");
                connection.setRequestProperty("Upload-Filename", strfileName);
                connection.setConnectTimeout(10000);
                //creating new dataoutputstream
                dataOutputStream = new DataOutputStream(connection.getOutputStream());

                //returns no. of bytes present in fileInputStream
                bytesAvailable = fileInputStream.available();
                //selecting the buffer size as minimum of available bytes or 1 MB
                bufferSize = Math.min(bytesAvailable, maxBufferSize);

                int nMaxLoopCount = bytesAvailable/bufferSize;
                //setting the buffer as byte array of size of bufferSize
                buffer = new byte[bufferSize];
                //reads bytes from FileInputStream(from 0th index of buffer to buffersize)
                bytesRead = fileInputStream.read(buffer, 0, bufferSize);

                //loop repeats till bytesRead = -1, i.e., no bytes are left to read
                int iProgress = 0, iCurCount=0;
                while (bytesRead > 0) {
//                    if (mbShowProgressDialog == true) {
//                        Thread.sleep(100);
//                    }
                    //write the bytes read from inputstream
                    dataOutputStream.write(buffer, 0, bufferSize);
                    bytesAvailable = fileInputStream.available();
                    bufferSize = Math.min(bytesAvailable, maxBufferSize);
                    bytesRead = fileInputStream.read(buffer, 0, bufferSize);
                    iCurCount +=1;
                    // 카메라에서 스크린 샷을 하면 150%까지 나오는 경우가 있어서 일단 회피함
                    iProgress = (int) Math.min(((double)iCurCount/(double)nMaxLoopCount)*100.0, 100);
                    publishProgress("progress", Integer.toString(iProgress), "전송율" + Integer.toString(iProgress) + "%");
                }

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

                //closing the input and output streams
                fileInputStream.close();
                dataOutputStream.flush();
                dataOutputStream.close();

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

    protected class  validateUser extends AsyncTask <String, Void, String>{
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
}



