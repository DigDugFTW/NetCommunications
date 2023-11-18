using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.Events
{
    public class RequestSendEventArgs : EventArgs
    {
        public RequestSendEventArgs()
        {

        }

        public RequestSendEventArgs(Header requestHeader, byte[] framedBytes, byte[] rawBytes)
        {
            RequestHeader = requestHeader;
            FramedBytes = framedBytes;
            RawBytes = rawBytes;
        }

        public byte[] RawBytes { get; set; }
        public byte[] FramedBytes { get; set; }
        public Header RequestHeader { get; set; }

    }
}
