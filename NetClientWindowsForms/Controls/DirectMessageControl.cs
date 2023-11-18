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

namespace NetClientWindowsForms.Controls
{
    public partial class DirectMessageControl : UserControl
    {
       
        public DirectMessageControl()
        {
            InitializeComponent();
        }

        private void DirectMessageControl_Load(object sender, EventArgs e)
        {
             
        }

        public DirectMessageTabPageControl AddTab(string key, string text, DirectMessageTabPageControl tabControl)
        {
            tabControl.Dock = DockStyle.Fill;

            tabControl.DestinationString = text;
            tabControlDirectMessage.TabPages.Add(key, text);
            tabControlDirectMessage.TabPages[key].Controls.Add(tabControl);
            return tabControl;
        }

        public void RemoveTab(string key)
        {
            tabControlDirectMessage.TabPages.RemoveByKey(key);
        }

        public bool ContainsTab(string key)
        {
            return tabControlDirectMessage.TabPages.ContainsKey(key);
        }

        public DirectMessageTabPageControl GetTabPage(string key)
        {
            return (DirectMessageTabPageControl)tabControlDirectMessage.TabPages[key].Controls[0];
        }


    }
}
