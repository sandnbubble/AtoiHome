using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AtoiHome
{
    public class DNSInfo
    {
        public static IPAddress GetMyExternalIpAddress()
        {

            var dnsQuery = new ProcessStartInfo("nslookup", "myip.opendns.com. resolver1.opendns.com")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            var lookup = Process.Start(dnsQuery);
            var lookupResult = lookup.StandardOutput.ReadToEnd().Split('\n');
            bool answer = false;
            foreach (var line in lookupResult)
            {
                if (line.Contains("Name:") || line.Contains("이름:"))
                {
                    answer = true;
                }
                else if (!answer)
                    continue;

                if (line.StartsWith("Address:"))
                    return IPAddress.Parse(line.Substring("Address:".Length).Trim());
            }
            return null;
        }

        public static IPAddress GetLocalIP(NetworkInterfaceType nicType)
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Program.log.DebugFormat(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if (ni.NetworkInterfaceType == nicType)
                            {
                                Program.log.DebugFormat(ip.Address.ToString());
                                return ip.Address;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
