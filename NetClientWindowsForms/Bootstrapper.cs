
using NetClientWindowsForms.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetClient;
using Autofac;
using WindowsFormsShared.EventAggregator;

namespace NetClientWindowsForms
{
    public static class Bootstrapper
    {
  

        public static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<ShellView>().UsingConstructor(typeof(IEventAggregator), typeof(ServerLoginControl), typeof(ClientCommunicationsControl), typeof(DebugLogControl), typeof(DirectMessageControl));

            builder.RegisterType<DirectMessageControl>().UsingConstructor().SingleInstance();
            builder.RegisterType<DebugLogControl>();

            builder.RegisterType<Client>().SingleInstance();

            builder.RegisterType<ServerLoginControl>().UsingConstructor(typeof(IEventAggregator), typeof(Client));
            builder.RegisterType<ClientCommunicationsControl>().UsingConstructor(typeof(IEventAggregator), typeof(DirectMessageControl), typeof(Client));

           

            return builder.Build();
        }

    }
}
