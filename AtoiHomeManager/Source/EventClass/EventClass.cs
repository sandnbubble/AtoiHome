using AtoiHomeServiceLib;

namespace AtoiHomeManager
{
    // Application thread publish this event to send message for views
    public delegate void ModelContextEvent(object sender, ModelContextArgs e);
    public delegate void OneClickShotEvent(object sender, OneClickShotEventArgs e);

    public class ModelContextArgs
    {
        public MessageType MsgType { get; set; }
        public string Message { get; set; }
        public ModelContextArgs(MessageType MsgType, string Message)
        {
            // this is message that was received from IPCServer
            this.MsgType = MsgType;
            this.Message = Message;
        }
    }

    public class MainWindowContext
    {
        // Bind with ButtonConnect.IsEnabled property in MainWindow.xaml
        public bool bConnectButtonEnable { get; set; }
        public string strDownloadedFilename { get; set; }
    }

}
