using NetShared.Events;
using NetShared.Logging;
using NetShared.NetParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

using static NetShared.Net.TcpFramer;

namespace NetShared.Net
{


    public delegate void OnResponseSend(ResponseSendEventArgs args);
    public delegate void OnRequestSend(RequestSendEventArgs args);
    public delegate void OnAsyncSendComplete();

    public delegate void OnObjectReceived(ObjectReceivedEventArgs eventArgs);
    public delegate void OnHeaderReceived(Header header);

    // Server Specific Events
    public delegate void OnClientConnect(ClientInformationEventArgs eventArgs);
    public delegate void OnClientError(ClientErrorEventArgs eventArgs);
    public delegate void OnServerStart();
    public delegate void OnServerStop();
    //

    // Client Specific Events
    public delegate void OnConnectionLost();
    //

    public class TcpComms
    {
        #region Events
        public event OnResponseSend ResponseSend;
        public event OnRequestSend RequestSend;
        public event OnAsyncSendComplete AsyncSendComplete;

        public event OnObjectReceived ObjectReceived;
        public event OnHeaderReceived HeaderReceived;

        public event OnClientConnect ClientConnect;
        public event OnClientError ClientError;

        public event OnConnectionLost ConnectionLost;

        #endregion


        public TcpClient TcpClient { get; set; }
        public TcpListener TcpListener { get; set; }

        public int SentBytes { get; set; }
        public int ReceivedBytes { get; set; }

        public ISerializer Serializer { get; set; }
        public ITcpFramer TcpFramer { get; set; }

        public enum Role
        {
            SERVER, CLIENT
        }

        public TcpComms()
        {

        }

        public TcpComms(ISerializer serializer, ITcpFramer framer)
        {
            Serializer = serializer;
            TcpFramer = framer;
        }



        public void SendFramedRequest(Socket sock, object request, int senderIdentity, int destination, out Header header, int bodyHash = 0)
        {
            header = default;
            try
            {
                // Dirty. Optimization to skip serialization when sending already serialized data
                byte[] requestObjectBytes;


                if(request is byte[])
                 {
                    requestObjectBytes = request as byte[];
                }
                 else
                {
                    requestObjectBytes = Serializer.Serialize(request);
                }


                byte[] framedSendBytes = TcpFramer.CreateFramedSend(requestObjectBytes, StatusCode.NONE, Flag.REQ, senderIdentity, destination, out Header requestHeader, bodyHash);
                header = requestHeader;
                SendFramedAsync(framedSendBytes, sock);
                RequestSend?.Invoke(new RequestSendEventArgs(header, framedSendBytes, requestObjectBytes));
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }

        }

        // Send response back to sender
        // Set destination identity to sender
        // Set      sender identity to param:identity
        public void SendFramedResponse(Socket sock, Header senderHeader, ParserResponse response, int identity)
        {
            try
            {
                if (response.Response == null)
                {
                    return;
                }

                byte[] responseBytes = Serializer.Serialize(response.Response);
                byte[] framedResponseBytes = TcpFramer.CreateFramedResponse(senderHeader, response.StatusCode, Flag.REP, identity, responseBytes);
                SendFramedAsync(framedResponseBytes, sock);
                ResponseSend?.Invoke(new ResponseSendEventArgs(senderHeader));
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }

        }

        public int SendFramed(byte[] framedData, Socket sock)
        {
            int sent = sock.Send(framedData);
            SentBytes += sent;
            return sent;
        }



        // Used for light weight status updates.. 
        public void SendHeader(Header header, Socket sock)
        {
            SendFramedAsync(header.HeaderBytes, sock);
        }

        public void SendFlag(Flag flag, int dataHash, int senderIdentity, int destinationIdentity)
        {
            var flagHeader = TcpFramer.CreateHeader(null, StatusCode.NONE, flag, dataHash, senderIdentity, destinationIdentity);
            SendFramedAsync(flagHeader.GetHeaderBytes(), TcpClient.Client);
        }

        public void SendFramedAsync(byte[] framedData, Socket sock)
        {
            var stateObject = new StateObject()
            {
                WorkSocket = sock,
                Size = framedData.Length
            };

            SentBytes += stateObject.Size;
            stateObject.Buffer = framedData;

            stateObject.WorkSocket.BeginSend(stateObject.Buffer, 0, framedData.Length, SocketFlags.None, BeginSendCallback, stateObject);
        }



        private readonly object _stateObjectLock = new object();

        private void BeginSendCallback(IAsyncResult result)
        {
            var stateObject = (StateObject)result.AsyncState;
            int bytesSent = stateObject.WorkSocket.EndSend(result);

            lock (_stateObjectLock)
            {
                stateObject.BytesHandled += bytesSent;
            }


            if (stateObject.BytesHandled < stateObject.Size)
            {
                //stateObject.WorkSocket.BeginSend(stateObject.Buffers.First(), stateObject.BytesHandled, stateObject.Size, SocketFlags.None, BeginSendCallback, stateObject);
                stateObject.WorkSocket.BeginSend(stateObject.Buffer, bytesSent, stateObject.Size - bytesSent, SocketFlags.None, BeginSendCallback, stateObject);
            }
            else if (stateObject.BytesHandled == stateObject.Size)
            {
                lock (_stateObjectLock)
                {
                    stateObject.Reset();
                }
                // Send is complete 
                AsyncSendComplete?.Invoke();
            }

        }

        // Used in server and for client to client connections
        public virtual void BeginAcceptCallback(IAsyncResult result)
        {

            TcpClient client = TcpListener.EndAcceptTcpClient(result);

            ClientConnect?.Invoke(new ClientInformationEventArgs(client, null));

            var stateObject = new StateObject()
            {
                WorkSocket = client.Client
            };


            //stateObject.WorkSocket.BeginReceive(stateObject.Buffers.First(), 0, StateObject.BufferSize, SocketFlags.None, BeginReceiveCallback, stateObject);
            stateObject.WorkSocket.BeginReceive(stateObject.Buffer, 0, StateObject.BufferSize, SocketFlags.None, BeginReceiveCallback, stateObject);

            TcpListener.BeginAcceptTcpClient(BeginAcceptCallback, null);
        }


        // Used in both server and client
        public void BeginReceiveCallback(IAsyncResult result)
        {
            var stateObject = (StateObject)result.AsyncState;
            int dataReceiveSize = 0;

            try
            {
                dataReceiveSize = stateObject.WorkSocket.EndReceive(result);
                ReceivedBytes += dataReceiveSize;
                lock (_stateObjectLock)
                {
                    stateObject.BytesHandled += dataReceiveSize;
                }


                #region Header reading


                if (stateObject.Size == 0)
                {

                    //var bytes = ConvertToByteArray(stateObject.ReceiveBuffer);
                    var bytes = stateObject.Buffer;
                    var header = TcpFramer.ReadHeader(bytes);
                    Logger.LogDebug($"Header => Id:{header.HashCodeIdentifier}, Len:{header.DataLength}, Sender:{header.SenderIdentity}, Dest:{header.DestinationIdentity}", newLineBefore: true);


                    if (header.DataLength > 0)
                    {
                        lock (_stateObjectLock)
                        {
                            stateObject.Size = header.DataLength;
                            stateObject.CurrentHeader = header;
                        }
                    }
                    else
                    {
                        ClientError?.Invoke(new ClientErrorEventArgs(stateObject.WorkSocket, new IndexOutOfRangeException($"Header data length didn't fall within expected value: {header.DataLength}")));
                    }

                }

                #endregion


                Logger.LogDebug($"Client[{stateObject.CurrentHeader.SenderIdentity}] => [{stateObject.CurrentHeader.DestinationIdentity}]");
                if (stateObject.BytesHandled != 0 && stateObject.BytesHandled == stateObject.Size)
                {
                    //Logger.LogDebug($"SO => Size:{stateObject.Size}, BytesHandled:{stateObject.BytesHandled}", newLineAfter: true);
                    // Testing..
                    if (stateObject.BytesHandled == TcpFramer.HeaderAllocation)
                    {
                        HeaderReceived?.Invoke(stateObject.CurrentHeader);
                    }
                    else
                    {

                        byte[] tmpBuff = new byte[stateObject.Size];

                        //byte[] totalBytes = Combine(stateObject.Buffers.ToArray());
                        //byte[] totalBytes = ConvertToByteArray(stateObject.ReceiveBuffer);


                        Buffer.BlockCopy(stateObject.Buffer, TcpFramer.HeaderAllocation, tmpBuff, 0, stateObject.Size);

                        ObjectReceived?.Invoke(new ObjectReceivedEventArgs(stateObject.WorkSocket, stateObject.CurrentHeader, tmpBuff));
                    }

                    lock (_stateObjectLock)
                    {
                        stateObject.Reset();
                    }
                }


                int recSize = stateObject.Size == 0 ? StateObject.BufferSize : stateObject.BytesHandled - stateObject.Size;
                int offSet = stateObject.BytesHandled;

                Logger.LogDebug($"Offset: {offSet}, Size: {recSize}, #Handled: {stateObject.BytesHandled}");
                //stateObject.WorkSocket.BeginReceive(stateObject.Buffers[stateObject.BufferIndex], offSet, recSize, SocketFlags.None, BeginReceiveCallback, stateObject);
                stateObject.WorkSocket.BeginReceive(stateObject.Buffer, offSet, recSize, SocketFlags.None, BeginReceiveCallback, stateObject);

            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // Change this..
                    ConnectionLost?.Invoke();
                }
            }

            catch (Exception e)
            {
                ClientError?.Invoke(new ClientErrorEventArgs(stateObject.WorkSocket, e));
            }
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        public byte[] ConvertToByteArray(IList<ArraySegment<byte>> list)
        {
            var bytes = new byte[list.Sum(asb => asb.Count)];
            int pos = 0;

            foreach (var asb in list)
            {
                Buffer.BlockCopy(asb.Array, asb.Offset, bytes, pos, asb.Count);
                pos += asb.Count;
            }

            return bytes;
        }



    }
}
