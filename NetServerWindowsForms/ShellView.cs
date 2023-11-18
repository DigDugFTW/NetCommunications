using NetServerWindowsForms.Controls;
using NetShared.Logging;
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

namespace NetServerWindowsForms
{
    public partial class ShellView : Form, ISubscriber
    {
        private IEventAggregator _events;
        private ServerStateControl _stateControl;

        public ShellView(IEventAggregator eventAggregator, ServerStateControl stateControl)
        {
            _events = eventAggregator;
            _stateControl = stateControl;

            InitializeComponent();
        }

        private void ShellView_Load(object sender, EventArgs e)
        {
            
        }

       
        void IHandle<EventArgs>.Handle(object sender, EventArgs tEvent)
        {
            
        }

        private void toolStripTop_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void serverLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contentControl.ActivateItem(_stateControl);
        }
    }
}
