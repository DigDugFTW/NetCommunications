using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.NetObject
{
    /// <summary>
    /// Class sent by client in a request
    /// Used to lookup destination identifier
    /// </summary>
    
    [Serializable]
    public class RequestMessage : RequestBase
    {

        public RequestMessage()
        {

        }

        public RequestMessage(string request, params object[] args)
        {
            RequestObject = request;
            RequestParameters = args;
        }

        public string RequestObject { get; set; }
        public object[] RequestParameters { get; set; }
    }
}
