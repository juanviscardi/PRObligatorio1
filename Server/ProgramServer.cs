﻿using Common;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMng = new SettingsManager();
        //static Dictionary<string, string> usuarios = new();
        static List<Usuario> usuarios = new();
        static Dictionary<string, string[]> mensajes = new();
        static List<Repuesto> repuestos = new();
        static List<string> categorias = new();

        private static readonly Object _agregarUsuario= new Object();
        private static readonly Object _agregarRepuesto = new Object();
        private static readonly Object _agregarCategoria = new Object();
        private static readonly Object _asociarCategoria = new Object();

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
            string userType = "error";
            string usernameConnected = "";
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(socketClient);
            //const int largoDataLength = 4;  // Defino la constante del largo ******* HAY QUE DEFINIRLO MAS PROLLIJO EN OTRO LADO
            // ******************************************************************************

            while (clientIsConnected)
            {
                // byte[] dataLength = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                // byte[] dataLength = networkdatahelper.Receive(largoDataLength);
                // byte[] data = networkdatahelper.Receive(BitConverter.ToInt32(dataLength));
                // string message = Encoding.UTF8.GetString(data);
                // string[] messageConTodo = message.Split(ProtocolSpecification.fieldsSeparator);

                //Console.WriteLine("Mensaje Recibido: {0}", message);
                //string[] data8 = message.Split(ProtocolSpecification.fieldsSeparator);
                // Feo pero funciona - Algo para saber quien soy
                string algo = socketClient.RemoteEndPoint.ToString() ?? string.Empty;

                string[] datos = algo.Split(":"); //IPAddress : Puerto
                try
                {
                    // Console.WriteLine("Mensaje Recibido: {0} desde {1} en el puerto {2} \n", message, datos[0], datos[1]);
                    if (string.Equals(userType, "error"))
                {
                        // log in
                        byte[] lengthUsuarioContrasena = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                        byte[] dataUsuarioContrasena = networkdatahelper.Receive(BitConverter.ToInt32(lengthUsuarioContrasena));
                        string messageUsuarioContrasena = Encoding.UTF8.GetString(dataUsuarioContrasena);
                        string[] usuarioContrasena = messageUsuarioContrasena.Split(ProtocolSpecification.fieldsSeparator);
                        string usuario = usuarioContrasena[0];
                        string pass = usuarioContrasena[1];
                        string adminUsername = settingsMng.ReadSettings(ServerConfig.usernameConfigKey);
                        string adminPassword = settingsMng.ReadSettings(ServerConfig.passwordConfigKey);
                        if (string.Equals(usuario, adminUsername) && string.Equals(pass, adminPassword))
                        {
                            usernameConnected = usuario;
                            userType = "admin";
                            Console.WriteLine("Usuario: {0} hizo login como admin", usuario);
                            networkdatahelper.Send("admin");
                        }
                        else
                        {
                            bool existeYEsValido = false;
                            usuarios.ToList().ForEach(x => {
                                if (string.Equals(x.userName, usuario) && string.Equals(x.userPassword, pass))
                                {
                                    existeYEsValido = true;
                                    usernameConnected = usuario;
                                }
                            });
                            if (existeYEsValido)
                            {
                                userType = "mecanico";
                                Console.WriteLine("Usuario: {0} hizo login con mecanico", usuario);
                                networkdatahelper.Send("mecanico");
                            }
                            else
                            {
                                Console.WriteLine("Usuario: {0} no se conecto", usuario);
                                networkdatahelper.Send("error");
                            }
                        }
                        continue;
                    }
                    byte[] dataLength0 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                    byte[] data0 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength0));
                    string cmd = Encoding.UTF8.GetString(data0);
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

                                        Console.WriteLine("Alta de usuario. Se debe poder dar de alta a un usuario (mecánico)."); 
                                        Console.WriteLine("Estafuncionalidad solo puede realizarse desde el usuario admin.");
                                        Console.WriteLine("Console.WriteLine(1 - Anadir usuario");
                                        
                                        byte[] dataLength1 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                                        byte[] data1 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength1));
                                        string message1 = Encoding.UTF8.GetString(data1);
                                        string[] messageConTodo = message1.Split(ProtocolSpecification.fieldsSeparator);
                                        var userName = messageConTodo[0];
                                        var userPassword = messageConTodo[1];

                                        Usuario user = new Usuario(
                                                           userName,
                                                           userPassword,
                                                           "mecanico");

                                        
                                        lock (_agregarUsuario)
                                        {
                                            if (!usuarios.Contains(user))
                                            {
                                                //Aca hay que hacer lock

                                                usuarios.Add(user);
                                                networkdatahelper.Send("exito");
                                            }
                                            else 
                                            {
                                                networkdatahelper.Send("el usuario ya existe");
                                            }

                                        }




                                        break;
                                    case "2":
                                        // Console.WriteLine("2 - Configuracion");
                                        clientIsConnected = true;
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
                                        byte[] dataLength2 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                                        byte[] data2 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength2));
                                        string message2 = Encoding.UTF8.GetString(data2);
                                        string[] messageConTodo = message2.Split(ProtocolSpecification.fieldsSeparator);
                                        var repuestoName = messageConTodo[0];
                                        var repuestoProveedor = messageConTodo[1];
                                        var repuestoMarca = messageConTodo[2];

                                        Repuesto repu = new Repuesto(
                                                           repuestos.Count().ToString(),
                                                           repuestoName,
                                                           repuestoProveedor,
                                                           repuestoMarca); ;


                                        lock (_agregarRepuesto)
                                        {
                                            if (!repuestos.Contains(repu))
                                            {
                                                //Aca hay que hacer lock

                                                repuestos.Add(repu);
                                                networkdatahelper.Send("exito");
                                            }
                                            else
                                            {
                                                networkdatahelper.Send("el repuesto ya existe");
                                            }

                                        }

                                       
                                        break;
                                    case "2":
                                        // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                        break;
                                    case "3":
                                        // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                        break;
                                    case "4":
                                        // Console.WriteLine("4 - Asociar foto a repuesto");
                                        byte[] dataLength5 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                                        byte[] data5 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength5));
                                        string nombreRepuesto = Encoding.UTF8.GetString(data5);
                                        FileCommsHandler fileCommsHandler = new FileCommsHandler(socketClient);
                                        string nombreArchivo = fileCommsHandler.ReceiveFile();
                                        // el archivo queda guardado en el bin
                                        Console.WriteLine("Nombre Repuesto: {0}, Nombre Archivo: {1}", nombreRepuesto, nombreArchivo);
                                        // falta revisar que el repuesto exista, y agregar el path al repuesto en el atributo foto, ver si queremos agregar mas de una foto o no

                                        break;
                                    case "5":
                                        // Console.WriteLine("5 - Consultar repuestos existentes");
                                        Console.WriteLine("TODO");
                                        repuestos.ToList().ForEach(x => Console.WriteLine(x));
                                        break;
                                    case "6":
                                        // Console.WriteLine("6 - Consultar un repuesto específico");
                                        break;
                                    case "7":
                                        // Console.WriteLine("7 - Enviar y recibir mensajes");
                                        byte[] dataLength8 = networkdatahelper.Receive(ProtocolSpecification.fixedLength);
                                        byte[] data8 = networkdatahelper.Receive(BitConverter.ToInt32(dataLength8));
                                        string message8 = Encoding.UTF8.GetString(data8);
                                        mensajes.Add(message8, datos);
                                        Console.WriteLine("Mensaje Recibido: {0} desde {1} en el puerto {2} \n", message8, datos[0], datos[1]);
                                        break;
                                    case "8":
                                        // Console.WriteLine("8 - Salir");
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
                catch (SocketException ex)
                {
                    //*************************************************
                    // ******* PONER UN MENSAJE ACORDE
                    // Usando el valor de ex tal vez

                    Console.WriteLine("ERROR:" + "Client disconnected a lo bruto \n" + socketClient.RemoteEndPoint + "\n");
                    

                          // Hay que pasar a false la propiedad del usuario que se desconecto

                    // No se si esto vale!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    

                    // ESTE ERROR ME LO COMO (NO LO MUESTRO)
                    // O LO TIRO A UNA BASE DE EVENTOS/ERRORES
                    // **********************************************

                    clientIsConnected = false;
                }

            }
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();
            socketClient.Dispose();
            Console.WriteLine("Client disconnected");
            //Console.WriteLine(socketClient.RemoteEndPoint);
        }
    }
}
