using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetShared.Exceptions
{
    public class SerializationException : Exception
    {
        public SerializationException(string message) : base(message)
        {
           
        }
    }
}
