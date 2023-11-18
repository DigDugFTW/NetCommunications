using System;
using NetShared;

namespace NetClient.Authentication
{
    public static class ClientIdentity
    {
        private static int _identity = 0;

        public static void InitializeIdentity()
        {
            _identity = NetHelper.GetMacAddress().GetHashCode() ^ new Random().Next();
        }

        public static int CurrentIdentity
        {
            get
            {
                if (_identity == 0)
                {
                    InitializeIdentity();
                }
                return _identity;
            }

        }

        
        public static string CurrentUserStringIdentity { get; set; } = Environment.UserName;
        public static string ServerStringIdentity { get; set; } = "server";
        public static int ServerIntIdentity { get; set; } = -1;
    }
}
