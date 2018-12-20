using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;



namespace AtoiHomeServiceLib
{
    [ServiceContract]
    public interface ITextTransfer
    {
        [OperationContract]
        [WebGet(UriTemplate = "/")]
        String GetRoot();

        [OperationContract]
        [WebGet]
        string GetData(string arg);
    }

    [ServiceContract]
    public interface ITextTransferRest: ITextTransfer
    {
        [OperationContract]

        // NON-SOAP 종점 operation
        [WebInvoke(UriTemplate = "UploadImage/{strFilename}")]
        String UploadImage(string strFilename, Stream image);
    }

    [ServiceContract]
    public interface ITextTransferSoap: ITextTransfer
    {
        // SOAP 종점 operation
        [OperationContract]
        Stream DownloadImage(String strFilename);
    }
}
