using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.Events
{
    public class ObjectReceivedEventArgs : EventArgs
    {
        public ObjectReceivedEventArgs()
        {

        }

        public ObjectReceivedEventArgs(Socket senderSocket, Header senderHeader,  byte[] bytes)
        {
            SenderSocket = senderSocket;
            SenderHeader = senderHeader;
            
            ReceivedBytes = bytes;
        }

        public Socket SenderSocket { get; private set; }
        public Header SenderHeader { get; private set; }
     
        public byte[] ReceivedBytes { get; set; }


    }
}
