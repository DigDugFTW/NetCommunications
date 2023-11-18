using NetShared;
using NetShared.Net;
namespace NetTcpSandBoxTesting
{
    public static class Class1
    {
        public static void main(string[] args)
        {
            Console.WriteLine("NetShared lib details:");
            Console.WriteLine($"{NetShared.NetHelper.GetPublicIp}");
            Console.WriteLine($"{NetShared.NetHelper.GetPrivateIp}");
            Console.WriteLine($"{NetShared.NetHelper.GetMacAddress}");
            Console.ReadKey();
        }



    }
}