using AtoiHomeServiceLib;
using System;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Description;


namespace AtoiHome
{
    public enum EnumNetworkType
    {
        WAN_NETWORK = 0,
        LAN_NETWORK_ETHERNET,
        LAN_NETWORK_WIRELESS,
        LOCALHOST,
    }

    public class ServiceHostManager
    {
        ServiceHost serviceHost, IPCHost;
        NotifyService IPCService = new NotifyService();

        public int StartService () 
        {
            Program.log.DebugFormat("Start TextTransfer service");
            try
            {

                #region Set host ipaddress
                EnumNetworkType iNetworkType = EnumNetworkType.LOCALHOST;
                String strServiceIP = null;
                switch (iNetworkType)
                {
                    case EnumNetworkType.WAN_NETWORK:
                        strServiceIP = DNSInfo.GetMyExternalIpAddress().ToString();
                        break;
                    case EnumNetworkType.LAN_NETWORK_ETHERNET:
                    case EnumNetworkType.LAN_NETWORK_WIRELESS:
                        if (iNetworkType == EnumNetworkType.LAN_NETWORK_ETHERNET)
                            strServiceIP = DNSInfo.GetLocalIP(NetworkInterfaceType.Ethernet).ToString();
                        else
                            strServiceIP = DNSInfo.GetLocalIP(NetworkInterfaceType.Wireless80211).ToString();
                        break;
                    default:
                        strServiceIP = "LOCALHOST";
                        break;
                }

                /*************************************************************************************
                 * 유동IP가 바뀌는 경우를 고려해서 서비스가 시작되면 sandnbubble @gmail.com으로 변경된
                 공인 IP를 보냄.
                 ************************************************************************************/
                //var client = new SmtpClient("smtp.gmail.com", 587);
                //client.Credentials = new NetworkCredential("sandnbubble@gmail.com", "gus010365!!");
                //client.EnableSsl = true;
                //client.Send("sandnbubble@gmail.com", "sandnbubble@gmail.com", "test", strServiceIP);
                //Program.log.DebugFormat("Sent host ipaddress to sandnbubble@gmail.com");
                #endregion

                // ServiceHost는 첫번째 매개변수로 service type과 service instance를 사용할 수 있다.
                // 매개변수로 service type을 사용할 경우 service의 instance는 client request가 발생할 때
                // 생성된다. 따라서 TextTransfer service 메서드인 UploadImage가 발행하는 ImageUploadedEvent의 eventhandler를
                // ServiceHostManager에서 등록하려면 ServiceHost의 첫번째 매개변수를 아래와 같이 TextTransfer instance로 설정해야한다.
                // 이 방법을 사용할 때 event 발행자인 TextTransfer 서비스의 속성은 반드시 아래와 같아야한다. 
                //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
                // TextTransfer class 참조할 것

                #region TextTransfer service 
                TextTransfer instancTextTransfer = new TextTransfer();
#if DEBUG
                serviceHost = new ServiceHost(instancTextTransfer, new Uri("http://" + strServiceIP + ":80/Test/TextTransfer"));
#else
                serviceHost = new ServiceHost(instancTextTransfer, new Uri("http://" + strServiceIP + ":80/TextTransfer"));
#endif
                serviceHost.Opened += new EventHandler(TextTransferServiceOpened);
                serviceHost.Open();
                Program.log.DebugFormat("\tTextTransfer Service Started with {0} end points", serviceHost.Description.Endpoints.Count);

                foreach (Uri address in serviceHost.BaseAddresses)
                {
                    Program.log.DebugFormat("\tListening on " + address);
                    foreach (ServiceEndpoint ed in serviceHost.Description.Endpoints)
                    {
                        Program.log.DebugFormat("\t{0} {1}", ed.Name, ed.ListenUri.ToString());
                    }
                }
#endregion

#region NotifyService that IPC between TextTransfer service and AtoiHomeManager client application
                NotifyService instanceNotify = new NotifyService();
#if DEBUG
                IPCHost = new ServiceHost(instanceNotify, new Uri("net.pipe://localhost/Test"));
#else
                IPCHost = new ServiceHost(instanceNotify, new Uri("net.pipe://localhost"));
#endif

                NetNamedPipeBinding IPCBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                IPCBinding.ReceiveTimeout = TimeSpan.MaxValue;
                IPCHost.AddServiceEndpoint(typeof(INotifyService), IPCBinding, "Notify");
                IPCHost.Opened += new EventHandler(NotifyServiceOpened);
                IPCHost.Closing += new EventHandler(NotifyServiceClosing);
                IPCHost.Open();
                Program.log.DebugFormat("\tNotification Service Started with {0} end points", IPCHost.Description.Endpoints.Count);

                foreach (Uri address in IPCHost.BaseAddresses)
                {
                    Program.log.DebugFormat("\tListening on " + address);
                    foreach (ServiceEndpoint ed in IPCHost.Description.Endpoints)
                    {
                        Program.log.DebugFormat("\t{0} {1}", ed.Name, ed.ListenUri.ToString());
                    }
                }
                Program.log.DebugFormat("\tClick any key to close...");
#endregion
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat(ex.Message);
                serviceHost.Close();
                IPCHost.Close();
            }
            return 0;
        }

        public bool StopService()
        {
            Program.log.DebugFormat("Invoked StopService");
            try
            {
                Program.log.DebugFormat("\tPush service is closing...... ");
                IPCHost.Close();
                Program.log.DebugFormat("\tTextTransfer Service is closing...... ");
                serviceHost.Close();
                Program.log.DebugFormat("\tSevices were closed.... bye~~~");
                return true;
            }
            catch (Exception e)
            {
                IPCHost.Abort();
                serviceHost.Abort();
                Program.log.ErrorFormat(e.Message);
                return false;
            }
        }

#region EventHandler



        /// <summary>
        /// Notify service가 시작되면 main thread에서 Notify service의 오퍼레이션들(Connect, Disconnect 등)이
        /// 발행하는 이벤트를 구독하기위한 이벤트핸들러를 정의하고 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyServiceOpened(Object sender, EventArgs e)
        {
            try
            {
                // Host는 다르지만 TextTransferEvent 타입은 TextTransf와 NotifyService가 공유한다.
                (IPCHost.SingletonInstance as NotifyService).onTextTransferEvent += new TextTransferEvent(NotifyServiceOperationCompleted);
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat(ex.Message);
            }
        }

        /// <summary>
        /// Notification 서비스가 중지될 때 연결된 클라이언트들에게 서비스 중지를 전달
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyServiceClosing(Object sender, EventArgs e)
        {
            try
            {
                TextTransferEventArgs ClosingEventArgs = new TextTransferEventArgs("NotifyService", MessageType.NOTIFYSERVICE_CLOSING, "Closing", null);
                IPCService.SendMessage(ClosingEventArgs);
            }
            catch (Exception ex)
            {
                IPCHost.Abort();
                Program.log.ErrorFormat(ex.Message);
            }
        }

        /// <summary>
        /// TextTransfer service가 시작되면 main thread에서 TextTransfer service의 오퍼레이션들(GetData, UploadImage, DownloadImage 등)이
        /// 발행하는 이벤트를 구독하기위한 이벤트핸들러를 정의하고 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextTransferServiceOpened(Object sender, EventArgs e)
        {
            try
            {
                (serviceHost.SingletonInstance as TextTransfer).onTextTransferEvent += new TextTransferEvent(TextTransferServiceOperationCompleted);
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat(ex.Message);
            }
        }


        /// <summary>
        /// TextTransfer 서비스의 오퍼레이션들이 발행하는 이벤트를 처리하기 위한 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyServiceOperationCompleted(Object sender, TextTransferEventArgs e)
        {
            try
            {
                if (e.MessageType != MessageType.ERROR_MSG)
                {
                    Program.log.DebugFormat("Successful completed {0} operation with {1} in {2}", e.MessageType.ToString(), e.Message, sender.ToString());
                    switch (e.MessageType)
                    {
                        case MessageType.CONNECTED_CLIENT:
                            Program.log.DebugFormat("Connected new client {0}, {1} ", e.UserId, e.Message);
                            break;
                        case MessageType.DISCONNECTED_CLIENT:
                            Program.log.DebugFormat("Disconnected client {0}, {1} ", e.UserId, e.Message);
                            break;
                        default:
                            Program.log.ErrorFormat("Unknown message type is received from client {0}, {1} ", e.UserId, e.Message);
                            break;
                    }
                }
                else
                {
                    Program.log.ErrorFormat("Exception Error occurred in {0}\r\n{1}", sender.ToString(), e.DebugMsg);
                }
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat("Exception Error occurred in {0}\r\n{1}", this.ToString(), ex.Message);
            }
        }
#endregion

#region
        /// <summary>
        /// TextTransfer 서비스의 오퍼레이션들이 발행하는 이벤트를 처리하기 위한 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextTransferServiceOperationCompleted(Object sender, TextTransferEventArgs e)
        {
            try
            {
                if (e.MessageType != MessageType.ERROR_MSG)
                {
                    Program.log.DebugFormat("Successful completed {0} operation with {1} in {2}", e.MessageType.ToString(), e.Message, sender.ToString());
                    switch (e.MessageType)
                    {
                        // net.pipe에 연결된 client application에 UPLOAD_IMAGE를 알림
                        case MessageType.UPLOAD_IMAGE:
                            e.MessageType = MessageType.DOWNLOAD_IMAGE;
                            IPCService.SendMessage(e);
                            Program.log.DebugFormat("Sent {0} message to {1}", e.MessageType.ToString(), e.UserId);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Program.log.ErrorFormat("Exception Error occurred in {0}\r\n{1}", sender.ToString(), e.DebugMsg);
                }
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat("Exception Error occurred in {0}\r\n{1}", this.ToString(), ex.Message);
            }
        }
#endregion
    }
}
