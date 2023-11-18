using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsShared.EventAggregator
{
    // <summary>
    /// Handles events between forms and controls.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {

        public EventAggregator()
        {

        }

        public Dictionary<Type, IList<ISubscriber>> SubscriberCollection = new Dictionary<Type, IList<ISubscriber>>();


        // Listen for an event
        public void Subscribe(ISubscriber subscriber, Type eventType)
        {

            // Handle repeated add.
            if (SubscriberCollection.ContainsKey(eventType))
            {
                if (SubscriberCollection[eventType].Contains(subscriber))
                    return;

                SubscriberCollection[eventType].Add(subscriber);
            }
            else
            {
                SubscriberCollection.Add(eventType, new List<ISubscriber>() { subscriber });
            }

            // Add to Subscriber Collection
            //SubscriberCollection.Add(eventType, subscriber);

        }

        // Push an event
        public void Publish(object sender, EventArgs e)
        {
            Type t = e.GetType();
            bool isValid = SubscriberCollection.TryGetValue(t, out IList<ISubscriber> subscribers);
            if (isValid)
            {
                foreach (ISubscriber subscriber in subscribers)
                {
                    subscriber.Handle(sender, e);
                }
            }


        }
    }
}
