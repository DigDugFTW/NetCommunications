using NetShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetServer.Authentication
{
    public static class ServerIdentity
    {
        private static int _intIdentity = 0;

        public static void InitializeIdentity()
        {
            _intIdentity = NetHelper.GetMacAddress().GetHashCode() ^ new Random().Next();
        }

        public static int CurrentIdentity 
        {
            get
            {
                if(_intIdentity == 0)
                {
                    InitializeIdentity();
                }

                return _intIdentity;
            }
        }

    }
}
