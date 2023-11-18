using System;
using System.Net;
using System.Net.Sockets;
using static NetShared.Net.TcpFramer;
using NetShared.Logging;
using NetShared.Net;
using NetShared.Events;
using NetShared.NetParser;
using NetServer.Authentication;
using System.Security.Cryptography;

namespace NetServer
{

    public delegate void OnServerStart();
    public delegate void OnServerStop();

    public class Server
    {
        public event OnServerStart ServerStart;
        public event OnServerStop ServerStop;


        public TcpComms ServerComms = default;
        public IParser Parser { get; set; }
        public Authenticator Authenticator { get; set; }

        public IPAddress ListeningAddress { get; set; } = IPAddress.Any;
        public int ListeningPort { get; set; } = 1337;

        public Server()
        {


        }

        public Server(TcpComms serverComms, IParser parser, Authenticator authenticator)
        {
            ServerComms = serverComms;
            Parser = parser;
            Authenticator = authenticator;
        }
        public Server(IPAddress listeningAddress, int port, TcpComms serverComms, IParser parser, Authenticator authenticator)
        {
            ListeningAddress = listeningAddress;
            ListeningPort = port;

            ServerComms = serverComms;
            ServerComms.TcpListener = new TcpListener(listeningAddress, port);

            Parser = parser;
            Authenticator = authenticator;

            AttachEvents();
        }

        ~Server()
        {
            DetachEvents();
        }

        public void AttachEvents()
        {
            ServerComms.ObjectReceived += ServerComms_ObjectReceived;
            ServerComms.HeaderReceived += ServerComms_HeaderReceived;
        }

       

        public void DetachEvents()
        {
            ServerComms.ObjectReceived -= ServerComms_ObjectReceived;
            ServerComms.HeaderReceived -= ServerComms_HeaderReceived;
        }

        private async void ServerComms_ObjectReceived(ObjectReceivedEventArgs eventArgs)
        {
            var receivedObject = ServerComms.Serializer.Deserialize<object>(eventArgs.ReceivedBytes, 0, eventArgs.ReceivedBytes.Length);

            // Destination is server, server authentication can be bypassed by having destination identity to 1, used only for authentication..
            if (eventArgs.SenderHeader.DestinationIdentity == ServerIdentity.CurrentIdentity || eventArgs.SenderHeader.DestinationIdentity == 1)
            {

                var response = await Parser.ParseType<ParserResponse>(new ParserArgs(eventArgs.SenderSocket, eventArgs.SenderHeader), receivedObject);
                if (response == default || response.Response == null)
                {
                    return;
                }


                // Send response to sender
                ServerComms.SendFramedResponse(eventArgs.SenderSocket, eventArgs.SenderHeader, response, ServerIdentity.CurrentIdentity);
            }
            else
            {
                // Destination is client
                var destination = Authenticator.IntIdentifierToIdentity[eventArgs.SenderHeader.DestinationIdentity];

                // Forward request to client
                ServerComms.SendFramedRequest(destination.ClientSocket, eventArgs.ReceivedBytes, eventArgs.SenderHeader.SenderIdentity, eventArgs.SenderHeader.DestinationIdentity, out Header header, eventArgs.SenderHeader.HashCodeIdentifier);
            }
        }

        private void ServerComms_HeaderReceived(Header header)
        { 
           // Console.WriteLine($"Received Header FL:{header.Flag}, SI:{header.SenderIdentity}, DI:{header.DestinationIdentity}");
        }

        

        public void Start()
        {
            
            ServerComms.TcpListener = new TcpListener(ListeningAddress, ListeningPort);


            ServerComms.TcpListener.Start();
            ServerComms.TcpListener.BeginAcceptTcpClient(ServerComms.BeginAcceptCallback, null);

            ServerStart?.Invoke();
        }

        public void Stop()
        {
            try
            {
                ServerComms.TcpListener.Stop();

                ServerStop?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

    }

}
