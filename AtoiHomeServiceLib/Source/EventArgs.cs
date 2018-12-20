using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace AtoiHomeServiceLib
{
    [DataContract]
    public class TextTransferEventArgs
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public MessageType MessageType { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string DebugMsg { get; set; }

        public TextTransferEventArgs(string UserId, MessageType MessageType, string Message, string DebugMsg)
        {
            this.UserId = UserId;
            this.MessageType = MessageType;
            this.Message = Message;
            this.DebugMsg = DebugMsg;
        }

        public TextTransferEventArgs(TextTransferEventArgs e)
        {
            this.UserId = e.UserId;
            this.MessageType = e.MessageType;
            this.Message = e.Message;
            this.DebugMsg = e.DebugMsg;
        }
    }
}
