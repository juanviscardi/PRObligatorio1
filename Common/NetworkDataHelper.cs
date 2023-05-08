using System.Text;
using System.Net.Sockets;

namespace Common
{
    public class NetworkDataHelper
    {

        //private readonly Socket _socket;
        private readonly TcpClient _tcpClient;

        public NetworkDataHelper(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void Send(byte[] data)
        {
            int offset = 0;
            int size = data.Length;

            // Necesitamos pedir el stream para enviar
            var networkStream = _tcpClient.GetStream();

            networkStream.Write(data, offset, size);
        }

        /* public byte[] Receive(int length)
         {
             byte[] response = new byte[length];
             int offset = 0;

             // Necesitamos pedir el stream para recibir
             var networkStream = _tcpClient.GetStream();

             try { 
             while (offset < length)
             {

                 int received = networkStream.Read(response, offset, length - offset);
                 if (received != 0)
                 {
                     offset += received;
                 }
                 else
                 {
                     throw new SocketException();
                 }

             }
             return response;

         }

         public string Receive()
         {
             byte[] dataLength3 = this.Receive(ProtocolSpecification.fixedLength);
             byte[] data3 = this.Receive(BitConverter.ToInt32(dataLength3));
             return Encoding.UTF8.GetString(data3);

         }

         public void Send(string data)
         {
             byte[] bytesData = Encoding.UTF8.GetBytes(data);  // Convierto de string a un array de bytes
             int datalength = bytesData.Length;
             byte[] dataLength = BitConverter.GetBytes(datalength);
             this.Send(dataLength);
             this.Send(bytesData);
         }
     }   
 }
 */
        public string Receive()
        {
            byte[] dataLength3 = this.Receive(ProtocolSpecification.fixedLength);
            byte[] data3 = this.Receive(BitConverter.ToInt32(dataLength3));
            return Encoding.UTF8.GetString(data3);

        }

        public void Send(string data)
        {
            byte[] bytesData = Encoding.UTF8.GetBytes(data);  // Convierto de string a un array de bytes
            int datalength = bytesData.Length;
            byte[] dataLength = BitConverter.GetBytes(datalength);
            this.Send(dataLength);
            this.Send(bytesData);
        }

        public byte[] Receive(int length)
        {
            byte[] response = new byte[length];
            int offset = 0;
            var networkStream = _tcpClient.GetStream();

            try
            {
                while (offset < length)
                {
                    int received = networkStream.Read(response, offset, length - offset);
                    offset += received;
                }
                return response;
            }
            catch
            {
                throw new Exception("Cliente desconectado");

            }
        }
    }
}