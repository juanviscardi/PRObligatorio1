using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Common
{
    public class NetworkDataHelper
    {

        private readonly Socket _socket;

        public NetworkDataHelper(Socket socket)
        {
            _socket = socket;
        }

        public void Send(byte[] data) 
        {
            int offset = 0;
            int size = data.Length;

            while (offset < size)
            {
                int sent = _socket.Send(data, offset, size - offset, SocketFlags.None);
                if (sent != 0)
                {
                    offset += sent;
                }
                else
                {
                    throw new SocketException();    //Se me desconecto o algo
                }
            }
        }

        public byte[] Receive(int length)
        {
            byte[] response = new byte[length];
            int offset = 0;

            while (offset < length)
            {
                int received = _socket.Receive(response, offset, length - offset, SocketFlags.None);
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
