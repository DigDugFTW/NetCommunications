using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Events
{
    public class ClientErrorEventArgs : EventArgs
    {
        public Socket Client { get; set; }
        public Exception Exception { get; set; }

        public ClientErrorEventArgs()
        {

        }

        public ClientErrorEventArgs(Socket socket, Exception e) 
        {
            Client = socket;
            Exception = e;
        }

    }
}
