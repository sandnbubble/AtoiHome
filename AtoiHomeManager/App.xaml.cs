using System;
using System.Windows;
using AtoiHomeServiceLib;
using Hardcodet.Wpf.TaskbarNotification;


namespace AtoiHomeManager
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public TaskbarIcon notifyIcon;
        public bool bConnected { get; set; }
        public event ModelContextEvent onModelContextEvent = delegate { };
        public event TextTransferEvent onTextTransferEvent = delegate { };


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                //create the notifyicon(it's a resource declared in NotifyIconResources.xaml
                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
                bConnected = AtoiHomeManager.MainWindow.ConnectToIPCService();
                onTextTransferEvent += new TextTransferEvent(TextTransferEventHandler);

                //App configuration
#if DEBUG
                ShutdownMode = ShutdownMode.OnMainWindowClose;

#else
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
#endif


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }

        //IPCServer에서 호출되는 양방향 통신을 위한 콜백함수
        //매개변수로 전달되는 message를 class로 바꿔서 다양한 용도로 사용해야함
        public class NotifyCallback : ICallbackService
        {
            public void SendCallbackMessage(TextTransferEventArgs e)
            {
                try
                {
                    // App로 이벤트를 발행
                    (Current as App).onTextTransferEvent(this, new TextTransferEventArgs(e));
                    //MessageBox.Show("UserId : " + e.UserId + "MessageType : " + e.MesssageType + "Message : " + e.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
