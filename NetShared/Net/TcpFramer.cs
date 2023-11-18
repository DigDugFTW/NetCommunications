using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace NetShared.Net
{

    /// <summary>
    /// Prepares data in a specified format to be sent
    /// </summary>
    public class TcpFramer : ITcpFramer
    {
        public enum StatusCode
        {
            OK = 200, ERROR = 303, NONE = 0, TIME_OUT = 101
        }

        public enum Flag
        {
            REQ = 21, REP = 41, EOS = 999, BOS = 900, NONE = 0, ACK = 100, KEEP_ALIVE = 300, AUTH = 351, KICK = 451
        }

        public int HeaderAllocation { get; set; } = 24;

        public Header CreateHeader(byte[] body, StatusCode statusCode, Flag flag, int dataHash, int identity, int destination)
        {
            var header = new Header()
            {
                FlagBytes = BitConverter.GetBytes((int)flag),
                StatusCodeBytes = BitConverter.GetBytes((int)statusCode),
                HashCodeIdentifierBytes = BitConverter.GetBytes(dataHash),
                SenderIdentityBytes = BitConverter.GetBytes(identity),
                DestinationIdentityBytes = BitConverter.GetBytes(destination)
            };

            if (body == null)
            {
                body = new byte[0];
            }

            header.DataLengthBytes = BitConverter.GetBytes(body.Length + HeaderAllocation);

            return header;
        }

        public byte[] PrependHeader(byte[] body, Header header)
        {
            byte[] data = new byte[body.Length + HeaderAllocation];
            byte[] headerBytes = header.HeaderBytes;

            // Header
            Buffer.BlockCopy(headerBytes, 0, data, 0, HeaderAllocation);

            // Body
            Buffer.BlockCopy(body, 0, data, HeaderAllocation, body.Length);

            return data;
        }

        /// <summary>
        /// Reads header from stream of bytes
        /// Pulls HeaderAllocation byte amount and copies the bytes into a header object.
        /// </summary>
        /// <param name="data">Array to read from.</param>
        /// <returns>Header object read from byte stream. </returns>
        public Header ReadHeader(byte[] data)
        {
            var header = new Header();
            Buffer.BlockCopy(data, 0, header.DataLengthBytes, 0, 4);
            Buffer.BlockCopy(data, 4, header.StatusCodeBytes, 0, 4);
            Buffer.BlockCopy(data, 8, header.FlagBytes, 0, 4);
            Buffer.BlockCopy(data, 12, header.HashCodeIdentifierBytes, 0, 4);
            Buffer.BlockCopy(data, 16, header.SenderIdentityBytes, 0, 4);
            Buffer.BlockCopy(data, 20, header.DestinationIdentityBytes, 0, 4);
            return header;

        }

        // prepares data received in response to a request
        // copies sender bodyHash / sets destination to sender identity
        public byte[] CreateFramedResponse(Header senderHeader, StatusCode statusCode, Flag flag, int identity, byte[] responseObject)
        {
            Header responseHeader = CreateHeader(responseObject, statusCode, flag, senderHeader.HashCodeIdentifier, identity, senderHeader.SenderIdentity);
            byte[] fullResponseBytes = PrependHeader(responseObject, responseHeader);
            return fullResponseBytes;
        }

        // prepares data to be sent
        public byte[] CreateFramedSend(byte[] sendBytes, StatusCode statusCode, Flag flag, int identity, int destination, out Header header, int bodyHash = 0)
        {
            if (bodyHash == 0)
            {
                bodyHash = sendBytes.GetHashCode() ^ new Random().Next(1, int.MaxValue);
            }

            header = CreateHeader(sendBytes, statusCode, flag, bodyHash, identity, destination);

            byte[] fullRequestBytes = PrependHeader(sendBytes, header);
            return fullRequestBytes;
        }

        public class Header
        {
            public int HeaderAllocation { get; set; } = 24;
            public Header()
            {

            }

            public Header(int headerAllocation = 24)
            {
                HeaderAllocation = headerAllocation;
            }
            // Length of data to be sent in total (given fragmentation and order via tcp)
            public byte[] DataLengthBytes { get; set; } = new byte[4]; 

            public byte[] StatusCodeBytes { get; set; } = new byte[4];
            public byte[] FlagBytes { get; set; } = new byte[4];
            public byte[] HashCodeIdentifierBytes { get; set; } = new byte[4];
            public byte[] SenderIdentityBytes { get; set; } = new byte[4];
            public byte[] DestinationIdentityBytes { get; set; } = new byte[4];


            private int _dataLength = 0;
            public int DataLength
            {
                get
                {
                    if (_dataLength == 0)
                    {
                        return BitConverter.ToInt32(DataLengthBytes, 0);
                    }return _dataLength;
                }
                set { _dataLength = value;}
            }

            private int _statusCode = 0;

            public int StatusCode
            {
                get { if (_statusCode == 0) { return BitConverter.ToInt32(StatusCodeBytes, 0); } return _statusCode; }
                set { _statusCode = value; }
            }

            private int _flag = 0;
            public int Flag
            {
                get
                {
                    if (_flag == 0)
                    {
                        return BitConverter.ToInt32(FlagBytes, 0);
                    }
                    return _flag;
                }
                set
                {
                    _flag = value;
                }
            }

            private int _hashCodeIdentifier = 0;
            public int HashCodeIdentifier
            {
                get
                {
                    if (_hashCodeIdentifier == 0)
                    {
                        return BitConverter.ToInt32(HashCodeIdentifierBytes, 0);
                    }
                    return _hashCodeIdentifier;
                }
                set
                {
                    _hashCodeIdentifier = value;
                }
            }

            private int _senderIdentity = 0;
            public int SenderIdentity
            {
                get
                {
                    if (_senderIdentity == 0)
                    {
                        return BitConverter.ToInt32(SenderIdentityBytes, 0);
                    }
                    return _senderIdentity;
                }
                set
                {
                    _senderIdentity = value;
                }
            }

            private int _destinationIdentity = 0;
            public int DestinationIdentity
            {
                get
                {
                    if (_destinationIdentity == 0)
                    {
                        return BitConverter.ToInt32(DestinationIdentityBytes, 0);
                    }
                    return _destinationIdentity;
                }
                set
                {
                    _destinationIdentity = value;
                }
            }

            private byte[] _headerBytes;

            public byte[] HeaderBytes
            {
                get { if (_headerBytes == null) { _headerBytes = GetHeaderBytes(); } return _headerBytes; }
                set { _headerBytes = value; }
            }

            public virtual byte[] GetHeaderBytes()
            {
                byte[] headerBytes = new byte[HeaderAllocation];
                Buffer.BlockCopy(DataLengthBytes, 0, headerBytes, 0, 4);
                Buffer.BlockCopy(StatusCodeBytes, 0, headerBytes, 4, 4);
                Buffer.BlockCopy(FlagBytes, 0, headerBytes, 8, 4);
                Buffer.BlockCopy(HashCodeIdentifierBytes, 0, headerBytes, 12, 4);
                Buffer.BlockCopy(SenderIdentityBytes, 0, headerBytes, 16, 4);
                Buffer.BlockCopy(DestinationIdentityBytes, 0, headerBytes, 20, 4);
                return headerBytes;
            }

        }




    }


}
