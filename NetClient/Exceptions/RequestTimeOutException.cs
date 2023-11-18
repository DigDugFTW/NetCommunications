using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetClient.Exceptions
{

    [Serializable]
    public class RequestTimeOutException : Exception
    {
        public RequestTimeOutException() { }
        public RequestTimeOutException(string message) : base(message) { }
        public RequestTimeOutException(string message, Exception inner) : base(message, inner) { }
        protected RequestTimeOutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
