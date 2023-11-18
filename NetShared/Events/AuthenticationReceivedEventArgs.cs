
using NetShared.Net;
using NetShared.NetObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace NetShared.Events
{
    public class AuthenticationReceivedEventArgs : EventArgs
    {
        public bool AuthenticationStatus { get; set; }
        public StateObject ClientStateObject { get; set; }
        public AuthenticationRequest AuthenticationInformation { get; set; }
        public AuthenticationReceivedEventArgs(bool authStatus, AuthenticationRequest authInfo, StateObject clientStateObject)
        {
            AuthenticationStatus = authStatus;
            AuthenticationInformation = authInfo;
            ClientStateObject = clientStateObject;
        }
    }
}
