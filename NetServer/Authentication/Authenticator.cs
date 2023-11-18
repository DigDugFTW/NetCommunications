

using NetShared.Logging;
using NetShared.NetObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetServer.Authentication
{
    public delegate void OnClientAdd(ClientIdentity identity);
    public delegate void OnClientRemove(ClientIdentity identity);

    /// <summary>
    /// Manages server authentication for clients, stores authenticated user information.
    /// </summary>
    public class Authenticator
    {
       
        public event OnClientAdd ClientAdd;
        public event OnClientRemove ClientRemove;

        public string ServerPassword = "helloworld";
        public string ServerStringIdentity { get; set; } = "server";
        

        public Dictionary<Socket, ClientIdentity> UsersSocketToIdentity = new Dictionary<Socket, ClientIdentity>();
        public Dictionary<string, Socket> StringIdentifierToSocket = new Dictionary<string, Socket>();
        public Dictionary<int, ClientIdentity> IntIdentifierToIdentity = new Dictionary<int, ClientIdentity>();

        public virtual bool AuthCheck(AuthenticationRequest authRequest)
        {
            string username = authRequest.Username;
            string password = authRequest.Password;


            if (password.Equals(ServerPassword) && !string.IsNullOrWhiteSpace(username) && !StringIdentifierToSocket.ContainsKey(username))
            {
                return true;
               
            }
            return false;
        }

        public void AddAuthenticatedUser(ClientIdentity identity)
        {
            UsersSocketToIdentity.Add(identity.ClientSocket, identity);
            StringIdentifierToSocket.Add(identity.Username, identity.ClientSocket);
            IntIdentifierToIdentity.Add(identity.Identity, identity);
            ClientAdd?.Invoke(identity);
        }

        public void RemoveAuthenticatedUser(ClientIdentity identity)
        {
            UsersSocketToIdentity.Remove(identity.ClientSocket);
            StringIdentifierToSocket.Remove(identity.Username);
            IntIdentifierToIdentity.Remove(identity.Identity);
            ClientRemove?.Invoke(identity);
        }

        // Send kick message..
        public void KickUser(ClientIdentity identity)
        {
            identity.ClientSocket.Shutdown(SocketShutdown.Both);
            identity.ClientSocket.Close(1000);
           
            RemoveAuthenticatedUser(identity);
        }

    }
}
