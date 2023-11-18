using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using NetShared.Net;
using static NetShared.Net.TcpFramer;
using NetShared.Logging;
using NetClient.Events;
using NetShared.NetParser;
using NetClient.Authentication;
using NetShared.NetObject;

using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NetClient.Exceptions;
using NetShared.Extensions;
using NetShared.Events;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Cache;

namespace NetClient
{
  

    public delegate void OnConnecting();
    public delegate void OnConnectionAttempt(ClientConnectionEventArgs args);
    public delegate void OnConnected(ClientConnectionEventArgs args);
    public delegate void OnRequestTimeout(RequestTimeoutEventArgs args);

    /// <summary>
    /// Serves as a TcpClient wrapper that handles asynchronous communications.
    /// 
    /// Handles unplanned requests from server
    /// Handles planned requests to server
    /// Allows for communications between peers connected to the server
    /// </summary> 
    public class Client : System.ComponentModel.INotifyPropertyChanged
    {

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        public void callInpc(object sender, [CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public event OnConnecting Connecting;
        public event OnConnectionAttempt ConnectionAttempt;
        public event OnConnected Connected;
        public event OnRequestTimeout RequestTimeout;

        public Dictionary<int, byte[]> RequestDictionary = new Dictionary<int, byte[]>();
        public Dictionary<int, Response<object>> ResponseDictionary = new Dictionary<int, Response<object>>();
        public Dictionary<int, AutoResetEvent> AutoResetEvents = new Dictionary<int, AutoResetEvent>();

        public TcpComms ClientComms { get; set; }

        public IParser Parser { get; set; }

        public Client()
        {

         
        }

        public Client(TcpComms tcpComms, IParser parser)
        {
            ClientComms = tcpComms;
            
            ClientComms.TcpClient = new TcpClient();
           

            Parser = parser;

            AttachEvents();
        }

        ~Client()
        {
            DetachEvents();
        }

        public void AttachEvents()
        {
            ClientComms.HeaderReceived += ClientComms_HeaderReceived;
            ClientComms.ObjectReceived += ClientComms_ObjectReceived;
            ClientComms.RequestSend += ClientComms_RequestSend;
        }

     

        public void DetachEvents()
        {
            ClientComms.HeaderReceived -= ClientComms_HeaderReceived;
            ClientComms.ObjectReceived -= ClientComms_ObjectReceived;
            ClientComms.RequestSend -= ClientComms_RequestSend;
        }

        private void ClientComms_HeaderReceived(Header header)
        {
            Console.WriteLine($"{header.Flag}, {header.HashCodeIdentifier}");
        }

   
        private async void ClientComms_ObjectReceived(ObjectReceivedEventArgs eventArgs)
        {
            var receivedObject = ClientComms.Serializer.Deserialize<object>(eventArgs.ReceivedBytes, 0, eventArgs.ReceivedBytes.Length);

            // Could do check here with AutoResetEvents.TryGetValue
            if (RequestDictionary.ContainsKey(eventArgs.SenderHeader.HashCodeIdentifier))
            {
                var response = new Response<object>(receivedObject, eventArgs.SenderHeader.SenderIdentity, (StatusCode)eventArgs.SenderHeader.StatusCode);
                ResponseDictionary.Add(eventArgs.SenderHeader.HashCodeIdentifier, response);
                
                // Duplicate hashcodes?? Or threading issue: Response not being recogonized
                bool containsKey = AutoResetEvents.TryGetValue(eventArgs.SenderHeader.HashCodeIdentifier, out AutoResetEvent are);
                if (containsKey)
                {
                    are.Set();
                    AutoResetEvents.Remove(eventArgs.SenderHeader.HashCodeIdentifier);
                }

            }
            else
            {
                
                var response = await Parser.ParseType<ParserResponse>(new ParserArgs(eventArgs.SenderSocket, eventArgs.SenderHeader), receivedObject);
                if (response == default)
                    return;

               
                ClientComms.SendFramedResponse(ClientComms.TcpClient.Client, eventArgs.SenderHeader, response, ClientIdentity.CurrentIdentity);

            }
        }

        private void ClientComms_RequestSend(RequestSendEventArgs args)
        {
            RequestDictionary.Add(args.RequestHeader.HashCodeIdentifier, args.FramedBytes);
        }

        /// <summary>
        /// Connects client to endpoint
        /// Sends authentication information
        /// Initiates asynchronous receive of data
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public async Task<Response<int>> Connect(string hostname, int port, RequestBase authRequest, int connectionAttempts = 0)
        {
            var resp = new Response<int>(default, -1, StatusCode.ERROR);
            try
            {
                Connecting?.Invoke();

                ClientComms.TcpClient.Connect(hostname, port);

                var stateObject = new StateObject()
                {
                    WorkSocket = ClientComms.TcpClient.Client
                };

               
                //stateObject.WorkSocket.BeginReceive(stateObject.Buffers.First(), 0, stateObject.Buffers.First().Length, SocketFlags.None, ClientComms.BeginReceiveCallback, stateObject);
                stateObject.WorkSocket.BeginReceive(stateObject.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ClientComms.BeginReceiveCallback, stateObject);

                resp = await ObjectRequest<int, RequestBase>(authRequest, 1);
                ClientIdentity.ServerIntIdentity = resp.ReceivedObject;
                Connected?.Invoke(new ClientConnectionEventArgs(connectionAttempts, hostname, port) { ServerIdentityReturn = resp.ReceivedObject });
                return resp;


            }
            catch (Exception)
            {
               
                ConnectionAttempt?.Invoke(new ClientConnectionEventArgs(connectionAttempts, hostname, port) { ConnectionSuccess = false }); ;
            }

            return resp;
        }

        public void Disconnect()
        {
            try
            {
                ClientComms.TcpClient.Client.Disconnect(true);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }
        /// <summary>
        /// Sends data to server in TCPComms format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">string identifier of destination</param>
        /// <param name="request">string request, single string with no spaces.</param>
        /// <param name="destinationIntIdentifier">used for server, use parameter when server Int identity has been received.</param>
        /// <param name="parameters">parameters for the function that is called on the destination machine.</param>
        /// <returns></returns>
        public async Task<Response<T>> StringRequest<T>(string destination, string request, int destinationIntIdentifier = 1, params object[] parameters)
        {

            var reqObject = new RequestMessage()
            {
                SenderStringIdentifier = ClientIdentity.CurrentUserStringIdentity,
                DestinationStringIdentifier = destination,
                RequestObject = request
            };

            

            if (parameters != null && parameters.Length > 0)
            {
                reqObject.RequestParameters = parameters;
            }



            var response = await ObjectRequest<T, RequestMessage>(reqObject, destinationIntIdentifier);

            return response;


        }


        public async Task<Response<TReturn>> ObjectRequest<TReturn, UType>(UType uType, int destinationIntId) where UType : class
        {
            var stopWatch = new Stopwatch();
            var resetEvent = new AutoResetEvent(false);

            stopWatch.Start();


            ClientComms.SendFramedRequest(ClientComms.TcpClient.Client, uType, ClientIdentity.CurrentIdentity, destinationIntId, out Header requestHeader);

            AutoResetEvents.Add(requestHeader.HashCodeIdentifier, resetEvent);

            try
            {

                await resetEvent.AsTask(TimeSpan.FromMilliseconds(ClientComms.TcpClient.ReceiveTimeout));

                var responseHasValue = ResponseDictionary.TryGetValue(requestHeader.HashCodeIdentifier, out Response<object> response);
                if (responseHasValue)
                {
                    var responseObject = new Response<TReturn>((TReturn)response.ReceivedObject, response.Sender, response.Status, stopWatch.Elapsed.TotalMilliseconds);
                    stopWatch.Reset();

                    ResponseDictionary.Remove(requestHeader.HashCodeIdentifier);

                    RequestDictionary.Remove(requestHeader.HashCodeIdentifier);

                    return responseObject;
                }
                else
                {
                    throw new TaskCanceledException($"Wait task cancelled because response wasn't present. ID:{requestHeader.HashCodeIdentifier}");
                }



            }
            catch (TaskCanceledException)
            {
                // Possibly throw RequestTimeoutException
                // Need info to resend request (Without repeating the serialization etc..)
                RequestTimeout?.Invoke(new RequestTimeoutEventArgs(requestHeader.HashCodeIdentifier));
                return new Response<TReturn>(default, 0, StatusCode.TIME_OUT);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return new Response<TReturn>(default, 0, StatusCode.ERROR);
            }
        }
    }

}
