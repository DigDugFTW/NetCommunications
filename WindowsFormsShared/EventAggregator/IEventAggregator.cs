using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsShared.EventAggregator
{
    public interface IEventAggregator
    {
        void Publish(object sender, EventArgs e);
        void Subscribe(ISubscriber subscriber, Type eventType);
    }
}
