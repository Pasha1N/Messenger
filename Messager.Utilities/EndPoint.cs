using System.Net;
using System.Text;

namespace Messenger.Utilities
{
    public class EndPoint
    {
        public IPAddress IPAddress { get; set; }

        public int Port { get; set; }

        public void CleaningUpResources()
        {
            IPAddress = null;
            Port = 0;
        }

        public EndPoint GetEndPoint(string remoteEndPoint)
        {
            StringBuilder stringBuilder = new StringBuilder(remoteEndPoint);
            int idCurrentSymbol = 0;
            EndPoint endPoint = new EndPoint();

            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (stringBuilder[i] == ':')
                {
                    idCurrentSymbol = i;
                    break;
                }
            }

            if (idCurrentSymbol > 0)
            {
                int endPointPort = int.Parse(remoteEndPoint.Substring(idCurrentSymbol + 1, remoteEndPoint.Length - (idCurrentSymbol + 1)));

                IPAddress iPEndPoint = IPAddress.Parse((remoteEndPoint.Substring(0, idCurrentSymbol)));

                endPoint.IPAddress = iPEndPoint;
                endPoint.Port = endPointPort;
            }
            else
            {
                endPoint = null;
            }

            return endPoint;
        }
    }
}