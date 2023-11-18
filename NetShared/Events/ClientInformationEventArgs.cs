
using NetShared.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Events
{
    public class ClientInformationEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
        public StateObject StateObject { get; set; } = default;

        public ClientInformationEventArgs()
        {

        }

        public ClientInformationEventArgs(TcpClient client)
        {
            Client = client;
        }

        public ClientInformationEventArgs(TcpClient client, StateObject stateObject)
        {
            Client = client;
            StateObject = stateObject;
        }

    }
}
