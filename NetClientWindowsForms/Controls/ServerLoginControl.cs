using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetClient;
using NetClient.Authentication;
using WindowsFormsShared.EventAggregator;
using NetClientWindowsForms.Events;
using NetShared.NetObject;
using NetShared.Net;
using NetShared.NetParser;

namespace NetClientWindowsForms.Controls
{
    public partial class ServerLoginControl : UserControl
    {
        private IEventAggregator _events;
        private Client _client;
        
        public ServerLoginControl(IEventAggregator events, Client client)
        {
            _events = events;
            _client = client;

            _client.Parser = new Parser();
            _client.ClientComms = new TcpComms(new Serializer(), new TcpFramer()) { TcpClient = new System.Net.Sockets.TcpClient() };

            _client.AttachEvents();

      


            InitializeComponent();
        }

        

        private async void buttonServerLogin_Click(object sender, EventArgs e)
        {
           
            try
            {
                Response<int> connectionResponse = await _client.Connect(textBoxServerAddress.Text, 1337, new AuthenticationRequest() { Username = textBoxUsername.Text, Password = textBoxPassword.Text, DestinationStringIdentifier = "server", SenderStringIdentifier = textBoxUsername.Text });
               
                if (connectionResponse != null && connectionResponse.Status == NetShared.Net.TcpFramer.StatusCode.OK)
                {
                    ClientIdentity.CurrentUserStringIdentity = textBoxUsername.Text;
                    
                    _events.Publish(this, new ServerLogOnEvent());
                }
                else
                {
                

                    textBoxUsername.Clear();
                    textBoxServerAddress.Clear();
                    textBoxPassword.Clear();
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
