using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AtoiHomeServiceLib
{
    public enum MessageType
    {
        GET_DATA = 0,
        UPLOAD_IMAGE,
        DOWNLOAD_IMAGE,
        NOTIFYSERVICE_CLOSING,
        ERROR_MSG,
        CONNECTED_CLIENT,
        DISCONNECTED_CLIENT,
    }

    [ServiceContract(Namespace = "http://www.atoihome.com", SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbackService))]
    public interface INotifyService
    {
        [OperationContract(IsOneWay = true)]
        void Connect(TextTransferEventArgs e);

        [OperationContract(IsOneWay = true)]
        void Disconnect(TextTransferEventArgs e);
        
            [OperationContract(IsOneWay = true)]
        void SendMessage(TextTransferEventArgs e);
    }

    public interface ICallbackService
    {
        [OperationContract(IsOneWay = true)]
        void SendCallbackMessage(TextTransferEventArgs e);

    }
}
