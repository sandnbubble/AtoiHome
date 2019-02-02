using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace AtoiHomeServiceLib
{
    //////////////////////////////////////////////////////////////////////////////////////

    public delegate void OneClickShotEvent(object sender, OneClickShotEventArgs e);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class OneClickShot : IOneClickShotRest, IOneClickShotSoap
    {
        // create OneClickShotEvent instance
        public event OneClickShotEvent RaiseOneClickShotEvent = delegate { };

        /// <summary>
        /// LOCAL_SERVICE가 업로드 파일을 저장하는 폴더 설정
        /// 서비스가 시작될때 윈도우 레지스트리에서 폴더path을 얻은 후 이 메서드를 콜해서
        /// 업로드폴더 경로를 지정한다.
        /// </summary>
        private static string SZ_BASEDIR = "";

        public bool setEnv(string strUploadPath)
        {
            string folderPath = System.IO.Path.Combine(strUploadPath);
            System.IO.DirectoryInfo folderInfo = new System.IO.DirectoryInfo(folderPath);
            if (!folderInfo.Exists)
            {
                return false;
                throw new System.IO.DirectoryNotFoundException("Directory not found");
            }
            else
            {
                SZ_BASEDIR = strUploadPath;
                return true;
            }
        }

        public string GetRoot ()
        {
            return @"This site was built to test RESTful XFaxService";
        }

        /// <summary>
        /// Get 메서드를 사용하는 Rest API를 시험하기 위해 만든 오퍼레이션 삭제될 것
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public string GetData(string arg)
        {
            try
            {
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs("atoi", "gksrmf65!!",  MessageType.GET_DATA, "I'm a atoi", null));
                return "Hello " + arg;
            }
            catch (Exception ex)
            {
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs("atoi", "gksrmf65!!", MessageType.ERROR_MSG, this.ToString(), ex.Message.ToString()));
            }
            return null;
        }

        /// <summary>
        /// oneclick apps에서 POST mathod를 사용하여 전송하는 Rest API 오퍼레이션
        /// </summary>
        /// <param name="strFilename"></param> 업로드 파일명
        /// <param name="InputStream"></param> 업로드에 사용되는 stream
        /// <returns></returns>
        /// 
        public string UploadImage(string strFilename, Stream InputStream)
        {
            try
            {
                string Email;
                if (CheckAccessCore(out Email) == false)
                {
                    WebOperationContext ctx = WebOperationContext.Current;
                    ctx.OutgoingResponse.StatusCode = HttpStatusCode.NonAuthoritativeInformation;
                    return Message.CreateMessage(MessageVersion.None, "UploadImage", "Failure").ToString();
                }

                if (strFilename == null)
                    return Message.CreateMessage(MessageVersion.None, "UploadImage", "Filename is null").ToString();


                string szFilePath = SZ_BASEDIR + "\\" + strFilename; ;
                Console.WriteLine("Save Folder : {0}", szFilePath);
                if (System.IO.File.Exists(szFilePath)) System.IO.File.Delete(szFilePath);

                FileStream fileStream = null;
                using (fileStream = new FileStream(szFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    const int bufferLen = 1024 * 4;
                    byte[] buffer = new byte[bufferLen];
                    int count = 0;
                    while ((count = InputStream.Read(buffer, 0, bufferLen)) > 0)
                    {
                        fileStream.Write(buffer, 0, count);
                    }
                    fileStream.Close();
                    InputStream.Close();
                }
                // Publish ImageUploadedEvent
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs(Email, "gksrmf65!!", MessageType.UPLOAD_IMAGE, strFilename, "Success"));
                return Message.CreateMessage(MessageVersion.None, "UploadImage", "Success").ToString();
            }
            catch (Exception ex)
            {
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs("atoi", "gksrmf65!!", MessageType.ERROR_MSG, strFilename, ex.Message.ToString()));
                return Message.CreateMessage(MessageVersion.None, "UploadImage", ex.Message).ToString();
            }
        }

        /// <summary>
        /// oneclickviewer에서 remote service에 요청하는 SOAP 파일다운로드 오퍼레이션
        /// local service일 경우는 이미지가 저장된 폴더에서 파일을 로드
        /// </summary>
        /// <param name="strFilename"></param>
        /// <returns></returns>
        public Stream DownloadImage(string strFilename)
        {
            try
            {
                string filePath = System.IO.Path.Combine(SZ_BASEDIR, strFilename);
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if (!fileInfo.Exists)
                    throw new System.IO.FileNotFoundException("File not found");
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs("atoi", "gksrmf65!!", MessageType.DOWNLOAD_IMAGE, strFilename, "Success"));
                return File.OpenRead(filePath);
            }
            catch (Exception ex)
            {
                RaiseOneClickShotEvent(this, new OneClickShotEventArgs("atoi", "gksrmf65!!", MessageType.ERROR_MSG, strFilename, ex.Message.ToString()));
            }
            return null;
        }

        /// <summary>
        /// HTTP헤더 정보를 얻는 디버그용도의 메서드
        /// </summary>
        /// <param name="strFindKey"></param>
        /// <returns></returns>
        protected string GetRequestHeaderProperty (string strFindKey)
        {
            string strRet = null;

            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            WebHeaderCollection headers = request.Headers;

            foreach (string headerName in headers.AllKeys)
            {
                if (headerName.ToLower().Equals(strFindKey.ToLower()))
                    strRet = headers[headerName].ToLower();
                Console.WriteLine(headerName + ": " + headers[headerName]);
            }
            return strRet;
        }

        

        protected bool CheckAccessCore(out string Email)
        {
            //Extract the Authorization header, and parse out the credentials converting the Base64 string:  
            var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if ((authHeader != null) && (authHeader != string.Empty))
            {
                var strToken = authHeader.Substring(7);
                BearerOAuthTokenDeserializer.AuthenticationTicket Token = BearerOAuthTokenLib.OAuthTokeAPI.DeserializeToken(strToken);
                Email = BearerOAuthTokenLib.OAuthTokeAPI.FindValueByKey(Token, "sub");
                string role = BearerOAuthTokenLib.OAuthTokeAPI.FindValueByKey(Token, "role");
                return true;
            }
            else
            {
                //No authorization header was provided, so challenge the client to provide before proceeding:  
                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"MyWCFService\"");
                //Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
        }
    }
}
