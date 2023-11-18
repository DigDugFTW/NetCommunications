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
using NetShared.NetObject;

namespace NetClientWindowsForms.Controls
{
    public partial class DirectMessageTabPageControl : UserControl
    {
        private Client _client;
        public string DestinationString { get; set; }
        public DirectMessageTabPageControl(Client client)
        {
            _client = client;

            InitializeComponent();
        }

        private void buttonSendDirectMessage_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                _client.StringRequest<object>(DestinationString, "dm", 1, textBoxDirectMessageSend.Text);
            });

            textBoxDirectMessageSend.Clear();
        }

        public void AddChat(string chat)
        {
            richTextBoxDirectMessageReceive.Text += $"\n{chat}";
        }

        private async void textBoxDirectMessageSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                string text = textBoxDirectMessageSend.Text;
                textBoxDirectMessageSend.Clear();

                var res = await _client.StringRequest<object>(DestinationString, "dm", 1, text);

                //Task.Run(() => 
                //{
                //   _client.StringRequest<object>(DestinationString, "dm", 1, text);
                //});
               var tResp = await  _client.StringRequest<object>(DestinationString, "dm", 1, text);

                richTextBoxDirectMessageReceive.Text += $"\nme:{tResp.ReceivedObject}";
            }


        }

        

        private void buttonCloseDirectMessageTab_Click(object sender, EventArgs e)
        {
            
        }
    }
}
