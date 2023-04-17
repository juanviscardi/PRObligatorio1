using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using System.ComponentModel.Design;

namespace ClientApp
{
    public class ProgramClient
    {
        static readonly SettingsManager settingsMng = new SettingsManager();

        public static void Main(string[] args)
        {
            // Levanto IP y puertos de archivo

            //Sustituyo ip y port por los valores del archivo
            string serverIp = settingsMng.ReadSettings(ClientConfig.serverIPConfigKey);
            string clientIp = settingsMng.ReadSettings(ClientConfig.ClientIPConfigKey);
            int serverPort = int.Parse(settingsMng.ReadSettings(ClientConfig.serverPortconfigKey));
            int clientPort = int.Parse(settingsMng.ReadSettings(ClientConfig.ClientPortconfigKey));

            //endPoint hacia server
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            //socket y EndPint Ciente
            Console.WriteLine("Starting Client Application...!");
            var socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), clientPort);

            socketClient.Bind(localEndPoint);
            Console.WriteLine("Starting Client");
            Console.WriteLine("Connecting.......");

            socketClient.Connect(remoteEndPoint); // Me conecto al servidor
            Console.WriteLine("Connected to Server!!!!");

            NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(socketClient);

            /******* DE ACA PARA ABAJO ES LO QUE HAY QUE ENAPSULAR   **************************************/

            bool salir = false;

            var cmd = "";

            while (!salir)
            {
                Console.WriteLine("0 - Conectarse (previa autenticación) y desconectarse al servidor");
                Console.WriteLine("1 - Alta de usuario");
                Console.WriteLine("2 - Alta de repuesto");
                Console.WriteLine("3 - Alta de Categoría de repuesto");
                Console.WriteLine("4 - Asociar Categorías a los repuestos");
                Console.WriteLine("5 - Asociar foto a repuesto");
                Console.WriteLine("6 - Consultar repuestos existentes");
                Console.WriteLine("7 - Consultar un repuesto específico");
                Console.WriteLine("8 - Enviar y recibir mensajes");
                Console.WriteLine("9 - Configuración");

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
                            string data0 = cmd +
                                    ProtocolSpecification.fieldsSeparator +
                                    username + ProtocolSpecification.fieldsSeparator +
                                    password;
                            //Mando la info al server
                            //Console.WriteLine(data0);

                            byte[] data = Encoding.UTF8.GetBytes(data0);  // Convierto de string a un array de bytes
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

                            //}


                            //}

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
                            Console.WriteLine("CRF2 Alta de repuesto.");
                            Console.WriteLine("Se debe poder dar de alta a un repuesto en el sistema, incluyendo");
                            Console.WriteLine("id, nombre, proveedor y marca .");

                            Console.WriteLine("Please enter nombre repueasto");
                            string nombreRepuesto = Console.ReadLine();
                            Console.WriteLine("Please enter provedor repueasto");
                            string proveedorRepuesto = Console.ReadLine();
                            Console.WriteLine("Please enter marca repueasto");
                            string marcaRepuesto = Console.ReadLine();

                            //PRUEBA***************************
                            
                            repuesto repu = new repuesto(
                                                    nombreRepuesto ?? string.Empty, 
                                                    proveedorRepuesto ?? string.Empty, 
                                                    marcaRepuesto ?? string.Empty);

                            string data0 = cmd + ProtocolSpecification.fieldsSeparator + 
                                nombreRepuesto + ProtocolSpecification.fieldsSeparator +
                                proveedorRepuesto + ProtocolSpecification.fieldsSeparator +
                                marcaRepuesto;

                            byte[] data = Encoding.UTF8.GetBytes(data0);  // Convierto de string a un array de bytes
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

                            //*********************************


                            Console.WriteLine(data);
                            Console.WriteLine("TODO");
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "3":   //CRF3 Alta de Categoría de repuesto.
                                //El sistema debe permitir crear una Categoría para los repuestos.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "4":   //CRF4 Asociar Categorías a los repuestos.
                                //El sistema debe permitir asociar categorías a los repuestos.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "5":   //CRF5 Asociar foto a repuesto.
                                //El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "6":   //CRF6 Consultar repuestos existentes.
                                //El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "7":   //CRF7 Consultar un repuesto específico.
                                //El sistema deberá poder buscar un repuesto específico.
                                //También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;
                    case "8":   //CRF8 Enviar y recibir mensajes.
                                //El sistema debe permitir que un mecánico envíe mensajes a otro,
                                //y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.

                        Console.WriteLine("Type a message a press enter to send it");
                        //NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(socketClient);
                        bool salirCRF8 = false;
                        while (!salirCRF8 && !salir)
                        {
                            var message = Console.ReadLine();
                            if (string.IsNullOrEmpty(message) || message.Equals("exit", StringComparison.Ordinal))
                            {
                                salirCRF8 = true;
                            }
                            else
                            {
                                message = cmd + ProtocolSpecification.fieldsSeparator + message;

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

                            //cmd = Console.ReadLine();
                            //salirCRF8 = true;
                        }


                        break;
                    case "9":   //CRF9  Configuración.
                                //Se deberá ser capaz de modificar los puertos e ip utilizados por el cliente y la clave del usuario admin sin necesidad de recompilar el proyecto.
                                //Dichos valores no deben estar “hardcodeados” en el código.
                        {
                            Console.WriteLine("TODO");
                            Console.ReadLine();
                        }
                        break;


                    default:
                        {
                            salir = false;
                            break;
                        }
                }
            }




            Console.WriteLine("Will close Connection....");

            socketClient.Shutdown(SocketShutdown.Both); // Desconecto ambos sentidos de la connecion
            socketClient.Close();
            socketClient.Dispose();
        }
    }
}
