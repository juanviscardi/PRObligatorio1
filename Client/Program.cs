using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Reflection;
using Common;
using System.Text.RegularExpressions;
using System.Runtime.ConstrainedExecution;

namespace Client
{
    public class Program
    {
        
        public static void Main(string[] args)
        {   
            // Levanto IP y puertos de archivo
            int serverPort = int.Parse(Properties.Resources.ServerPort);
            int clientPort = int.Parse(Properties.Resources.ClientPort);
            string clientAddress = Properties.Resources.ClientIp;
            string serverAddress = Properties.Resources.ServerIp;
            //Termino de traer info de archivos

            Console.WriteLine("Starting Client Application...!");
            var socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Poner el puerto en 0 le indico que utilice el primero disponible
            //var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(clientAddress), clientPort);
            

            // Aca le defino el endpoint del servidor
            //var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);
            //********** serverAddress hace pum

            socketClient.Bind(localEndPoint);
            Console.WriteLine("Starting Client");
            Console.WriteLine("Connecting.......");
            socketClient.Connect(remoteEndPoint); // Me conecto al servidor

            Console.WriteLine("Connected to Server!!!!");

            /******* DE ACA PARA ABAJO ES LO QUE HAY QUE ENAPSULAR   **************************************/
            Console.WriteLine("Type a message a press enter to send it");
            bool salir = false;
            
            string cmd = "";
            cmd = Console.ReadLine();
            switch (cmd)
            {
                case "0": //CRF0 
                           //Conectarse (previa autenticación) y desconectarse al servidor. 
                           //Se deberá ser capaz de conectarse y desconectarse del servidor, implica autenticación.
                    {
                        Console.WriteLine("Please enter username");
                        string username = Console.ReadLine();
                        Console.WriteLine("Please enter password");
                        string password = Console.ReadLine();
                        //Armo string que voy a mandar
                        string data = username + "|||" + password;
                        //Mando la info al server
                        Console.WriteLine(data);
                        //VVeo que hago segun lo que me dice el server
                    }
                    break;
                case "1":   //CRF1 Alta de usuario
                            // Alta de usuario. Se debe poder dar de alta a un usuario (mecánico). 
                            // Estafuncionalidad solo puede realizarse desde el usuario admin.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "2":   //CRF2 Alta de repuesto.
                            //Se debe poder dar de alta a un repuesto en el sistema, incluyendo
                            //id, nombre, proveedor y marca.


                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "3":   //CRF3 Alta de Categoría de repuesto.
                            //El sistema debe permitir crear una Categoría para los repuestos.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "4":   //CRF4 Asociar Categorías a los repuestos.
                            //El sistema debe permitir asociar categorías a los repuestos.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "5":   //CRF5 Asociar foto a repuesto.
                            //El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "6":   //CRF6 Consultar repuestos existentes.
                            //El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "7":   //CRF7 Consultar un repuesto específico.
                            //El sistema deberá poder buscar un repuesto específico.
                            //También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "8":   //CRF8 Enviar y recibir mensajes.
                            //El sistema debe permitir que un mecánico envíe mensajes a otro,
                            //y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;
                case "9":   //CRF9  Configuración.
                            //Se deberá ser capaz de modificar los puertos e ip utilizados por el cliente y la clave del usuario admin sin necesidad de recompilar el proyecto.
                            //Dichos valores no deben estar “hardcodeados” en el código.
                    {
                        Console.WriteLine("TODO");
                    }
                    break;


                default:
                    {
                        salir = false;
                        break;
                    }
            }


            NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(socketClient);
            while (!salir) 
            {
                
                var message = Console.ReadLine();
                if (string.IsNullOrEmpty(message) || message.Equals("exit", StringComparison.Ordinal))
                {
                    salir = true;
                }
                else
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);  // Convierto de string a un array de bytes
                    int datalength = data.Length;
                    byte[] dataLength = BitConverter.GetBytes(datalength);
                    try
                    {
                        networkdatahelper.Send(dataLength);
                        networkdatahelper.Send(data);
                    }
                    catch (SocketException) 
                    {
                        Console.WriteLine("Perdi la conexion con el server");
                        salir = true;

                    }

                }
            }

           
            Console.WriteLine("Will close Connection....");

            socketClient.Shutdown(SocketShutdown.Both); // Desconecto ambos sentidos de la connecion
            socketClient.Close();
        }
    }
}
