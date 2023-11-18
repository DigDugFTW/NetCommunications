using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetClient.Events
{
    public class RequestTimeoutEventArgs : EventArgs
    {
        public RequestTimeoutEventArgs()
        {

        }

        public int RetryCount { get; set; } = 0;
        public int HashCodeIdentifier { get; set; }

        public RequestTimeoutEventArgs(int hashCodeIdentifier)
        {
            HashCodeIdentifier = hashCodeIdentifier;
        }

    }
}
