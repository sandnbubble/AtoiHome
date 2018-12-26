using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace AtoiHomeServiceLib
{
    [DataContract]
    public class OneClickShotEventArgs
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public MessageType MessageType { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string DebugMsg { get; set; }

        public OneClickShotEventArgs(string UserId, string Password, MessageType MessageType, string Message, string DebugMsg)
        {
            this.UserId = UserId;
            this.Password = Password;
            this.MessageType = MessageType;
            this.Message = Message;
            this.DebugMsg = DebugMsg;
        }

        public OneClickShotEventArgs(OneClickShotEventArgs e)
        {
            this.UserId = e.UserId;
            this.Password = e.Password;
            this.MessageType = e.MessageType;
            this.Message = e.Message;
            this.DebugMsg = e.DebugMsg;
        }
    }
}
