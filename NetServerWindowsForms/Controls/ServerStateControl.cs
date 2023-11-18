using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetServerWindowsForms.Models;
using WindowsFormsShared.EventAggregator;
using NetShared.Logging;
using NetShared.Net;
using static NetShared.Net.TcpFramer;
using NetShared.NetParser;
using NetShared.NetObject;
using NetServer.Authentication;
using System.Net;
using System.Net.Sockets;

namespace NetServerWindowsForms.Controls
{
    public partial class ServerStateControl : UserControl
    {
        private IEventAggregator _events;
        private NetServer.Server _server;
        private Parser _parser = new Parser();
        private Authenticator _authenticator = new Authenticator();
        private TcpComms _serverComms;

        public ServerStateControl(IEventAggregator eventAggregator)
        {
            _serverComms = new TcpComms(new Serializer(), new TcpFramer());
       
           
            _server = new NetServer.Server(IPAddress.Any, 1337, _serverComms, _parser, _authenticator);
            _events = eventAggregator;
           

            InitializeComponent();
        }

        private void ServerStateControl_Load(object sender, EventArgs e)
        {
            Logger.OnDebugLogCalled += Logger_OnDebugLogCalled;
            Logger.OnExceptionLogCalled += Logger_OnExceptionLogCalled;
            Logger.OnMessageLogCalled += Logger_OnMessageLogCalled;

            _parser.AddHandler<RequestMessage, Task<ParserResponse>>(RequestMessageHandler)
                .AddChild("chat", doChat)
                .AddChild("list", getClientList);

            _parser.AddHandler<AuthenticationRequest, ParserResponse>(AuthenticationHandler);

            propertyGridServerConfig.SelectedObject = new ServerConfigurationModel();

        }

      

        private void Logger_OnMessageLogCalled(string message)
        {
            crossThreadInvoke(richTextBoxServerLog, () => richTextBoxServerLog.Text += $"\n{message}");
        }

        private void Logger_OnExceptionLogCalled(Exception exception, string sender)
        {
            crossThreadInvoke(richTextBoxServerLog, () => richTextBoxServerLog.Text += $"\n[{sender}]:{exception.Message}");
        }

        private void Logger_OnDebugLogCalled(string logMessage, bool newLineBefore, bool newLineAfter, string sender)
        {
            crossThreadInvoke(richTextBoxServerLog, () => richTextBoxServerLog.Text += $"\n[Debug({sender})]: {logMessage}");
        }

        private void toolStripButtonServerStart_Click(object sender, EventArgs e)
        {
            var configModel = propertyGridServerConfig.SelectedObject as ServerConfigurationModel;
            _authenticator.ServerPassword = configModel.Password;
            _server.ListeningPort = configModel.ListeningPort;
            crossThreadInvoke(richTextBoxServerLog, () => richTextBoxServerLog.Text += $"\n{_server.ListeningAddress}:{_server.ListeningPort}\nPass:{_authenticator.ServerPassword}");
            _server.Start();
        }

        private ParserResponse AuthenticationHandler(ParserArgs args, AuthenticationRequest authentication)
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
        private async Task<ParserResponse> RequestMessageHandler(ParserArgs args, RequestMessage requestMessage)
        {
            if (requestMessage.DestinationStringIdentifier.Equals(_authenticator.ServerStringIdentity))
            {
                var ret = await _parser.CallHandlerChild<RequestMessage, ParserResponse, string>(requestMessage.RequestObject.ToString(), args, requestMessage);
                return (ParserResponse)ret;
            }
            // Could lookup by int identifier..
            bool identifierPresent = _authenticator.StringIdentifierToSocket.TryGetValue(requestMessage.DestinationStringIdentifier, out Socket destinationSocket);
            if (identifierPresent)
            {
                _serverComms.SendFramedRequest(destinationSocket, requestMessage, args.SenderHeader.SenderIdentity, args.SenderHeader.DestinationIdentity, out Header header, args.SenderHeader.HashCodeIdentifier);
                return default;
            }
            return new ParserResponse("Handler couldn't find destination..", StatusCode.ERROR) { HasError = true };
        }

        private ParserResponse doChat(ParserArgs args, RequestMessage request)
        {
           

            foreach(var c in _authenticator.StringIdentifierToSocket)
            {
                if (c.Key == request.SenderStringIdentifier)
                    continue;

                _serverComms.SendFramedRequest(c.Value, "chat", args.SenderHeader.SenderIdentity, 0, out Header header);
            }

            return new ParserResponse("Message sent", StatusCode.OK);
        }

        private ParserResponse getClientList(ParserArgs args, RequestMessage request)
        {
            var clientList = new List<string>();
            foreach (var c in _authenticator.StringIdentifierToSocket)
            {
                clientList.Add(c.Key);
            }

            return new ParserResponse(clientList, StatusCode.OK);
        }

        private void crossThreadInvoke(Control c, Action a)
        {
            c.Invoke(a);
        }
    }
}
