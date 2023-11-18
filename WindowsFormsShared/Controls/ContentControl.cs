using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetClientWindowsForms.Controls
{
    public partial class ContentControl : UserControl
    {
        public ContentControl()
        {
            InitializeComponent();
        }

        public void ActivateItem(Control control)
        {
            cti(ActiveItem, () =>
            {
                ActiveItem.Controls.Clear();
                ActiveItem.Controls.Add(control);
            });
        }

        private void cti(Control c, Action a)
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
    }
}
