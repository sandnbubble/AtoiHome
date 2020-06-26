using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AtoiHomeServiceLib.Source.Utility
{
    public class DNSInfo
    {
        /// <summary>
        /// nslookup을 프로세스로 실행시키고 표준출력을 redirection해서 공인IP를 얻어오는 루틴인데
        /// "권한 없는 응답:" 라인이 원인은 알 수 없지만 redirection이 안되고 service console에 출력됨
        /// 다른 방법을 찾아야할듯
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetPublicIP()
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
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if (ni.NetworkInterfaceType == nicType)
                            {
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
