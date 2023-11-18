using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NetShared
{
    /// <summary>
    /// Static instace for easy network extension methods
    /// </summary>
    public static class NetHelper
    {
        public static IPAddress GetPublicIp()
        {
            using(WebClient client = new WebClient())
            {
                
                string ip = client.DownloadString("https://ipinfo.io/ip").Replace("\n", string.Empty);
                
                bool isValid = IPAddress.TryParse(ip, out IPAddress validAddress);
                if (isValid)
                {
                    return validAddress;
                }
                else
                {
                    throw new Exception("Couldn't obtain public ip address");
                }
            }
        }

        /// <summary>
        /// Issue with returning correct address when multiple NIC's are present. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IPAddress GetPrivateIp()
        {
            var hostEntries = Dns.GetHostEntry(IPAddress.Loopback);
            
            foreach(var addr in hostEntries.AddressList)
            {
                if(addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {

                    return addr;
                }
            }
            throw new Exception("Couldn't obtain private ip address");
        }

        public static PhysicalAddress GetMacAddress()
        {
            foreach(var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    return networkInterface.GetPhysicalAddress();
                }
            }
            throw new Exception("Couldn't obtain mac address");
        }

        public static bool CheckNetworkConnection(out long msRtt)
        {
            msRtt = 0;
            using(var ping = new Ping()) 
            {
                const string host = "8.8.8.8";
                const int timeout = 1000;
                try
                {
                    PingReply pingReply = ping.Send(host, timeout);

                    msRtt = pingReply.RoundtripTime;

                    return pingReply.Status == IPStatus.Success;
                }catch(Exception e)
                {
                    msRtt = -1;
                    return false;
                }
            }
        }

        
         
    }
}
