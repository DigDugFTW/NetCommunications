using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.NetObject
{
    /// <summary>
    /// Serves as the base class for all objects that are sent over the network.
    /// </summary>
    [Serializable]
    public class MessageBase
    {
        public string SenderStringIdentifier { get; set; }
        public string DestinationStringIdentifier { get; set; }

        public bool HasError { get; set; }
        public Exception Exception { get; set; } = new Exception();

        public MessageBase()
        {

        }

    }
}
