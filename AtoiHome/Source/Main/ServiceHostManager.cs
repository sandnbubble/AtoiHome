using AtoiHomeServiceLib;
using System;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Description;
using AtoiHomeServiceLib.Source.Utility;
using System.Data.SqlClient;

// _REMOTE_SERVICE_
// 인증서버, 웹사이트, WCF service로 구성되며 sumtech corp에서 관리한다.
// https://www.ahoihome.site, WCF atoihomeservice remote service
// 
// _LOCAL_SERVICE_
// 사용자 컴퓨터에 설치되어 서비스를 제공한다.
// http://localservice, WCF atoihomeservice standalone service
// _REMOTE_SERVICE_, _LOCAL_SERVICE_의 소스를 분리하여 별도의 프로젝트로 관리하는 방안을 고려중
// 공통소스를 분리하는 것이 선행되어야 함.

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
        OneClickShot OneClickShotService = new OneClickShot();

        public int startService()
        {
            Program.log.DebugFormat("Start OneClickShot service");
            try
            {
                // 레지스트리에서 업로드폴더경로를 얻어와서 atoihomeservice의 업로드경로로 설정한다.
                // REMOTE_SERVICE일 경우 IIS에서 가상디렉토리를 수동으로 재설정해야하는데
                // 자동화하는 방법을 찾아야함.
                string UploadPath = Utils.getRegValue("atoihome", "UploadPath");
                OneClickShotService.setEnv(UploadPath);
                Program.log.DebugFormat("UploadPath : {0}", UploadPath);

                #region Set host ipaddress
                EnumNetworkType iNetworkType = EnumNetworkType.LOCALHOST;
                string strServiceIP = null;
                switch (iNetworkType)
                {
                    case EnumNetworkType.WAN_NETWORK:
                        strServiceIP = DNSInfo.GetPublicIP().ToString();
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
                // 생성된다. 따라서 OneClickShot service 메서드인 UploadImage가 발행하는 ImageUploadedEvent의 eventhandler를
                // ServiceHostManager에서 등록하려면 ServiceHost의 첫번째 매개변수를 아래와 같이 OneClickShot instance로 설정해야한다.
                // 이 방법을 사용할 때 event 발행자인 OneClickShot 서비스의 속성은 반드시 아래와 같아야한다. 
                //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
                // OneClickShot class 참조할 것

                #region OneClickShot service 
#if DEBUG
                serviceHost = new ServiceHost(OneClickShotService, new Uri("http://" + strServiceIP + ":80/Test/OneClickShot"));
#else
                serviceHost = new ServiceHost(OneClickShotService, new Uri("http://" + strServiceIP + ":80/OneClickShot"));
#endif
                serviceHost.Opened += new EventHandler(OneClickShotServiceOpened);
                serviceHost.Open();
                Program.log.DebugFormat("\tOneClickShot Service Started with {0} end points", serviceHost.Description.Endpoints.Count);

                foreach (Uri address in serviceHost.BaseAddresses)
                {
                    Program.log.DebugFormat("\tListening on " + address);
                    foreach (ServiceEndpoint ed in serviceHost.Description.Endpoints)
                    {
                        Program.log.DebugFormat("\t{0} {1}", ed.Name, ed.ListenUri.ToString());
                    }
                }
                #endregion

                #region NotifyService that IPC between OneClickShot service and AtoiHomeManager client application
#if DEBUG
                IPCHost = new ServiceHost(IPCService, new Uri("net.pipe://localhost/Test"));
#else
                IPCHost = new ServiceHost(IPCService, new Uri("net.pipe://localhost"));
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

        public bool stopService()
        {
            Program.log.DebugFormat("Invoked StopService");
            try
            {
                Program.log.DebugFormat("\tPush service is closing...... ");
                IPCHost.Close();
                Program.log.DebugFormat("\tOneClickShot Service is closing...... ");
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
                // Host는 다르지만 OneClickShotEvent 타입은 TextTransf와 NotifyService가 공유한다.
                (IPCHost.SingletonInstance as NotifyService).OneClickShotEvent += new OneClickShotEvent(NotifyServiceOperationCompleted);
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
                OneClickShotEventArgs ClosingEventArgs = new OneClickShotEventArgs("NotifyService", null, MessageType.NOTIFYSERVICE_CLOSING, "Closing", null);
                IPCService.CallbackAllClients(ClosingEventArgs);
            }
            catch (Exception ex)
            {
                IPCHost.Abort();
                Program.log.ErrorFormat(ex.Message);
            }
        }

        /// <summary>
        /// OneClickShot service가 시작되면 main thread에서 OneClickShot service의 오퍼레이션들(GetData, UploadImage, DownloadImage 등)이
        /// 발행하는 이벤트를 구독하기위한 이벤트핸들러를 정의하고 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneClickShotServiceOpened(Object sender, EventArgs e)
        {
            try
            {
                (serviceHost.SingletonInstance as OneClickShot).RaiseOneClickShotEvent += new OneClickShotEvent(OneClickShotServiceOperationCompleted);
            }
            catch (Exception ex)
            {
                Program.log.ErrorFormat(ex.Message);
            }
        }


        /// <summary>
        /// OneClickShot 서비스의 오퍼레이션들이 발행하는 이벤트를 처리하기 위한 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyServiceOperationCompleted(Object sender, OneClickShotEventArgs e)
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
                        case MessageType.SET_ENV:
                            if (OneClickShotService.setEnv(e.Message))
                            {
                                Utils.setRegValue("atoihome", "UploadPath", e.Message);
                            }
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
        /// OneClickShot 서비스의 오퍼레이션들이 발행하는 이벤트를 처리하기 위한 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneClickShotServiceOperationCompleted(Object sender, OneClickShotEventArgs e)
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
#if !_LOCAL_SERVICE_
                            //Remote service일 경우 www.atoihome.site와 DB를 연동한다.
                            DateTime dtUpload = DateTime.Now;
                            string strUploadDate = dtUpload.ToString("yyyyMMdd HH:mm:ss");
                            Program.log.DebugFormat("UploadDate = {0}", strUploadDate);
                            string SQLInsert = string.Format("INSERT INTO uploadimage (UserID, UploadDate, ImagePath) VALUES ('{0}', '{1}', '{2}');",
                                e.UserId,
                                strUploadDate,
                                e.Message);
                            InsertQuery(SQLInsert);
                            //IPCService는 네임드파이프를 사용해서 서버사이드 프로세스간에 IPC를 위해 제공된 것이다.
                            //따라서 전송대상은 현재는 AtoiHomeManager로 한정된다. 
                            //원거리에서 연결한 클라이언트인 OneClickViewer들은 별도의 nettcpip 엔드포인트를 생성해 연결하고
                            //전송루틴을 추가해서 해결할것
                            //RemoteServiceForOneClickViewr.SendMessage(e) 형태가 될 것이다.
                            IPCService.CallbackAllClients(e);
                            Program.log.DebugFormat("Sent {0} message to AtoiHomeManager", e.MessageType.ToString());
#else
                            // LOCAL_SERVICE일 경우 연결된 모든 OneClickViewer에게 메시지를 보낸다.
                            // 이렇게 하면 LOCAL_SERVIC를 사용하는 OneClickViewer에서 로그인을 할 필요가 없게된다.
                            IPCService.CallbackAllClients(e);
                            Program.log.DebugFormat("Sent {0} message to OneClickViewer", e.MessageType.ToString());
#endif
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

#if !_LOCAL_SERVICE_
        /// <summary>
        /// Insert uploadimage infomation to database
        /// </summary>
        /// <param name="strQuery"></param>
        /// <returns></returns>
        public bool InsertQuery(string strQuery)
        {
            try
            {
                string strDbAdminID = "sa";
                string strDbAdminPassword = "gksrmf";
                string strDbName = "AtoiHomeWeb";

                SqlConnection myConnection = new SqlConnection(
                    "Data Source=ATOI\\ATOIHOMEDBSERVER; Persist Security Info = False; "+
                    "User ID = " +strDbAdminID
                    +"; Password = " + strDbAdminPassword
                    + "; Initial Catalog = " +strDbName);
                SqlCommand myCommand = myConnection.CreateCommand();
                myCommand.CommandText = strQuery;
                myConnection.Open();
                myCommand.ExecuteNonQuery();
                myConnection.Close();
                return true;
            }
            catch (Exception e)
            {
                Program.log.DebugFormat(e.Message.ToString());
                return false;
            }
        }
#endif
    }
#endregion
}
