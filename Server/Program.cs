using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;


namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            int serverPort = int.Parse(Properties.Resources.ServerPort);
            string serverAddress = Properties.Resources.ServerIp;

            Console.WriteLine("Starting Server Application.....!!!");
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);
            IPEndPoint localEndPoint = new(IPAddress.Parse(serverAddress), serverPort);
            

            // Asociamos el socket con el endpoint
            socketServer.Bind(localEndPoint);

            socketServer.Listen(100); // Nuestro Socket pasa a estar en modo escucha,
                                      // el 100 me dice que puedo manejar la espera de varios a la vez, es el backlog,
                                      // la cola de conexiones que aceptara

            Console.WriteLine("Waiting for Clients.....");

            while (true)  // OJO CON ESTE WHILE TRUE, MEJOR PONER UN LIMITE X Y DJAR EN ENTRAR AL RESTO
                          //********************************
                          //*******************************
            {
                Socket socketClient = socketServer.Accept();
                    // El Accept es bloqueante
                    // Espera a que llegue una nueva conexion,
                    // por eso fue el 100 en el listen sino bloquea y no podra hacer nada mas.
                Console.WriteLine("Acepte un nuevo pedido de conexion");

                //Feo pero funciona - Algo para saber quien soy
                Console.WriteLine(socketClient.RemoteEndPoint);

                    // ******* PONER UN MENSAJE ACORDE
                    // Usando el nombre/valor del puerto usado tal vez

                new Thread(() => HandleClient(socketClient)).Start(); 
                    // Lanzamos un nuevo hilo para manejar al nuevo cliente

            } 
         }

        static void HandleClient(Socket socketClient) 
        {

            bool clientIsConnected = true;
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(socketClient);
            const int largoDataLength = 4;  // Defino la constante del largo ******* HAY QUE DEFINIRLO MAS PROLLIJO EN OTRO LADO
                                            // ******************************************************************************

            while (clientIsConnected) 
            {
                
                try
                {

                    byte[] dataLength = networkdatahelper.Receive(largoDataLength);
                    byte[] data = networkdatahelper.Receive(BitConverter.ToInt32(dataLength));
                     string message = Encoding.UTF8.GetString(data);
                        Console.WriteLine("Mensaje Recibido: {0}", message);
                    
                    // Feo pero funciona - Algo para saber quien soy
                    Console.WriteLine(socketClient.RemoteEndPoint);

                }
                catch (SocketException ex) 
                {
                    //*************************************************
                    // ******* PONER UN MENSAJE ACORDE
                    // Usando el valor de ex tal vez
                    
                    Console.WriteLine("ERROR: {0}", ex.ToString() + "\n" + socketClient.RemoteEndPoint + "\n");

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
