using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace PFLAC_WPF.Services
{
    public class NetworkChecker
    {
        private readonly string _testHost;

        public NetworkChecker(string testHost = "1.1.1.1")
        {
            _testHost = testHost;
        }

        public bool IsNetworkAvailable()
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(_testHost, 1000);

                return reply?.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
          }
    }
}
