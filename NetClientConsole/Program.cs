using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetClient;
using NetClient.Authentication;
using NetClient.Events;
using NetShared.Events;
using NetShared.Logging;
using NetShared.Net;
using NetShared.NetObject;
using NetShared.NetParser;
using static NetShared.Net.TcpFramer;

namespace NetClientConsole
{

    /// <summary>
    /// TODO: Make parsing commands more streamlined for implementation of delegate command
    /// reference IE "doDisconnect(
    /// </summary>

    class Program
    {

        private static TcpComms _tcpComms = default;
        private static Parser _parser = new Parser();
        public static Client NetClient = default;

        private static ConsoleTitleHelper _titleHelper = new ConsoleTitleHelper();

        public static async Task Main(string[] args)
        {
            
            _tcpComms = new TcpComms(new Serializer(), new TcpFramer());
            NetClient = new Client(_tcpComms, _parser);


            //Logger.OnMessageLogCalled += Logger_OnMessageLogCalled;
            //Logger.OnDebugLogCalled += Logger_OnDebugLogCalled;
            //Logger.OnExceptionLogCalled += Logger_OnExceptionLogCalled;



            // Clean configuration up..
            _tcpComms.TcpClient.SendBufferSize = 50 * 1024;
            _tcpComms.TcpClient.ReceiveBufferSize = 50 * 1024;
            _tcpComms.TcpClient.SendTimeout = 10000;
            _tcpComms.TcpClient.ReceiveTimeout = 10000;

            /* On client launch 
             *  Prompt to server login credentials
             *  Should be able to do offline mode (no server side methods avail) 
             *   Offline mode has local methods available like
             *     help, connect, reconnect
             * 
             */

            _parser.AddHandler<RequestMessage, Task<ParserResponse>>(HandleRequestMessage)
                .AddChild("getMachineName", doGetMachineName)
                .AddChild("getMath", doGetMath)
                .AddChild("help", doGetHelp)
                .AddChild("shell", doRemoteShell)
                .AddChild("ping", doPing)
                .AddChild("client_add", doAddClient)
                .AddChild("client_remove", doRemoveClient)
                .AddChild("para", doPara)
                .AddChild("chat", doChat)
                .AddChild("dm", doDirectMessage)
                .AddChild("run_tests", doRunTests)
                .AddChild("cmd_output", doCmdOutputStream);



            NetClient.ConnectionAttempt += NetClient_ConnectionAttempt;
            NetClient.Connected += NetClient_Connected;


           
            _tcpComms.ConnectionLost += _tcpComms_ConnectionLost;

            _titleHelper.PropertyChanged += _titleHelper_PropertyChanged;


            await Connect();
            await StartStandardSend();



            Console.ReadLine();
        }

     

        private static void _titleHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.Title = _titleHelper.ToString();
        }

        private static async void _tcpComms_ConnectionLost()
        {
            Console.WriteLine("Connection lost from remote host.\n Retry connection? [Y/N]");
            var key = Console.ReadKey();
            retry:
            if (key.Key == ConsoleKey.Y)
            {
                await Connect();
                await StartStandardSend();
            }else if(key.Key == ConsoleKey.N) 
            {
                await authenticationPrompt();
            }else
            {
                goto retry;
            }
        }

      

        private static async void RunPoller(int interval)
        {
            _titleHelper.DownloadBytes = _tcpComms.ReceivedBytes;
            _tcpComms.ReceivedBytes = 0;
            _titleHelper.UploadBytes = _tcpComms.SentBytes;
            _tcpComms.SentBytes = 0;

            await Task.Delay(interval);
            RunPoller(interval);
        }

       

        // Running ping concurrently with tests causes dropped requests...
        private static async void StartPing(int interval)
        {
            Response<string> resp;
            do
            {
                resp = await NetClient.StringRequest<string>(ClientIdentity.ServerStringIdentity, "ping", ClientIdentity.ServerIntIdentity);
                _titleHelper.Ping = resp.ResponseTime;
                await Task.Delay(interval);
            } while (resp.Status != StatusCode.ERROR);
            await Console.Out.WriteLineAsync("Stopping Pinger");

        }

        #region Client Events
        private static void NetClient_Connected(ClientConnectionEventArgs args)
        {
           

            RunPoller(1000);
            _titleHelper.SessionInformation = $"Client:{ClientIdentity.CurrentIdentity} ({ClientIdentity.CurrentUserStringIdentity}), Server:{ClientIdentity.ServerIntIdentity} ({ClientIdentity.ServerStringIdentity})";

            StartPing(1000);
        }
        private static void NetClient_RequestTimeout(RequestTimeoutEventArgs args)
        {

        }

        private async static void NetClient_ConnectionAttempt(ClientConnectionEventArgs args)
        {
            if (!args.ConnectionSuccess)
            {
                Console.WriteLine($"Connection failed. Retrying  #{args.ConnectionAttempts}");
                var response = await NetClient.Connect(args.HostName, args.HostPort, new AuthenticationRequest() { Username = ClientIdentity.CurrentUserStringIdentity, Password = "helloworld" }, ++args.ConnectionAttempts);
                await Console.Out.WriteLineAsync($"Response status: {response.Status}");
                await Task.Delay(1000);
            }

        }

   



        #endregion

        private static async Task<ParserResponse> HandleRequestMessage(ParserArgs args, RequestMessage req)
        {
            Logger.LogDebug($"Sender: {req.SenderStringIdentifier}, Destination: {req.DestinationStringIdentifier}");
            try
            {
                var ret = await _parser.CallHandlerChild<RequestMessage, ParserResponse, string>(req.RequestObject.ToString(), args, req);
                return ret;
            }
            catch (Exception e)
            {
                return new ParserResponse($"{e.Message}", StatusCode.ERROR);
            }

        }

        #region Parser Methods
        private static ParserResponse doCmdOutputStream(ParserArgs args, RequestMessage request)
        {
            Console.WriteLine(request.RequestParameters[0]?.ToString());

            return default;
        }

        private static ParserResponse doPara(ParserArgs args, RequestMessage request)
        {
            return new ParserResponse("Contrary to popular belief, Lorem Ipsum is not simply random text. It has roots in a piece of classical Latin literature from 45 BC, making it over 2000 years old. Richard McClintock, a Latin professor at Hampden-Sydney College in Virginia", StatusCode.OK);
        }
        private static ParserResponse doPing(ParserArgs args, RequestMessage request)
        {
            return new ParserResponse("pong", StatusCode.OK);
        }
        private static ParserResponse doRemoteShell(ParserArgs args, RequestMessage request)
        {
            var resetEvent = new ManualResetEvent(false);
            StringBuilder consoleOutput = new StringBuilder();
            string paramString = string.Join(" ", request.RequestParameters);
            Logger.LogDebug($"Running command: \"{paramString}\"");

            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo("cmd", $"/c {paramString}")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            // send Beginning of Stream message
            int streamHash = new Random().Next(1, int.MaxValue);

            //var beginningOfStreamHeader = _tcpComms.TcpFramer.CreateHeader(null, StatusCode.NONE, Flag.BOS, streamHash, ClientIdentity.CurrentIdentity, args.SenderHeader.SenderIdentity);

            //_tcpComms.SendHeader(beginningOfStreamHeader, NetClient.ClientComms.TcpClient.Client);

            _tcpComms.SendFlag(Flag.BOS, streamHash, ClientIdentity.CurrentIdentity, args.SenderHeader.SenderIdentity);

            var sb = new StringBuilder();
            proc.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                {

                    //var endOfStreamHeader = _tcpComms.TcpFramer.CreateHeader(null, StatusCode.NONE, Flag.EOS, streamHash, ClientIdentity.CurrentIdentity, args.SenderHeader.SenderIdentity);

                    //_tcpComms.SendHeader(endOfStreamHeader, NetClient.ClientComms.TcpClient.Client);
                    _tcpComms.SendFlag(Flag.EOS, streamHash, ClientIdentity.CurrentIdentity, args.SenderHeader.SenderIdentity);
                    
                    
                }
                else
                {
                    sb.Append($"\n{e.Data}");
                    
                    //if (sb.Length > 4000)
                    //{
                    //    await NetClient.StringRequest<object>(request.SenderStringIdentifier, "cmd_output", args.SenderHeader.SenderIdentity, sb.ToString());
                    //    sb.Clear();
                    //}

               
                }

            };


            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();




            return new ParserResponse("Command Executed.", StatusCode.OK);
        }

        private static ParserResponse doGetHelp(ParserArgs args, RequestMessage request)
        {
            List<string> parserFunc = new List<string>();
            foreach (var v in _parser.TypeHandlerDictionary[typeof(RequestMessage)].Children.Dictionary)
            {

                parserFunc.Add($"{v.Key}:{v.Value.GetType()}");
            }

            return new ParserResponse(parserFunc, StatusCode.OK);
        }

        private static ParserResponse doGetMath(ParserArgs args, RequestMessage request)
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

        private static ParserResponse doGetMachineName(ParserArgs args, RequestMessage request)
        {
            return new ParserResponse(Environment.MachineName, StatusCode.OK);
        }

        private static ParserResponse doDirectMessage(ParserArgs args, RequestMessage request)
        {
            Console.WriteLine($"\nProc_Dm:");
            for (int i = 1; i < request.RequestParameters.Length; i++)
            {
                Console.Write($"{request.RequestParameters[i]} ");
            }
            Console.WriteLine();

            return new ParserResponse("Dm received.", StatusCode.OK);
        }

        private static ParserResponse doChat(ParserArgs args, RequestMessage request)
        {
            Console.WriteLine($"Proc_Chat: {request.RequestParameters[0]?.ToString()}");

            return new ParserResponse("Chat received.", StatusCode.OK);
        }

        private static ParserResponse doRunTests(ParserArgs args, RequestMessage request)
        {
            bool validCast = int.TryParse(request.RequestParameters[0]?.ToString(), out int validCount);

            if (validCast)
            {
                Task.Run(async () => await RunTests(validCount));
            }
            else
            {
                return new ParserResponse("Test count argument was invalid.", StatusCode.ERROR);
            }

            return default;
        }

        private static ParserResponse doAddClient(ParserArgs args, RequestMessage request)
        {
            Console.WriteLine($"Client {request.SenderStringIdentifier} connected!");
            return default;
        }
        private static ParserResponse doRemoveClient(ParserArgs args, RequestMessage request)
        {
            Console.WriteLine($"Client {request.SenderStringIdentifier} disconnected!");
            return default;
        }



        private static ParserResponse doTextStream(ParserArgs args, RequestMessage request)
        {
            var requestResource = request.RequestParameters[0];
             
            // Create bos header


            return default;
        }

        #endregion

        #region Console Methods
        private static async Task Connect()
        {

            string username = $"{Environment.UserName}_{new Random().Next(0, 1000)}";

            ClientIdentity.CurrentUserStringIdentity = username;

            Console.Write("Address: ");
            string address = Console.ReadLine();

            var response = await NetClient.Connect(address, 1337, new AuthenticationRequest() { Username = username, Password = "helloworld" });
            //Console.WriteLine($"STAT: {response.Status}, RTT: {response.ResponseTime}");

            if (response.Status != StatusCode.ERROR)
            {
                _titleHelper.SessionInformation = $"Client:{ClientIdentity.CurrentIdentity}:{ClientIdentity.CurrentUserStringIdentity}, Server:{ClientIdentity.ServerIntIdentity}:{ClientIdentity.ServerStringIdentity}";
            }
        }

        private static async Task StartStandardSend()
        {

            string input = "";
            string destination = "server";
            while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write($"{destination}>");
                input = Console.ReadLine();

                string command = "";
                var reqArgs = input.Split(' ').ToList();
                if (reqArgs.Count > 1)
                {
                    command = reqArgs[0];
                    reqArgs.RemoveAt(0);
                }
                else
                {
                    command = input;
                }

                switch (command)
                {
                    case "setDestination":
                        destination = reqArgs[0];
                        continue;
                    case "clear":
                        Console.Clear();
                        continue;
                    case "header":
                        //var header = TcpFramer.CreateHeader(null, StatusCode.NONE, Flag.NONE, new Random().Next(0, 10000), ClientIdentity.CurrentIdentity, ClientIdentity.ServerIntIdentity);
                        var header = _tcpComms.TcpFramer.CreateHeader(null, StatusCode.NONE, Flag.NONE, new Random().Next(0, 10000), ClientIdentity.CurrentIdentity, ClientIdentity.ServerIntIdentity);

                        //TcpComms.SendHeader(header, NetClient.TcpClientClass.Client);
                        _tcpComms.SendHeader(header, NetClient.ClientComms.TcpClient.Client);
                        continue;
                    case "tests":
                        bool validParse = int.TryParse(reqArgs[0], out int validArg);
                        if (validParse)
                        {
                            await RunTests(validArg);
                        }
                        break;
                }

                var resp = await NetClient.StringRequest<object>(destination, command, ClientIdentity.ServerIntIdentity, reqArgs.ToArray());

                if (resp == null)
                    continue;

                Console.WriteLine($"STAT:{resp.Status}, RTT:{resp.ResponseTime}");

                if (resp.Status == StatusCode.TIME_OUT)
                    continue;

                if (resp.ReceivedObject is List<string> stringList)
                {
                    foreach (var s in stringList)
                    {
                        Console.WriteLine(s);
                    }
                }
                else if (resp.ReceivedObject is string str)
                {
                    Console.WriteLine(str);
                }
                else if (resp.ReceivedObject is int num)
                {
                    Console.WriteLine(num);
                }

            }

        }
        private static async Task RunTests(int count, bool runDelayTests = false)
        {
            List<string> failedTestInfo = new List<string>();
            int failed = 0;
            Random ran = new Random();
            string[] commands =
            {
                "help", "list", "rand"
            };

            Console.WriteLine("Random command tests..\n");

            int totalTestCount = 0;
            double respTime = 0;

            for (int i = 0; i < count; i++)
            {
                totalTestCount++;
                string cmd = commands[ran.Next(0, commands.Length)];
                var requestResponse = await NetClient.StringRequest<object>("server", cmd, ClientIdentity.ServerIntIdentity);
                respTime += requestResponse.ResponseTime;
                if (requestResponse.Status == StatusCode.OK)
                {
                    Console.WriteLine($"#{i} [{cmd}] => ({requestResponse.ReceivedObject ?? "<<NULL>>"}, {requestResponse.Status}, ResponseTime: {requestResponse.ResponseTime})");
                }
                else
                {
                    failedTestInfo.Add($"Rand. Pos:{i} Cmd:{cmd}");

                    failed++;
                }
            }

            if (runDelayTests)
            {
                Console.WriteLine("Delayed Response tests..\n");
                for (int j = 0; j < 5; j++)
                {
                    var resp = await NetClient.StringRequest<object>("server", "delay", ClientIdentity.ServerIntIdentity, ran.Next(0, NetClient.ClientComms.TcpClient.ReceiveTimeout));
                    if (resp.Status == StatusCode.OK)
                    {
                        Console.WriteLine($"#{j} [delay] => ({resp.ReceivedObject}, Response Time: {resp.ResponseTime})");
                    }
                    else
                    {

                        failed++;
                    }
                }
            }


            Console.WriteLine("Math tests..\n");
            for (int k = 0; k < count; k++)
            {
                totalTestCount++;
                var num1 = ran.Next(1, 21);
                var num2 = ran.Next(1, 21);

                var resp = await NetClient.StringRequest<object>("server", "math", ClientIdentity.ServerIntIdentity, num1, num2);
                respTime += resp.ResponseTime;
                if (resp.Status == StatusCode.OK)
                {
                    Console.WriteLine($"#{k} [math] => ({num1}*{num2}) = ({resp.ReceivedObject ?? "<<NULL>>"}) Response Time: {resp.ResponseTime}");
                }
                else
                {
                    failedTestInfo.Add($"Math. Pos:{k}, Op: {num1}*{num2}");

                    failed++;
                }
            }

            Console.WriteLine("Large string size tests..\n");
            for (int l = 0; l < count; l++)
            {
                totalTestCount++;
                var paraIndex = new Random().Next(0, 6);
                var additional = new Random().Next(0, 3);
                // defaq if happenin..
                var resp = await NetClient.StringRequest<object>("server", "para", ClientIdentity.ServerIntIdentity, paraIndex, additional);
                respTime += resp.ResponseTime;
                if (resp.Status == StatusCode.OK)
                {
                    Console.WriteLine($"#{l} [para] => ({paraIndex}) = ({resp.ReceivedObject.ToString().Substring(0, 5) ?? "<<NULL>>"}) StrLen:{resp.ReceivedObject.ToString().Length}, Response Time: {resp.ResponseTime}");
                }
                else
                {
                    failedTestInfo.Add($"String. Pos:{l}");

                    failed++;
                }
            }


            Console.WriteLine($"Tests complete. Average response time over {totalTestCount} tests: {respTime / totalTestCount}Ms, with {failed} failed requests.\nInformation:");

            foreach (var t in failedTestInfo)
            {
                Console.WriteLine(t);
            }


            Console.WriteLine("[1] - Standard Input, [2] - Remote client tests");
            var cki = Console.ReadKey();
            if (cki.Key == ConsoleKey.D1)
            {
                await StartStandardSend();
            }
            else if (cki.Key == ConsoleKey.D2)
            {
                Console.WriteLine("Running remote client tests..");

                var clientList = await NetClient.StringRequest<List<string>>("server", "list", ClientIdentity.ServerIntIdentity);

                for (int m = 0; m < 101; m++)
                {
                    string dest = clientList.ReceivedObject[new Random().Next(0, clientList.ReceivedObject.Count)];

                    if (dest == ClientIdentity.CurrentUserStringIdentity)
                    {
                        m--;
                        continue;
                    }

                    Console.WriteLine($"Running tests on client {dest}");
                    for (int j = 0; j < new Random().Next(1, 101); j++)
                    {

                        var resp = await NetClient.StringRequest<List<string>>(dest, "help", ClientIdentity.ServerIntIdentity);
                        Console.WriteLine($"[{m}]:[{j}] : {resp.ReceivedObject} : RT: {resp.ResponseTime}");
                    }
                }
                await StartStandardSend();
            }
            Console.ReadLine();


        }
        private async static Task<bool> authenticationPrompt()
        {
            string username = "";
            try
            {
                Console.Write("Address: ");
                string address = Console.ReadLine();
                Console.Write("Username: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();

                Console.WriteLine("Exiting auth prompt");
                Response<int> authResponse = await NetClient.Connect(address, 1337, new AuthenticationRequest() { Username = username, Password = password });
                Console.WriteLine($"Auth response: {authResponse.ReceivedObject}, {authResponse.Status}");
                Console.Title = $"{ClientIdentity.CurrentIdentity} | {username}";
                if (authResponse != null && authResponse.Status == StatusCode.OK)
                {
                    ClientIdentity.ServerIntIdentity = authResponse.ReceivedObject;

                    Console.Clear();

                    Console.WriteLine($"REC: {authResponse.ReceivedObject.GetType()}, STAT: {authResponse.Status}, RTT: {authResponse.ResponseTime}");
                    return true;
                }

            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            return false;

        }
        #endregion


        #region Logger Events
        private static void Logger_OnExceptionLogCalled(Exception exception, string sender)
        {
            Console.WriteLine($"[Exception] ({sender}): {exception.Message}");
        }
        private static void Logger_OnDebugLogCalled(string logMessage, bool newLineBefore, bool newLineAfter, string sender)
        {
            string message = $"[{sender}]: {logMessage}";

            if (newLineBefore)
                message = message.Prepend('\n').ToString();

            if (newLineAfter)
                message = message.Append('\n').ToString();


            Console.WriteLine(message);
        }
        private static void Logger_OnMessageLogCalled(string message)
        {
            Console.WriteLine(message);
        }
        #endregion



    }
}
