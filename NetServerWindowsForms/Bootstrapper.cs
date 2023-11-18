using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NetServerWindowsForms.Controls;
using WindowsFormsShared.EventAggregator;

namespace NetServerWindowsForms
{
    public class Bootstrapper
    {


        public static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

           
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<ServerStateControl>().UsingConstructor(typeof(IEventAggregator));
            builder.RegisterType<ShellView>().UsingConstructor(typeof(IEventAggregator), typeof(ServerStateControl));
            
            return builder.Build();
        }

    }
}
