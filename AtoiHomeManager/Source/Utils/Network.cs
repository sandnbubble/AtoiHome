using AtoiHomeServiceLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AtoiHomeManager.Source.Utils
{
    public class Network
    {
        public static string strEmail = "admin@atoihome.site";
#if _TEST_SERVICE_
        public static string HostAddr = "net.pipe://localhost/test/Notify";
#else
        public static string HostAddr = "net.pipe://localhost/Notify";
#endif
        public static INotifyService NotificationService;

        public static bool ConnectService()
        {
            try
            {
#if (!_TEST_SERVICE_)
                if (Utility.GetServiceStatus("OneClickShot") == false)
                    return false;
#endif
                var callback = new App.NotifyCallback();
                var context = new InstanceContext(callback);
                NetNamedPipeBinding IPCBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
                IPCBinding.ReceiveTimeout = TimeSpan.MaxValue;
                var pipeFactory = new DuplexChannelFactory<INotifyService>(context, IPCBinding, new EndpointAddress(HostAddr));
                NotificationService = pipeFactory.CreateChannel();
                OneClickShotEventArgs e = new OneClickShotEventArgs(strEmail, "gksrmf65!!", MessageType.CONNECTED_CLIENT, "", null);

                if (NotificationService.Connect(e))
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Can not connect service");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DisconnectService()
        {
            NotificationService.Disconnect(new OneClickShotEventArgs(strEmail, null, MessageType.GET_DATA, "bye", null));
        }
    }

}
