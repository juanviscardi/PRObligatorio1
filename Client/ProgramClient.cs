﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using System.ComponentModel.Design;
using System.IO;

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

            // Si las credenciales no correctas me conecto al server sino que siga tratando
            socketClient.Connect(remoteEndPoint); // Me conecto al servidor
            Console.WriteLine("Connected to Server!!!!");

            NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(socketClient);
            //Inicializo cmd en 0 que es la opcion de login en el servidor
            string userType = "error";
            //Mando la info al server
            while (string.Equals(userType, "error"))
            {
                try
                {
                    //Pido credenciales
                    Console.WriteLine("Ingrese Usuario: ");
                    string username = Console.ReadLine() ?? string.Empty;
                    Console.WriteLine("Ingrese Contrasena: ");
                    string password = Console.ReadLine() ?? string.Empty;
                    //Armo string que voy a mandar
                    string usernamePassword = username + ProtocolSpecification.fieldsSeparator + password;
                    networkdatahelper.Send(usernamePassword);
                    byte[] dataLength0 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                    byte[] data0 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength0));
                    string message0 = Encoding.UTF8.GetString(data0);
                    userType = message0;
                    if (string.Equals(userType, "error"))
                    {
                        Console.WriteLine("El usuario o contrasena no es correcto");
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine("Perdi la conexion con el server");

                }
            }

            /******* DE ACA PARA ABAJO ES LO QUE HAY QUE ENAPSULAR   **************************************/
            bool salir = false;
            while (!salir)
            {
                ConsoleClientMenu.GetMenu(userType);
                string cmd = Console.ReadLine() ?? string.Empty;
                switch (userType)
                {
                    case "admin":
                        {
                            switch (cmd)
                            {
                                case "1":
                                    //CRF1 Alta de usuario
                                    // Alta de usuario. Se debe poder dar de alta a un usuario (mecánico). 
                                    // Estafuncionalidad solo puede realizarse desde el usuario admin.
                                    // Console.WriteLine("1 - Anadir usuario");
                                    Console.WriteLine("Ingrese Usuario: ");
                                    string username = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese Contrasena: ");
                                    string password = Console.ReadLine() ?? string.Empty;
                                    //Chequeo que no exista el usuarion que quiero crear
                                    string message = username + ProtocolSpecification.fieldsSeparator + password;
                                    networkdatahelper.Send(cmd);
                                    networkdatahelper.Send(message);

                                    byte[] dataLength0 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                                    byte[] data0 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength0));
                                    string message0 = Encoding.UTF8.GetString(data0);

                                    Console.WriteLine(message0);
                                    //Creo el usuario

                                    //Si existe vuelvo a pedir la data o doy opcion a salir

                                    break;
                               
                                case "2": 
                                    // Console.WriteLine("3 - Cerrar Sesion");
                                    salir = true;
                                    break;
                            }
                            break;
                        }
                    case "mecanico":
                        {

                            switch (cmd)
                            {
                                case "1":
                                    Console.WriteLine("CRF2 Alta de repuesto.");
                                    Console.WriteLine("Se debe poder dar de alta a un repuesto en el sistema, incluyendo");
                                    Console.WriteLine("id, nombre, proveedor y marca .");




                                    break;
                                case "2":
                                    // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                    //CRF3 Alta de Categoría de repuesto.
                                    //El sistema debe permitir crear una Categoría para los repuestos.
                                    break;
                                case "3":
                                    // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                    //CRF4 Asociar Categorías a los repuestos.
                                    //El sistema debe permitir asociar categorías a los repuestos.
                                    break;
                                case "4":
                                    // Console.WriteLine("4 - Asociar foto a repuesto");
                                    //CRF5 Asociar foto a repuesto.
                                    //El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                    FileHandler fileHandler = new FileHandler();
                                    Console.WriteLine("Por favor escribir el path del archivo a transferir");
                                    string path = Console.ReadLine() ?? string.Empty;
                                    while (string.IsNullOrEmpty(path) || path.Equals("exit", StringComparison.Ordinal) || !fileHandler.FileExists(path))
                                    {
                                        if (path.Equals("exit", StringComparison.Ordinal))
                                        {
                                            break;
                                        }
                                        if (string.IsNullOrEmpty(path))
                                        {
                                            Console.WriteLine("El path no puede ser vacio, escriba uno o escriba exit para salir");
                                            path = Console.ReadLine() ?? string.Empty;
                                            continue;
                                        }
                                        if (!fileHandler.FileExists(path))
                                        {
                                            Console.WriteLine("El path no lleva a no lleva a ningun archivo valido, por favor  escriba uno o escriba exit para salir");
                                            path = Console.ReadLine() ?? string.Empty;
                                            continue;
                                        }

                                    }
                                    Console.WriteLine("Escribir el nombre del repuesto para asociarle la foto");
                                    string nombreRepuesto = Console.ReadLine() ?? string.Empty;
                                    networkdatahelper.Send(cmd);
                                    networkdatahelper.Send(nombreRepuesto);
                                    FileCommsHandler fileCommsHandler = new FileCommsHandler(socketClient);
                                    fileCommsHandler.SendFile(path);
                                    break;
                                case "5":
                                    // Console.WriteLine("5 - Consultar repuestos existentes");
                                    //CRF6 Consultar repuestos existentes.
                                    //El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                                    break;
                                case "6":
                                    // Console.WriteLine("6 - Consultar un repuesto específico");
                                    //CRF7 Consultar un repuesto específico.
                                    //El sistema deberá poder buscar un repuesto específico.
                                    //También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.
                                    break;
                                case "7":
                                    // Console.WriteLine("7 - Enviar y recibir mensajes");
                                    //CRF8 Enviar y recibir mensajes.
                                    //El sistema debe permitir que un mecánico envíe mensajes a otro,
                                    //y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.
                                    Console.WriteLine("Type a message a press enter to send it, write exit to exit");
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
                                            try
                                            {

                                                networkdatahelper.Send(cmd);
                                                networkdatahelper.Send(message);
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
                                case "8":
                                    // Console.WriteLine("8 - Salir");
                                    salir = true;
                                    break;
                            }
                            break;
                        }
                    default:
                        {
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
