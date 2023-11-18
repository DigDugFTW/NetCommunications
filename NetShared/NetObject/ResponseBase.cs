using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetShared.Net.TcpFramer;

namespace NetShared.NetObject
{
    [Serializable]
    public class ResponseBase : MessageBase
    {
        public StatusCode StatusCode { get; set; } = StatusCode.NONE;
    }
}
