using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;



namespace AtoiHomeServiceLib
{
    [ServiceContract]
    public interface IOneClickShot
    {
        [OperationContract]
        [WebGet(UriTemplate = "/")]
        String GetRoot();

        [OperationContract]
        [WebGet]
        string GetData(string arg);

#if _EXTERNAL_MSSQLDB
        [OperationContract]
        [WebGet]
        string SignIn(string Email, string Password);

        [OperationContract]
        [WebGet]
        string SignUp(string Email, string Password);
    }
#endif
    [ServiceContract]
    public interface IOneClickShotRest: IOneClickShot
    {
        [OperationContract]

        // NON-SOAP 종점 operation
        [WebInvoke(UriTemplate = "UploadImage/{strFilename}")]
        String UploadImage(string strFilename, Stream image);
    }

    [ServiceContract]
    public interface IOneClickShotSoap: IOneClickShot
    {
        // SOAP 종점 operation
        [OperationContract]
        Stream DownloadImage(String strFilename);
    }
}
