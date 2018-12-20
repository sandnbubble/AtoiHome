using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace AtoiHomeServiceLib
{
    //////////////////////////////////////////////////////////////////////////////////////

    public delegate void TextTransferEvent(object sender, TextTransferEventArgs e);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class TextTransfer : ITextTransferRest, ITextTransferSoap
    {
        // create TextTransferEvent instance
        public event TextTransferEvent onTextTransferEvent = delegate { };

        /// <summary>
        /// 환경변수로 변경할 것
        /// </summary>
        private const string SZ_BASEDIR = "D:/uploadfile/";

        public String GetRoot ()
        {
            return @"This site was built to test RESTful XFaxService";
        }

        /// <summary>
        /// Get 메서드를 사용하는 Rest API를 시험하기 위해 만든 오퍼레이션 삭제될 것
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public String GetData(string arg)
        {
            try
            {
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.GET_DATA, "I'm a atoi", null));
                return "Hello " + arg;
            }
            catch (Exception ex)
            {
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.ERROR_MSG, this.ToString(), ex.Message.ToString()));
            }
            return null;
        }


        /// <summary>
        /// 클라이언트가 POST mathod를 사용하여 전송하는 오퍼레이션
        /// </summary>
        /// <param name="strFilename"></param> 업로드 파일명
        /// <param name="InputStream"></param> 업로드에 사용되는 stream
        /// <returns></returns>
        public String UploadImage(String strFilename, Stream InputStream)
        {
            try
            {
                if (strFilename == null)
                    return Message.CreateMessage(MessageVersion.None, "UploadImage", "Filename is null").ToString();

                string szFilePath = SZ_BASEDIR + strFilename; ;
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
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.UPLOAD_IMAGE, strFilename, "Success"));
                return Message.CreateMessage(MessageVersion.None, "UploadImage", "Success").ToString();
            }
            catch (Exception ex)
            {
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.ERROR_MSG, strFilename, ex.Message.ToString()));
                return Message.CreateMessage(MessageVersion.None, "UploadImage", ex.Message).ToString();
            }
        }

        public Stream DownloadImage(String strFilename)
        {
            try
            {
                string filePath = System.IO.Path.Combine("D:/uploadfile/", strFilename);
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if (!fileInfo.Exists) throw new System.IO.FileNotFoundException("File not found");
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.DOWNLOAD_IMAGE, strFilename, "Success"));
                return File.OpenRead(filePath);
            }
            catch (Exception ex)
            {
                onTextTransferEvent(this, new TextTransferEventArgs("atoi", MessageType.ERROR_MSG, strFilename, ex.Message.ToString()));
            }
            return null;
        }

        /// <summary>
        /// HTTP헤더 정보를 얻는 디버그용도의 메서드
        /// </summary>
        /// <param name="strFindKey"></param>
        /// <returns></returns>
        protected String GetRequestHeaderProperty (String strFindKey)
        {
            String strRet = null;

            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            WebHeaderCollection headers = request.Headers;

            //Program.log.DebugFormat("-------------------------------------------------------");
            //Program.log.DebugFormat(request.Method + " " + request.UriTemplateMatch.RequestUri.AbsolutePath);
            foreach (string headerName in headers.AllKeys)
            {
                if (headerName.ToLower().Equals(strFindKey))
                    strRet = headers[headerName].ToLower();
                //Program.log.DebugFormat(headerName + ": " + headers[headerName]);
            }
            //Program.log.DebugFormat("-------------------------------------------------------");
            return strRet;
        }
    }
}
