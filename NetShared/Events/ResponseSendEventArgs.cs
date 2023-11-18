using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.Events
{
    public class ResponseSendEventArgs : EventArgs
    {
        public ResponseSendEventArgs()
        {

        }

        public ResponseSendEventArgs(Header responseHeader)
        {
            ResponseHeader = responseHeader;
        }

        public Header ResponseHeader { get; set; }
    }
}
