using NetShared.NetObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.NetParser
{
    [Serializable]
    public class ParserResponse : ResponseBase
    {
        public ParserResponse()
        {

        }

        public ParserResponse(object response, StatusCode statusCode)
        {
            Response = response;
            base.StatusCode = statusCode;
        }

        
        public object Response { get; set; }
       

    }
}
