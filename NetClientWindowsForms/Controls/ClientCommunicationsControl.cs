using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsShared.EventAggregator;
using NetClient;
using NetShared.NetObject;
using static NetShared.Net.TcpFramer;
using NetShared.NetParser;
using NetShared.Logging;
using System.Threading;
using NetClientWindowsForms.Events;
using NetShared.Net;
using NetClient.Authentication;

namespace NetClientWindowsForms.Controls
{
    public partial class ClientCommunicationsControl : UserControl
    {
        private IEventAggregator _events;
        private Client _client;
        private DirectMessageControl _directMessageControl;

     
        public ClientCommunicationsControl(IEventAggregator events, DirectMessageControl directMessageControl, Client client)
        {
            _events = events;

            _directMessageControl = directMessageControl;
            _client = client;

            InitializeComponent();
        }

        private void ClientCommunicationsControl_Load(object sender, EventArgs e)
        {
            _client.Parser.AddHandler<RequestMessage, Task<ParserResponse>>(RequestMessageHandler)
                .AddChild("chat", doChat)
                .AddChild("client_dcon", doRemoveClient)
                .AddChild("client_con", doAddClient)
                .AddChild("dm", doDm);
        }

        private async Task<ParserResponse> RequestMessageHandler(ParserArgs args, RequestMessage request)
        {
            try
            {
                var ret = await _client.Parser.CallHandlerChild<RequestMessage, ParserResponse, string>(request.RequestObject.ToString(), args, request);
                return (ParserResponse)ret;
            }
            catch (Exception e)
            {
                return new ParserResponse($"{e.Message}", StatusCode.ERROR);
            }

         
        }

      

      

       

        private ParserResponse doChat(ParserArgs args, RequestMessage request)
        {
            crossThreadInvoke(listViewClientChat, () => listViewClientChat.Items.Add($"{request.SenderStringIdentifier}:{request.RequestParameters[0].ToString()}"));

            return default;
        }

        private ParserResponse doDm(ParserArgs args, RequestMessage request)
        {
            string stringFormat = $"{request.SenderStringIdentifier}:{request.RequestParameters[0].ToString()}";
            // Called when direct message has been received.
            //crossThreadInvoke(toolStripButtonShowDirectMessage, () => toolStripButtonShowDirectMessage.BackColor = Color.Yellow);

            if (!_directMessageControl.ContainsTab(request.SenderStringIdentifier))
            {
                crossThreadInvoke(_directMessageControl, () => _directMessageControl.AddTab(request.SenderStringIdentifier, request.SenderStringIdentifier, new DirectMessageTabPageControl(_client)));
            }

            var tab = _directMessageControl.GetTabPage(request.SenderStringIdentifier);

            crossThreadInvoke(tab, () => tab.AddChat(stringFormat));

            return default;
        }

        private ParserResponse doAddClient(ParserArgs args, RequestMessage request)
        {
            string client = request.RequestParameters[0]?.ToString();
            crossThreadInvoke(listViewClients, () => listViewClients.Items.Add(client));

            return default;
        }

        private ParserResponse doRemoveClient(ParserArgs args, RequestMessage request)
        {
            string client = request.RequestParameters[0]?.ToString();

            crossThreadInvoke(listViewClients, () =>
            {
                foreach (ListViewItem c in listViewClients.Items)
                {
                    if (c.Text.Equals(client))
                    {
                        listViewClients.Items.Remove(c);
                    }
                }
            }
            );

            return default;
        }

        private async void textBoxChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                string text = textBoxChatInput.Text;

                var result = await _client.StringRequest<object>(ClientIdentity.ServerStringIdentity, "chat", ClientIdentity.ServerIntIdentity, text);
                crossThreadInvoke(listViewClientChat, () => 
                {
                    listViewClientChat.Items.Add($"[{result.Sender}]: {result.ReceivedObject ?? "Null"}");
                });



                textBoxChatInput.Clear();
            }
        }

        private void crossThreadInvoke(Control c, Action a)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(a);
            }
            else
            {
                a.Invoke();
            }
        }

        private void dMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedClient = listViewClients.SelectedItems[0];
            // show direct message control
            if (!_directMessageControl.ContainsTab(selectedClient.Text))
            {
                _directMessageControl.AddTab(selectedClient.Text, selectedClient.Text, new DirectMessageTabPageControl(_client));
            }

            _events.Publish(this, new ShowDirectMessageControlEvent());
            // check if dm page is created for destination
            // if not create page


        }

        private void toolStripButtonShowDirectMessage_Click(object sender, EventArgs e)
        {
            toolStripButtonShowDirectMessage.BackColor = Color.DeepSkyBlue;
            _events.Publish(this, new ShowDirectMessageControlEvent());
        }
    }
}
