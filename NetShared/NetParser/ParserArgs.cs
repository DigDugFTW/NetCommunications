using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.NetParser
{
    public class ParserArgs
    {
        public ParserArgs()
        {

        }

        public ParserArgs(Socket senderSock, Header senderHeader)
        {
            SenderSocket = senderSock;
            SenderHeader = senderHeader;
        }

        public Socket SenderSocket { get; set; }
        public Header SenderHeader { get; set; }

    }
}
