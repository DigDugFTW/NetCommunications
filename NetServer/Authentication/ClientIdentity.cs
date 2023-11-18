using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetServer.Authentication
{
    public class ClientIdentity
    {
        public ClientIdentity()
        {

        }

        public ClientIdentity(Socket clientSocket, int identity)
        {
            ClientSocket = clientSocket;
            Identity = identity;
        }

        public Socket ClientSocket { get; set; }
        public int Identity { get; set; }
        public string Username { get; set; }

    }
}
