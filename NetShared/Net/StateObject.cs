using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Net
{
    public class StateObject
    {


        public StateObject()
        {

        }
        
        /// <summary>
        /// Used to determine buffer size for send and receive operations.
        /// </summary>
        public const int BufferSize = 1024 * 50;

        /// <summary>
        /// Buffer used for send and receive operations, initial size is BufferSize
        /// </summary>
        public byte[] Buffer = new byte[BufferSize];

        /// <summary>
        /// TcpClient Socket
        /// </summary>
        public Socket WorkSocket { get; set; }

        /// <summary>
        /// Data receive size. Size read via header that indicates the full receive or send size in bytes.
        /// </summary>
        public int Size = 0;

        /// <summary>
        /// Number of bytes handled during a send or receive call.
        /// </summary>
        public int BytesHandled = 0;

        /// <summary>
        /// Clients current header. Read at the beginning of a receive if header reading is enabled.
        /// </summary>
        public TcpFramer.Header CurrentHeader { get; set; }

        /// <summary>
        /// Resets the state objects fields without having to create a new state object.
        /// </summary>
        public void Reset()
        {
            Buffer = new byte[BufferSize];
            Size = 0;
            BytesHandled = 0;
        }

    }
}
