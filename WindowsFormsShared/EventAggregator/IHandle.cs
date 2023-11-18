using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsShared.EventAggregator
{
    public interface IHandle<T> where T : EventArgs
    {
        void Handle(object sender, T tEvent);
    }
}
