using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetServer;
using NetServer.Authentication;
using NetShared;
using NetShared.Events;
using NetShared.Logging;
using NetShared.Net;
using NetShared.NetObject;
using NetShared.NetParser;
using static NetShared.Net.TcpFramer;

// TODO make method implementation dynamic based on dll reference
 // --dll ref based on set implementation of dll--boiler plate that creates
 // recognizable* entry and query regions with the dynamic link library.
// 
namespace NetServerConsole
{
    class Program
    {
        private static Authenticator _authenticator = new Authenticator();
        private static Parser _parser = new Parser();
        private static TcpComms _tcpComms = default;

        public static Server NetServer = default;
        static void Main(string[] args)
        {
            //Logger.OnExceptionLogCalled += Logger_OnExceptionLogCalled;
            //Logger.OnMessageLogCalled += Logger_OnMessageLogCalled;
            // ***COMMENTED for exessive server console output***
            //Logger.OnDebugLogCalled += Logger_OnDebugLogCalled;

            _tcpComms = new TcpComms(new Serializer(), new TcpFramer());
            NetServer = new Server(IPAddress.Any, 1337, _tcpComms, _parser, _authenticator);

            
            _tcpComms.TcpListener.Server.SendBufferSize = 50 * 1024;
            _tcpComms.TcpListener.Server.ReceiveBufferSize = 50 * 1024;
            _tcpComms.TcpListener.Server.SendTimeout = 10000;
            _tcpComms.TcpListener.Server.ReceiveTimeout = 10000;

            // Make methods dynamic--
             // config that contains method references 
             // Points to dll that implements interface
             // interface loads all expected handles to methods with sig details
             // --server loads dynamic methods, executes methods based on permission
             // permission based on role
            _parser.AddHandler<RequestMessage, Task<ParserResponse>>(RequestMessageHandler)
                .AddChild("para", doGetParagraph)
                .AddChild("list", doClientList)
                .AddChild("chat", doChat)
                .AddChild("dm", doDirectMessage)
                .AddChild("echo", doEcho)
                .AddChild("help", doHelp)
                .AddChild("rand", doRandomNumber)
                .AddChild("getMachineName", doGetUserMachineName)
                .AddChild("math", doMath)
                .AddChild("delay", doDelayResponse)
                .AddChild("disconnect", doDisconnect)
                .AddChild("bcast", doBroadCastCommand)
                .AddChild("ping", doPing)
                .AddChild("rtt", doRttRequest);

            _parser.AddHandler<AuthenticationRequest, ParserResponse>(AuthenticationHandler);

            _tcpComms.ClientConnect += _tcpComms_ClientConnect;
            _tcpComms.ClientError += _tcpComms_ClientError;


            NetServer.ServerStart += NetServer_ServerStart;
            NetServer.ServerStop += NetServer_ServerStop;

            _authenticator.ClientRemove += ServerAuthenticator_ClientRemove;
            _authenticator.ClientAdd += ServerAuthenticator_ClientAdd;

            Console.WriteLine("Press enter to start server...");
            Console.ReadLine();

            NetServer.Start();



            Console.ReadLine();
        }

        private static void _tcpComms_ClientError(NetShared.Events.ClientErrorEventArgs eventArgs)
        {
            if (_authenticator.UsersSocketToIdentity.ContainsKey(eventArgs.Client))
            {
                var identity = _authenticator.UsersSocketToIdentity[eventArgs.Client];
                _authenticator.KickUser(identity);
            }
        }

        private static void _tcpComms_ClientConnect(NetShared.Events.ClientInformationEventArgs eventArgs)
        {

        }

        private async static void RunPoller(int interval)
        {
            Console.Title = $"Download Bytes/s: {_tcpComms.ReceivedBytes} ({(_tcpComms.ReceivedBytes / 1024) / 1024}MB), Upload Bytes/s: {_tcpComms.SentBytes} ({(_tcpComms.SentBytes / 1024) / 1024}MB)";
            _tcpComms.ReceivedBytes = 0;
            _tcpComms.SentBytes = 0;
            await Task.Delay(interval);
            RunPoller(interval);
        }



        private static void NetServer_ServerStart()
        {
            Console.WriteLine($"Starting server... {DateTime.Now}, {NetServer.ListeningAddress}:{NetServer.ListeningPort}");
            RunPoller(1000);
        }
        private static void NetServer_ServerStop()
        {
            Console.WriteLine($"Stopping server... {DateTime.Now}");
        }


        private static void ServerAuthenticator_ClientAdd(ClientIdentity identity)
        {
            Logger.LogDebug($"Client Add: {identity.Username}");
            foreach (var c in _authenticator.IntIdentifierToIdentity)
            {
                if (c.Key == identity.Identity)
                    continue;


                _tcpComms.SendFramedRequest(c.Value.ClientSocket, new RequestMessage("client_add") { SenderStringIdentifier = identity.Username }, c.Key, 0, out Header header);
            }
        }

        private static void ServerAuthenticator_ClientRemove(ClientIdentity identity)
        {
            Logger.LogDebug($"Client Remove: {identity.Username}");
            foreach (var c in _authenticator.IntIdentifierToIdentity)
            {
                if (c.Key == identity.Identity)
                    continue;

                _tcpComms.SendFramedRequest(c.Value.ClientSocket, new RequestMessage("client_remove") { SenderStringIdentifier = identity.Username }, c.Key, 0, out Header header);
            }
        }

        #region Parser Methods

        private static ParserResponse AuthenticationHandler(ParserArgs args, AuthenticationRequest authentication)
        {
            var parserResponse = new ParserResponse(-1, StatusCode.ERROR);
            bool pass = _authenticator.AuthCheck(authentication);
            if (pass)
            {
                var cId = new ClientIdentity(args.SenderSocket, args.SenderHeader.SenderIdentity)
                {
                    Username = authentication.Username
                };

                _authenticator.AddAuthenticatedUser(cId);
                parserResponse.Response = ServerIdentity.CurrentIdentity;
                parserResponse.StatusCode = StatusCode.OK;
            }

            return parserResponse;
        }

        private static async Task<ParserResponse> RequestMessageHandler(ParserArgs args, RequestMessage requestMessage)
        {
            Console.WriteLine($"{requestMessage.SenderStringIdentifier}({args.SenderHeader.SenderIdentity}) -> {requestMessage.DestinationStringIdentifier}({args.SenderHeader.DestinationIdentity})");
            if (requestMessage.DestinationStringIdentifier.Equals(_authenticator.ServerStringIdentity))
            {
                return await _parser.CallHandlerChild<RequestMessage, ParserResponse, string>(requestMessage.RequestObject.ToString(), args, requestMessage);
            }
            // Could lookup by int identifier..
            bool identifierPresent = _authenticator.StringIdentifierToSocket.TryGetValue(requestMessage.DestinationStringIdentifier, out Socket destinationSocket);
            if (identifierPresent)
            {
                _tcpComms.SendFramedRequest(destinationSocket, requestMessage, args.SenderHeader.SenderIdentity, args.SenderHeader.DestinationIdentity, out Header header, args.SenderHeader.HashCodeIdentifier);
                return default;
            }
            return new ParserResponse("Handler couldn't find destination..", StatusCode.ERROR) { HasError = true };
        }

        private static ParserResponse doDisconnect(ParserArgs args, RequestMessage request)
        {

            try
            {
                int delayMs = int.Parse(request.RequestParameters[0].ToString());
                Thread.Sleep(delayMs);
                var senderIdentity = _authenticator.IntIdentifierToIdentity[args.SenderHeader.SenderIdentity];

                _authenticator.KickUser(senderIdentity);
                return default;
            }
            catch (Exception e)
            {
                return new ParserResponse(e.Message, StatusCode.ERROR);
            }


        }

        private static ParserResponse doDelayResponse(ParserArgs args, RequestMessage request)
        {
            try
            {
                int delayMs = int.Parse(request.RequestParameters[0].ToString());
                Thread.Sleep(delayMs);
                return new ParserResponse($"Delay response of {delayMs}", StatusCode.OK);
            }
            catch (Exception e)
            {
                return new ParserResponse(e.Message, StatusCode.ERROR);
            }


        }

        private static ParserResponse doMath(ParserArgs args, RequestMessage request)
        {
            try
            {
                double num1 = double.Parse(request.RequestParameters[0].ToString());
                double num2 = double.Parse(request.RequestParameters[1].ToString());
                return new ParserResponse($"{num1 * num2}", StatusCode.OK);
            }
            catch (Exception e)
            {
                return new ParserResponse(e.Message, StatusCode.ERROR);
            }


        }

        private static ParserResponse doGetUserMachineName(ParserArgs args, RequestMessage request)
        {
            try
            {
                Socket destSocket = _authenticator.StringIdentifierToSocket[request?.RequestParameters[0].ToString()];
                ClientIdentity destIdentity = _authenticator.UsersSocketToIdentity[destSocket];
                //TcpComms.SendFramedRequest(destSocket, request, args.SenderHeader.SenderIdentity, destIdentity.Identity, out Header header, args.SenderHeader.HashCodeIdentifier);
                //_tcpComms.SendFramedRequest(destSocket, request, args.SenderHeader.SenderIdentity, destIdentity.Identity, out Header header, args.SenderHeader.HashCodeIdentifier);
                return new ParserResponse(Environment.UserName, StatusCode.OK);  
            }
            catch (Exception e)
            {
                return new ParserResponse(e.Message, StatusCode.ERROR);
            }

        }

        private static ParserResponse doGetParagraph(ParserArgs args, RequestMessage request)
        {
            string[] _paragraphs =
       {
            "Miachael is gae, michale doest kno dae wae, mgithel gaaaaaaae. Miachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaaeMiachael is gae, michale doest kno dae wae, mgithel gaaaaaaae",

            "You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.",
            "You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.You are gay. Gay you are. You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!. re gay!.  Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay! ay. Retard.You're gay!. Gay gay. Retard.You're gay!.re gay!. Gay gay. Retard.You're gay!. Gay gay. Retard.You're gay!.. Gay gay. Retard.You're gay!.",

            "Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia, looked up one of the more obscure Latin words, consectetur, from a Lorem Ipsum passage, and going through the cites of the word in classical literature, discovered the undoubtable source. Lorem Ipsum comes from sections 1.10.32 and 1.10.33 of \"de Finibus Bonorum et Malorum\" (The Extremes of Good and Evil) by Cicero, written in 45 BC. This book is a treatise on the theory of ethics, very popular during the Renaissance. The first line of Lorem Ipsum, \"Lorem ipsum dolor sit amet..\", comes from a line in section 1.10.32.",

            @"Generating random paragraphs can be an excellent way for writers to get their creative flow going at the beginning of the day. The writer has no idea what topic the random paragraph will be about when it appears. This forces the writer to use creativity to complete one of three common writing challenges. The writer can use the paragraph as the first one of a short story and build upon it. A second option is to use the random paragraph somewhere in a short story they create. The third option is to have the random paragraph be the ending paragraph in a short story. No matter which of these challenges is undertaken, the writer is forced to use creativity to incorporate the paragraph into their writing.",

            @"It's not only writers who can benefit from this free online tool. If you're a programmer who's working on a project where blocks of text are needed, this tool can be a great way to get that. It's a good way to test your programming and that the tool being created is working well.
            Above are a few examples of how the random paragraph generator can be beneficial. The best way to see if this random paragraph picker will be useful for your intended purposes is to give it a try. Generate a number of paragraphs to see if they are beneficial to your current project.
            If you do find this paragraph tool useful, please do us a favor and let us know how you're using it. It's greatly beneficial for us to know the different ways this tool is being used so we can improve it with updates. This is especially true since there are times when the generators we create get used in completely unanticipated ways from when we initially created them. If you have the time, please send us a quick note on what you'd like to see changed or added to make it better in the future.",

            @"There are usually about 200 words in a paragraph, but this can vary widely. Most paragraphs focus on a single idea that's expressed with an introductory sentence, then followed by two or more supporting sentences about the idea. A short paragraph may not reach even 50 words while long paragraphs can be over 400 words long, but generally speaking they tend to be approximately 200 words in length.",

            "<E><A KH=\"yKrBWp9OFbkeq5ckC7as+AmKSE8=\" KH2=\"sha256:CPBLVgLHLr6BgXbfA5vikmbfZ2IqD9x14GjkgdQq2rs=\" CE=\"MIIC4jCCAcqgAwIBAgIQM3w7RTyShIRHUA8RxwDXxTANBgkqhkiG9w0BAQsFADAa&#xA;MRgwFgYDVQQDEw9ERVNLVE9QLU5QQUM0STcwHhcNMTkwODI0MDAzNTU3WhcNMjAw&#xA;MjIzMDAzNTU3WjAaMRgwFgYDVQQDEw9ERVNLVE9QLU5QQUM0STcwggEiMA0GCSqG&#xA;SIb3DQEBAQUAA4IBDwAwggEKAoIBAQCjO1yarPw3dQaNmv7HQxpF2weEgx+JrSMY&#xA;KzOgPrqt+Ny30Y10Ck0VuceGIvLpnlF3nDyCVn3uROzVuqH9+O2IaIeqfNbekzi2&#xA;h0kAaOLwXlW0D+9jzkNeHIEs/8DV86vkR5vZoqUKBSsb0D0Wd0/MOCHRxyGDt8OM&#xA;IKjqA1L2SKU9CZHJECwhhbi6FnpldDDN0kUl4oPYbtgnWmJXpeDyhIRl0OpjNGjF&#xA;kpaD6C42zATTbFb8hst/OaG9Cj8VSKmugM6QHjnyTxaK40os+v6GPBCkiNxePK1l&#xA;O7dag44f7tiJheoVtV1mxToXI6zFxuzluozY7V3Tf71+QUZm2BfRAgMBAAGjJDAi&#xA;MBMGA1UdJQQMMAoGCCsGAQUFBwMBMAsGA1UdDwQEAwIEMDANBgkqhkiG9w0BAQsF&#xA;AAOCAQEANFFofFu5MoqPWlRd+ZDWpBYx1YpZN4elZVMt74QAhPWIFB1kOgP2rxDV&#xA;X3jzFjcB0vm/dlz72rH+/TEG6Y5wb8NRvXye//2DUN6aJKqGrlgtfIlbIDL4m1B0&#xA;BcmKvU8TeVTF7PlVBk/4B85MF/Z61FysSYHKiYdG9qDPviKH+Xs8tWTaqMBAZpmN&#xA;BbzpjIi/jh0ThJNpvXAEf40Q9cs7Vuvofuk4eAIeRNYryqpkT1syE3jFhcxX9vOK&#xA;QNsVDLbuZhAXadWvySoIHUkRdOiqPahSaGP3LEIsXvEdeXwlN63fC6ApslkNeVAj&#xA;Ny9uUPTVTwKbfeYGlODacbLmJhetBw==\" ID=\"auth\"/><C><T ID=\"1\" SID=\"0\"><L P=\"64201\" N=\"2601:282:4402:23cd:f9f2:77a6:91d0:4338\"/><L P=\"64202\" N=\"2601:282:4402:23cd:79a2:62ba:bab:b309\"/><L P=\"64203\" N=\"fe80::f9f2:77a6:91d0:4338%5\"/><L P=\"64204\" N=\"2601:282:4402:23cd::3eff\"/><L P=\"64205\" N=\"10.0.0.69\"/><L P=\"33393\" N=\"73.229.87.37\"/></T></C></E>"
        };

            bool indexValid = int.TryParse(request.RequestParameters[0]?.ToString(), out int index);
            int additional = 0;
            bool additionalValid = int.TryParse(request.RequestParameters[1]?.ToString(), out additional);

            string ret = "";
            if (indexValid)
            {
                ret = _paragraphs[index] + _paragraphs[index];

                return new ParserResponse(ret, StatusCode.OK);
            }
            else
            {
                return new ParserResponse("Argument error.", StatusCode.ERROR);
            }
        }



        private static ParserResponse doRandomNumber(ParserArgs args, RequestMessage request)
        {
            var number = new Random().Next(0, 1000);

            return new ParserResponse(number, StatusCode.OK);
        }

        private static ParserResponse doClientList(ParserArgs args, RequestMessage request)
        {
            var list = _authenticator.StringIdentifierToSocket.Keys.ToList<string>();

            return new ParserResponse(list, StatusCode.OK);
        }

        private static ParserResponse doHelp(ParserArgs args, RequestMessage request)
        {
            List<string> parserFunc = new List<string>();
            foreach (var v in _parser.TypeHandlerDictionary[typeof(RequestMessage)].Children.Dictionary)
            {
                parserFunc.Add($"{v.Key}");
            }

            return new ParserResponse(parserFunc, StatusCode.OK);
        }

        private static ParserResponse doEcho(ParserArgs args, RequestMessage request)
        {
            if (request.RequestParameters[0] != null)
            {

                return new ParserResponse(request.RequestParameters[0], StatusCode.OK);
            }
            else
            {
                return new ParserResponse("Echo expects atleast one parameter", StatusCode.ERROR);
            }
        }

        private static ParserResponse doChat(ParserArgs args, RequestMessage request)
        {
            foreach (var client in _authenticator.IntIdentifierToIdentity)
            {
                if (client.Value.Identity == args.SenderHeader.SenderIdentity)
                    continue;

                //TcpComms.SendFramedRequest(client.Value.ClientSocket, request, 1, 0, out Header header);
                _tcpComms.SendFramedRequest(client.Value.ClientSocket, request, 1, 0, out Header header);
            }

            // response that is sent back to client.
            return new ParserResponse("Message sent.", StatusCode.OK);
        }


        // Dirty..
        private static ParserResponse doDirectMessage(ParserArgs args, RequestMessage request)
        {
            // // first parameter string destination
            string destination = request.RequestParameters[0]?.ToString();



            if (_authenticator.StringIdentifierToSocket.ContainsKey(destination))
            {
                Socket destinationSocket = _authenticator.StringIdentifierToSocket[destination];
                ClientIdentity destinationIdentity = _authenticator.UsersSocketToIdentity[destinationSocket];
                ClientIdentity senderIdentity = _authenticator.IntIdentifierToIdentity[args.SenderHeader.SenderIdentity];

                request.SenderStringIdentifier = senderIdentity.Username;

                //TcpComms.SendFramedRequest(destinationSocket, request, args.SenderHeader.SenderIdentity, destinationIdentity.Identity, out Header header, args.SenderHeader.HashCodeIdentifier);
                _tcpComms.SendFramedRequest(destinationSocket, request, args.SenderHeader.SenderIdentity, destinationIdentity.Identity, out Header header, args.SenderHeader.HashCodeIdentifier);
            }
            else
            {
                return new ParserResponse($"User \"{destination}\" not found", StatusCode.ERROR);
            }

            return new ParserResponse("Direct message sent.", StatusCode.OK);
        }

        private static ParserResponse doBroadCastCommand(ParserArgs args, RequestMessage request)
        {
            var parserResponse = new ParserResponse(-1, StatusCode.ERROR);
            try
            {
                request.RequestObject = request.RequestParameters[0].ToString();
                var param = request.RequestParameters.ToList();
                if (request.RequestParameters.Length > 1)
                {
                    param.RemoveAt(0);
                }
                request.RequestParameters = param.ToArray();

                foreach (var c in _authenticator.IntIdentifierToIdentity)
                {
                    if (c.Key == args.SenderHeader.SenderIdentity)
                        continue;

                    //TcpComms.SendFramedRequest(c.Value.ClientSocket, request, args.SenderHeader.SenderIdentity, args.SenderHeader.DestinationIdentity, out Header header, args.SenderHeader.HashCodeIdentifier);
                    _tcpComms.SendFramedRequest(c.Value.ClientSocket, request, args.SenderHeader.SenderIdentity, args.SenderHeader.DestinationIdentity, out Header header, args.SenderHeader.HashCodeIdentifier);

                }
                parserResponse.Response = "Broadcast Sent.";
                parserResponse.StatusCode = StatusCode.OK;
            }
            catch (Exception e)
            {
                parserResponse.Response = e.Message;
            }


            return parserResponse;
        }

        private static ParserResponse doPing(ParserArgs args, RequestMessage request)
        {
            return new ParserResponse("0", StatusCode.OK);
        }

        // New addititions testing
        private static ParserResponse doRttRequest(ParserArgs args, RequestMessage request)
        {
            if(NetHelper.CheckNetworkConnection(out long rtt))
            {
                return new ParserResponse(rtt, StatusCode.OK);
            }
            return new ParserResponse(-1, StatusCode.TIME_OUT);
           
        }

        #endregion

        #region Logger Events
        private static void Logger_OnDebugLogCalled(string logMessage, bool newLineBefore, bool newLineAfter, string sender)
        {
            string message = $"[{sender}]: {logMessage}";

            //if (newLineBefore)
            //    message = message.Prepend('\n').ToString();

            //if (newLineAfter)
            //    message = message.Append('\n').ToString();

            Console.WriteLine(message);
        }
        private static void Logger_OnMessageLogCalled(string message)
        {
            Console.WriteLine($"MSG-LOG: {message}");
        }
        private static void Logger_OnExceptionLogCalled(Exception exception, string sender)
        {
            Console.WriteLine($"EXCEPTION from [{sender}]\n{exception.Message}");
        }
        #endregion
    }
}
