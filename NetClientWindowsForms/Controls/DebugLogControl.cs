using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetShared.Logging;

namespace NetClientWindowsForms.Controls
{
    public partial class DebugLogControl : UserControl
    {
        public DebugLogControl()
        {
            InitializeComponent();


        }

        private void DebugLogControl_Load(object sender, EventArgs e)
        {
            Logger.OnDebugLogCalled += Logger_OnDebugLogCalled; ;
            Logger.OnExceptionLogCalled += Logger_OnExceptionLogCalled;
            Logger.OnMessageLogCalled += Logger_OnMessageLogCalled;
        }

       

        private void Logger_OnMessageLogCalled(string message)
        {
            crossThreadInvoke(richTextBoxClientLog, () => richTextBoxClientLog.Text += $"Message-log:{message}");
        }

        private void Logger_OnExceptionLogCalled(Exception exception, string sender)
        {
            crossThreadInvoke(richTextBoxClientLog, () => richTextBoxClientLog.Text += $"Exception-{sender}:{exception.Message}");
        }

        private void Logger_OnDebugLogCalled(string logMessage, bool newLineBefore, bool newLineAfter, string sender)
        {
            crossThreadInvoke(richTextBoxClientLog, () => richTextBoxClientLog.Text += $"Debug-{sender}:{logMessage}");
        }

        private void crossThreadInvoke(Control control, Action a)
        {
            control.Invoke(a);
        }
    }
}
