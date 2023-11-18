using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTcpDebugSandBox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Net Details:");
           
            Console.WriteLine($"{NetShared.NetHelper.GetPublicIp()}");
            Console.WriteLine($"{NetShared.NetHelper.GetPrivateIp()}");
            Console.WriteLine($"{NetShared.NetHelper.GetMacAddress()}");
            Console.ReadKey();
        }
    }
}
