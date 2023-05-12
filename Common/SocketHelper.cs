using System.Net.Sockets;

namespace Common
{
    public class SocketHelper
    {
        private readonly TcpClient _tcpClient;

        public SocketHelper(TcpClient tcpClient)
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

        public async Task<byte[]> Receive(int length)
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

    /*public class SocketHelper
    {
        private readonly Socket _socket;

        public SocketHelper(Socket socket)
        {
            _socket = socket;
        }

        public void Send(byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                var sent = _socket.Send(
                    data,
                    offset,
                    data.Length - offset,
                    SocketFlags.None);
                if (sent == 0)
                    throw new Exception("Connection lost");
                offset += sent;
            }
        }

        public byte[] Receive(int length)
        {
            int offset = 0;
            var data = new byte[length];
            while (offset < length)
            {
                var received = _socket.Receive(
                    data,
                    offset,
                    length - offset,
                    SocketFlags.None);
                if (received == 0)
                    throw new Exception("Connection lost");
                offset += received;
            }

            return data;
        }
    }*/


}