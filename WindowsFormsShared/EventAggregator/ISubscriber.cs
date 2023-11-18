using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsShared.EventAggregator
{
    public interface ISubscriber : IHandle<EventArgs>
    {
    }
}
