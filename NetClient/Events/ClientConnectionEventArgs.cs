using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetClient.Events
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public int ConnectionAttempts { get; set; } = 0;
        public string HostName { get; set; }
        public int HostPort { get; set; }
        public bool ConnectionSuccess { get; set; } = false;

        public object ServerIdentityReturn { get; set; }

        public ClientConnectionEventArgs(int connectionAttempts, string hostName, int port)
        {
            ConnectionAttempts = connectionAttempts;
            HostName = hostName;
            HostPort = port;
        }


    }
}
