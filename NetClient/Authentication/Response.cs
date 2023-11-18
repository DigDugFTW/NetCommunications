using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetClient.Authentication
{
    public class Response<T>
    {
        public Response()
        {

        }

        public Response(T receivedObject, int sender, StatusCode status)
        {
            ReceivedObject = receivedObject;
            Sender = sender;
            Status = status;
        }

        public Response(T receivedObject, int sender, StatusCode status, double responseTime)
        {
            ReceivedObject = receivedObject;
            Sender = sender;
            Status = status;
            ResponseTime = responseTime;
        }

        public double ResponseTime { get; set; }

        public T ReceivedObject { get; set; }
        public int Sender { get; set; }
        public StatusCode Status { get; set; }
    }
}
