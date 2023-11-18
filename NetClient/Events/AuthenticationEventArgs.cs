using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetClient.Events
{
    public class AuthenticationEventArgs : EventArgs
    {
        public bool Success { get; set; }
    }
}
