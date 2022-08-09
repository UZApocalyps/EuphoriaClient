using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuphoriaClient
{
    public class UdpReader
    {
        private static UdpReader instance;

        public static UdpReader GetInstance()
        {
            if (instance == null)
            {
                instance = new UdpReader();

            }
            return instance;
        }
        private UdpReader()
        {

        }
    }
}
