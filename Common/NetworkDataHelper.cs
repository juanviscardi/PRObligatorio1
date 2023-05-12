using System.Text;
using System.Net.Sockets;

namespace Common
{
    public class NetworkDataHelper
    {

        private readonly TcpClient _tcpClient;

        public NetworkDataHelper(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public async Task Send(byte[] data)
        {
            int offset = 0;
            int size = data.Length;

            // Necesitamos pedir el stream para enviar
            var networkStream = _tcpClient.GetStream();

            await networkStream.WriteAsync(data, offset, size);
        }

        public async Task <string> Receive()
        {
            byte[] dataLength3 = await this.Receive(ProtocolSpecification.fixedLength);
            byte[] data3 = await this.Receive(BitConverter.ToInt32(dataLength3));
            return Encoding.UTF8.GetString(data3);

        }

        public async Task Send(string data)
        {
            byte[] bytesData = Encoding.UTF8.GetBytes(data);  // Convierto de string a un array de bytes
            int datalength = bytesData.Length;
            byte[] dataLength = BitConverter.GetBytes(datalength);
            await this.Send(dataLength);
            await this.Send(bytesData);
        }


        public async Task <byte[]> Receive(int length)
        {
            byte[] response = new byte[length];
            int offset = 0;
            var networkStream = _tcpClient.GetStream();

            try
            {
                while (offset < length)
                {
                    int received = await networkStream.ReadAsync(response, offset, length - offset);
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