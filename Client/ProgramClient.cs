using System;
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
            Console.WriteLine("Iniciando Aplicacion de Cliente...!");
            var socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), clientPort);

            socketClient.Bind(localEndPoint);
            Console.WriteLine("Iniciando Cliente");
            Console.WriteLine("Conectandose.......");

            // Si las credenciales no correctas me conecto al server sino que siga tratando
            socketClient.Connect(remoteEndPoint); // Me conecto al servidor
            Console.WriteLine("Conectado al Servidor!!!!");

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
                    string responseUsernamePassword = networkdatahelper.Receive();
                    userType = responseUsernamePassword;
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
                networkdatahelper.Send(cmd);
                switch (userType)
                {
                    case "admin":
                        {
                            switch (cmd)
                            {
                                case "1":
                                    //CRF1 Alta de usuario
                                    // Alta de usuario. Se debe poder dar de alta a un usuario (mecánico). 
                                    // Esta funcionalidad solo puede realizarse desde el usuario administrador.
                                    Console.WriteLine("Ingrese Usuario: ");
                                    string username = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese Contrasena: ");
                                    string password = Console.ReadLine() ?? string.Empty;
                                    string newUserRequest = username + ProtocolSpecification.fieldsSeparator + password;
                                    networkdatahelper.Send(newUserRequest);
                                    string newUserResponse = networkdatahelper.Receive();
                                    Console.WriteLine(newUserResponse);
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
                                    Console.WriteLine("id, nombre, proveedor y marca.");


                                    Console.WriteLine("Ingrese nombre: ");
                                    string nombre = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese Proveedor: ");
                                    string proveedor = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese Marca: ");
                                    string marca = Console.ReadLine() ?? string.Empty;

                                    //Chequeo que no exista el usuarion que quiero crear
                                    string message = nombre + ProtocolSpecification.fieldsSeparator + 
                                        proveedor + ProtocolSpecification.fieldsSeparator + 
                                        marca;
                                    networkdatahelper.Send(message);
                                    string altaRepuestoResponse = networkdatahelper.Receive();
                                    Console.WriteLine(altaRepuestoResponse);
                                    break;
                                case "2":
                                    // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                    //CRF3 Alta de Categoría de repuesto.
                                    //El sistema debe permitir crear una Categoría para los repuestos.
                                    Console.WriteLine("Ingrese nombre de la nueva categoria: ");
                                    string nombreCategoria = Console.ReadLine() ?? string.Empty;
                                    networkdatahelper.Send(nombreCategoria);
                                    string altaCateogriaResponse = networkdatahelper.Receive();
                                    Console.WriteLine(altaCateogriaResponse);
                                    break;
                                case "3":
                                    // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                    // CRF4 Asociar Categorías a los repuestos.
                                    // El sistema debe permitir asociar categorías a los repuestos.
                                    string nombresRepuestos = networkdatahelper.Receive();
                                    string nombresCategorias = networkdatahelper.Receive();
                                    List<string> listaNombresRepuestos = nombresRepuestos.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if(string.Equals("", nombresRepuestos))
                                    {
                                        Console.WriteLine("No hay ningun repuesto creado aun. ");
                                        networkdatahelper.Send("exit");
                                        break;
                                    }
                                    if (string.Equals("", nombresCategorias))
                                    {
                                        Console.WriteLine("No hay ninguna categoria creada aun. ");
                                        networkdatahelper.Send("exit");
                                        break;
                                    }
                                    Console.WriteLine("Repuestos: ");
                                    for (int i = 0; i < listaNombresRepuestos.Count; i++)
                                    {
                                        string nombreRepuestoIterado = listaNombresRepuestos[i];
                                        Console.WriteLine($"{i + 1} - {nombreRepuestoIterado}");
                                    }
                                    List<string> listaCategoriasRepuestos = nombresCategorias.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    Console.WriteLine("Categorias: ");
                                    for (int i = 0; i < listaCategoriasRepuestos.Count; i++)
                                    {
                                        string nombreCategoriaIterado = listaCategoriasRepuestos[i];
                                        Console.WriteLine($"{i + 1} - {nombreCategoriaIterado}");
                                    }
                                    Console.WriteLine("Ingrese nombre repuesto: ");
                                    string nombreRepuestoElegido = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese categoria: ");
                                    string nombreCategoriaElegida = Console.ReadLine() ?? string.Empty;
                                    string nombreRepuestoCategoria = nombreRepuestoElegido + ProtocolSpecification.fieldsSeparator +
                                        nombreCategoriaElegida;
                                    networkdatahelper.Send(nombreRepuestoCategoria);
                                    string asociarCateogoriaResponse = networkdatahelper.Receive();
                                    Console.WriteLine(asociarCateogoriaResponse);
                                    break;
                                case "4":
                                    // Console.WriteLine("4 - Asociar foto a repuesto");
                                    //CRF5 Asociar foto a repuesto.
                                    //El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                    string repuestosExistentesParaFoto = networkdatahelper.Receive();
                                    List<string> listaRepuestosExistentesParaFoto = repuestosExistentesParaFoto.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if (string.Equals("", repuestosExistentesParaFoto))
                                    {
                                        Console.WriteLine("No hay ningun repuesto creado aun. ");
                                        networkdatahelper.Send("exit");
                                        break;
                                    }


                                    FileHandler fileHandler = new FileHandler();
                                    Console.WriteLine("Por favor escribir el path del archivo a transferir");
                                    string path = Console.ReadLine() ?? string.Empty;
                                    bool cancelar = false;
                                    while ((string.IsNullOrEmpty(path) || path.Equals("exit", StringComparison.Ordinal) || !fileHandler.FileExists(path)) && !cancelar)
                                    {
                                        if (path.Equals("exit", StringComparison.Ordinal))
                                        {
                                            networkdatahelper.Send("exit");
                                            cancelar = true;
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
                                    if (cancelar)
                                    {
                                        break;
                                    }
                                    Console.WriteLine("Repuestos: ");
                                    for (int i = 0; i < listaRepuestosExistentesParaFoto.Count; i++)
                                    {
                                        string nombreRepuestoParaFotoIterado = listaRepuestosExistentesParaFoto[i];
                                        Console.WriteLine(nombreRepuestoParaFotoIterado);
                                    }
                                    Console.WriteLine("Escribir el nombre del repuesto para asociarle la foto");
                                    string nombreRepuesto = Console.ReadLine() ?? string.Empty;
                                    networkdatahelper.Send(nombreRepuesto);
                                    string responseNombreRepuesto = networkdatahelper.Receive();
                                    if(string.Equals(responseNombreRepuesto, "El repuesto no existe."))
                                    {
                                        Console.WriteLine(responseNombreRepuesto);
                                        break;
                                    }
                                    // me fijo si existia el repuesto
                                    FileCommsHandler fileCommsHandler = new FileCommsHandler(socketClient);
                                    fileCommsHandler.SendFile(path);
                                    string responseFotoAsociada = networkdatahelper.Receive();
                                    Console.WriteLine(responseFotoAsociada);
                                    break;
                                case "5":
                                    // Console.WriteLine("5 - Consultar repuestos existentes");
                                    //CRF6 Consultar repuestos existentes.
                                    //El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                                    Console.WriteLine("1 - Listar todos");
                                    Console.WriteLine("2 - Buscar por nombre repuesto");
                                    Console.WriteLine("3 - Buscar por nombre de la categoria");
                                    Console.WriteLine("4 - Buscar por nombre archivo foto");
                                    Console.WriteLine("5 - Buscar por nombre de proveedor");
                                    Console.WriteLine("6 - Buscar por nombre de marca");
                                    Console.WriteLine("Ingrese el numero de la opcion deseada");
                                    string opcionListado = Console.ReadLine() ?? string.Empty;
                                    networkdatahelper.Send(opcionListado);
                                    if(opcionListado != "1")
                                    {
                                        Console.WriteLine("Escribir el nombre: ");
                                        string nombreABuscar = Console.ReadLine() ?? string.Empty;
                                        networkdatahelper.Send(nombreABuscar);
                                    }
                                    string repuestosExistentes = networkdatahelper.Receive();
                                    if (string.Equals(repuestosExistentes, ""))
                                    {
                                        Console.WriteLine("No hay repuestos para mostrar. ");
                                        break;
                                    }
                                    List<string> listaRepuestosExistentes = repuestosExistentes.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    Console.WriteLine("Repuestos: ");
                                    for (int i = 0; i < listaRepuestosExistentes.Count; i++)
                                    {
                                        string nombreCategoriaIterado = listaRepuestosExistentes[i];
                                        Console.WriteLine(nombreCategoriaIterado);
                                    }
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
                                    Console.WriteLine("Escriba un mensaje y presione enter para enviarlo, escriba exit para salir");
                                    bool salirCRF8 = false;
                                    while (!salirCRF8 && !salir)
                                    {
                                        var message1 = Console.ReadLine();
                                        if (string.IsNullOrEmpty(message1) || message1.Equals("exit", StringComparison.Ordinal))
                                        {
                                            salirCRF8 = true;
                                        }
                                        else
                                        {
                                            try
                                            {

                                                networkdatahelper.Send(cmd);
                                                networkdatahelper.Send(message1);
                                            }
                                            catch (SocketException)
                                            {
                                                Console.WriteLine("Perdi la conexion con el server");
                                                salir = true;

                                            }

                                        }
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
            Console.WriteLine("Se Cerrara la Conexion....");
            socketClient.Shutdown(SocketShutdown.Both); // Desconecto ambos sentidos de la connecion
            socketClient.Close();
            socketClient.Dispose();
        }
    }
}
