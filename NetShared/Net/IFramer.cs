namespace NetShared.Net
{
    public interface ITcpFramer
    {
        byte[] CreateFramedResponse(TcpFramer.Header senderHeader, TcpFramer.StatusCode statusCode, TcpFramer.Flag flag, int identity, byte[] responseObject);
        byte[] CreateFramedSend(byte[] sendBytes, TcpFramer.StatusCode statusCode, TcpFramer.Flag flag, int identity, int destination, out TcpFramer.Header header, int bodyHash = 0);
        TcpFramer.Header CreateHeader(byte[] body, TcpFramer.StatusCode statusCode, TcpFramer.Flag flag, int dataHash, int identity, int destination);
        byte[] PrependHeader(byte[] body, TcpFramer.Header header);
        TcpFramer.Header ReadHeader(byte[] data);

         int HeaderAllocation { get; set; }
    }
}