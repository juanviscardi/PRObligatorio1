using Common;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMng = new SettingsManager();
        static Dictionary<string, string> usuarios = new();
        static Dictionary<string, string[]> mensajes = new();
        static List<repuesto> repuestos = new();


        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Server Application.....!!!");

            var socketServer = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            //Sustituimos ip y port por los valores del archivo
            string serverIp = settingsMng.ReadSettings(ServerConfig.serverIPConfigKey);
            int serverPort = int.Parse(settingsMng.ReadSettings(ServerConfig.serverPortconfigKey));

            //var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            Console.WriteLine("Server initialized with IP {0} and Port {1}", serverIp, serverPort);

            // Asociamos el socket con el endpoint
            socketServer.Bind(localEndPoint);

            socketServer.Listen(100); // Nuestro Socket pasa a estar en modo escucha,
                                      // el 100 me dice que puedo manejar la espera de varios a la vez, es el backlog,
                                      // la cola de conexiones que aceptara

            int clientes = 0;
            bool salir = false;

            Console.WriteLine("Waiting for Clients.....\n");

            while (!salir)
            {
                Socket socketClient = socketServer.Accept();
                // El Accept es bloqueante
                // Espera a que llegue una nueva conexion,
                // por eso fue el 100 en el listen sino bloquea y no podra hacer nada mas.
                clientes++;
                int nro = clientes;

                Console.WriteLine("Acepte un nuevo pedido de conexion");

                //Feo pero funciona - Algo para saber quien soy
                string algo = socketClient.RemoteEndPoint.ToString()
                                  ?? string.Empty;

                string[] datos = algo.Split(ProtocolSpecification.valuesSeparator);
                Console.WriteLine("Se conecto {0} en el puerto {1} \n", datos[0], datos[1]);
                // ******* PONER UN MENSAJE Acorde

                new Thread(() => HandleClient(socketClient)).Start();
                // Lanzamos un nuevo hilo para manejar al nuevo cliente

            }
            // Cierro el socket
            socketServer.Shutdown(SocketShutdown.Both);
            socketServer.Close();
            socketServer.Dispose();
        }

        static void HandleClient(Socket socketClient)
        {

            bool clientIsConnected = true;
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(socketClient);
            //const int largoDataLength = 4;  // Defino la constante del largo ******* HAY QUE DEFINIRLO MAS PROLLIJO EN OTRO LADO
            // ******************************************************************************

            while (clientIsConnected)
            {

                try
                {
                    byte[] dataLength = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                    //byte[] dataLength = networkdatahelper.Receive(largoDataLength);
                    byte[] data = networkdatahelper.Receive(BitConverter.ToInt32(dataLength));
                    string message = Encoding.UTF8.GetString(data);
                    string[] messageConTodo = message.Split(ProtocolSpecification.fieldsSeparator);

                    //Console.WriteLine("Mensaje Recibido: {0}", message);
                    //string[] data8 = message.Split(ProtocolSpecification.fieldsSeparator);
                    // Feo pero funciona - Algo para saber quien soy
                    string algo = socketClient.RemoteEndPoint.ToString() ?? string.Empty;
                    
                    string[] datos = algo.Split(":"); //IPAddress : Puerto

                    // Console.WriteLine("Mensaje Recibido: {0} desde {1} en el puerto {2} \n", message, datos[0], datos[1]);
                    switch (messageConTodo[0])
                    {
                        case "0":
                            {
                                var usuario = messageConTodo[1];
                                var pass = messageConTodo[2];
                                Console.WriteLine("Usuario: {0} pass: {1}", messageConTodo[1], messageConTodo[2]);
                                usuarios.Add(usuario,pass);
                              string freno = "";
                                break;

                            }

                        case "8":
                            {
                                mensajes.Add(messageConTodo[1], datos);
                                Console.WriteLine("Mensaje Recibido: {0} desde {1} en el puerto {2} \n", messageConTodo[1], datos[0], datos[1]);
                                string freno2 = "";
                                break;
                            }

                        //********************************
                        case "2":
                            {
                                var repuestoName = messageConTodo[1];
                                var repuestoProveedor = messageConTodo[2];
                                var repuestoMarca = messageConTodo[3];

                                repuesto repu = new repuesto(
                                                   repuestoName,
                                                   repuestoProveedor,
                                                   repuestoMarca);

                                repuestos.Add(repu);

                                string freno3 = "";
                                break;
                            }
                            
                            //*******************************

                        default: { break; }
                    } 


                }
                catch (SocketException ex)
                {
                    //*************************************************
                    // ******* PONER UN MENSAJE ACORDE
                    // Usando el valor de ex tal vez

                    Console.WriteLine("ERROR:" + "Client disconnected a lo bruto \n" + socketClient.RemoteEndPoint + "\n");

                    // ESTE ERROR ME LO COMO (NO LO MUESTRO)
                    // O LO TIRO A UNA BASE DE EVENTOS/ERRORES
                    // **********************************************

                    clientIsConnected = false;
                }
            }
            Console.WriteLine("Client disconnected");
            Console.WriteLine(socketClient.RemoteEndPoint);
        }
    }
}
