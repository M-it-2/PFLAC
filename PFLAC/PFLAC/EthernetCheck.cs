using System.Net.NetworkInformation;

namespace PFLAC
{
  public class EthernetCheck
  {
    public static bool IsEthernetAvailable()
    {
      try
      {
        using (Ping ping = new Ping())
        {
          PingReply reply = ping.Send("1.1.1.1");
          return reply.Status == IPStatus.Success;
        }
      }
      catch
      {
        return false;
      }
    }
  }
}
