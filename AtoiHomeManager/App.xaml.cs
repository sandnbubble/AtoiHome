using System;
using System.ServiceModel;
using System.Windows;
using AtoiHomeManager.Source.Utils;
using AtoiHomeServiceLib;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro;

namespace AtoiHomeManager
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public TaskbarIcon notifyIcon;
        public bool bConnected { get; set; }
        public IpInfo ServiceInfo { get; set; }

        //IPCService Callback(onOneClickShotEvent) -> App(onModelContextEvent) -> view or viewmodel 
        public event OneClickShotEvent onOneClickShotEvent = delegate { };
        public event ModelContextEvent onModelContextEvent = delegate { };

        protected override void OnStartup(StartupEventArgs e)
        {
            Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);

            // now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent("Green"),
                                        ThemeManager.GetAppTheme("BaseDark")); // or appStyle.Item1
            base.OnStartup(e);
            try
            {
                //create the notifyicon(it's a resource declared in NotifyIconResources.xaml
                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
                if (bConnected = Network.ConnectService())
                {
                        ServiceInfo = Network.NotificationService.GetHostPublicIP();
                }
                onOneClickShotEvent += new OneClickShotEvent(OneClickShotEventHandler);

#if DEBUG
                if (Current.MainWindow == null)
                {
                    Current.MainWindow = new MainWindow();
                    Current.MainWindow.Show();
                }
                ShutdownMode = ShutdownMode.OnMainWindowClose;

#else
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
#endif
            }
            catch (FaultException<CustomerServiceFault> fault)
            {
                MessageBox.Show($"Fault received: {fault.Detail.ErrorMessage}");
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

        //IPCService (named pipe)에서 호출되는 양방향 통신을 위한 콜백함수
        public class NotifyCallback : ICallbackService
        {
            public void SendCallbackMessage(OneClickShotEventArgs e)
            {
                try
                {
                    // App로 이벤트 발행
                    (Current as App).onOneClickShotEvent(this, new OneClickShotEventArgs(e));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
