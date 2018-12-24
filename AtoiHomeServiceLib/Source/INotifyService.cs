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

    [DataContract]
    public class CustomerServiceFault
    {
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public IpInfo ipInfo { get; set; }
    }

    [DataContract]
    public class IpInfo
    {
        [DataMember]
        public string strPublicIP { get; set; }

        [DataMember]
        public string strLocalIP { get; set; }
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

        [OperationContract]
        [FaultContractAttribute(typeof(CustomerServiceFault))]
        IpInfo GetHostPublicIP();
    }

    public interface ICallbackService
    {
        [OperationContract(IsOneWay = true)]
        void SendCallbackMessage(TextTransferEventArgs e);

    }
}
