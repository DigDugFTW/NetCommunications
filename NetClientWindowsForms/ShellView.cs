
using NetClientWindowsForms.Controls;
using NetClientWindowsForms.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsShared.EventAggregator;

namespace NetClientWindowsForms
{
    public partial class ShellView : Form, ISubscriber
    {
       
        private IEventAggregator _events;
        private ServerLoginControl _loginControl;
        private ClientCommunicationsControl _commsControl;
        private DebugLogControl _debugControl;
        private DirectMessageControl _directMessageControl;

        public ShellView(IEventAggregator eventAggregator, ServerLoginControl serverLoginControl, ClientCommunicationsControl commsControl, DebugLogControl debugControl, DirectMessageControl directMessageControl)
        {
            _events = eventAggregator;
            _loginControl = serverLoginControl;
            _commsControl = commsControl;
            _debugControl = debugControl;
            _directMessageControl = directMessageControl;

            _events.Subscribe(this, typeof(ServerLogOnEvent));
            _events.Subscribe(this, typeof(ShowDirectMessageControlEvent));

            InitializeComponent(); 
        }

        

        private void ShellView_Load(object sender, EventArgs e)
        {
            
        }

        // Topmost toolstrip
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        void IHandle<EventArgs>.Handle(object sender, EventArgs tEvent)
        {
            if (tEvent is ServerLogOnEvent)
            {
                contentControl.ActivateItem(_commsControl);
            }else if(tEvent is ShowDirectMessageControlEvent)
            {
                contentControl.ActivateItem(_directMessageControl);
            }
        }

        private void serverLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contentControl.ActivateItem(_loginControl);
        }

        private void debugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contentControl.ActivateItem(_debugControl);
        }

        private void communicationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contentControl.ActivateItem(_commsControl);
        }

      
    }
}
