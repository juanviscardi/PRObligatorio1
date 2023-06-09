﻿using System.Net.Sockets;
using System.Net;
using Common;
using System.Text;

namespace ClientApp
{
    public class ProgramClient
    {
        static readonly SettingsManager settingsMng = new SettingsManager();

        //public static void Main(string[] args)
        static async Task Main(string[] args)
        {
 
            //Sustituyo ip y port por los valores del archivo
            string serverIp = settingsMng.ReadSettings(ClientConfig.serverIPConfigKey);
            string clientIp = settingsMng.ReadSettings(ClientConfig.ClientIPConfigKey);
            int serverPort = int.Parse(settingsMng.ReadSettings(ClientConfig.serverPortconfigKey));
            int clientPort = int.Parse(settingsMng.ReadSettings(ClientConfig.ClientPortconfigKey));

            //endPoint hacia server
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            //endPoint haciua el cliente
            var localEndPoint = new IPEndPoint(IPAddress.Parse(clientIp), clientPort);

            var tcpClient = new TcpClient(localEndPoint);

            Console.WriteLine("Iniciando Aplicacion de Cliente...!");

            //Console.WriteLine("Iniciando Cliente, Conectandose.......");

            //tcpClient.Connect(remoteEndPoint);

            await tcpClient.ConnectAsync(remoteEndPoint);

            //NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(socketClient);
            NetworkDataHelper networkdatahelper = new Common.NetworkDataHelper(tcpClient);
            
            //Inicializo cmd en 0 que es la opcion de login en el servidor
            string userType = "error";
            string userConnected = "nadie";
            // Si las credenciales son correctas me conecto al server sino que siga tratando
            Console.WriteLine("Conectado al Servidor!!!!");

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

                    // Convierto de string a un array de bytes
                    /*
                    byte[] dataUsernamePassword = Encoding.UTF8.GetBytes(usernamePassword);
                    int datalength = dataUsernamePassword.Length;
                    byte[] dataLength = BitConverter.GetBytes(datalength);
                    */
                    await networkdatahelper.Send(usernamePassword);
                    userType = await networkdatahelper.Receive();
                    //await networkdatahelper.Receive(datalength);// (datalength);


                    //userType = responseUsernamePassword;
                    //userType = "error";

                    if (string.Equals(userType, "error"))
                    {
                        Console.WriteLine("El usuario o contrasena no es correcto");
                    }
                    else
                    {
                        userConnected = username;
                    }
                    
                }
                catch
                {
                    Console.WriteLine("Perdi la conexion con el server");
                    tcpClient.Close();

                }
            }
            bool salir = false;
            while (!salir)
            {
                ConsoleClientMenu.GetMenu(userType, userConnected);
                string cmd = Console.ReadLine() ?? string.Empty;
                await networkdatahelper.Send(cmd);
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
                                    await networkdatahelper.Send(newUserRequest);
                                    string newUserResponse = await networkdatahelper.Receive();
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
                                    //Console.WriteLine("CRF2 Alta de repuesto.");
                                    //Console.WriteLine("Se debe poder dar de alta a un repuesto en el sistema, incluyendo");
                                    //Console.WriteLine("id, nombre, proveedor y marca.");


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
                                    await networkdatahelper.Send(message);
                                    string altaRepuestoResponse = await networkdatahelper.Receive();
                                    Console.WriteLine(altaRepuestoResponse);
                                    break;
                                case "2":
                                    // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                    //CRF3 Alta de Categoría de repuesto.
                                    //El sistema debe permitir crear una Categoría para los repuestos.
                                    Console.WriteLine("Ingrese nombre de la nueva categoria: ");
                                    string nombreCategoria = Console.ReadLine() ?? string.Empty;
                                    await networkdatahelper.Send(nombreCategoria);
                                    string altaCateogriaResponse = await networkdatahelper.Receive();
                                    Console.WriteLine(altaCateogriaResponse);
                                    break;
                                case "3":
                                    // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                    // CRF4 Asociar Categorías a los repuestos.
                                    // El sistema debe permitir asociar categorías a los repuestos.
                                    string nombresRepuestos = await networkdatahelper.Receive();
                                    string nombresCategorias = await networkdatahelper.Receive();
                                    List<string> listaNombresRepuestos = nombresRepuestos.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if (string.Equals("", nombresRepuestos))
                                    {
                                        Console.WriteLine("No hay ningun repuesto creado aun. ");
                                        await networkdatahelper.Send("exit");
                                        break;
                                    }
                                    if (string.Equals("", nombresCategorias))
                                    {
                                        Console.WriteLine("No hay ninguna categoria creada aun. ");
                                        await networkdatahelper.Send("exit");
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
                                    await networkdatahelper.Send(nombreRepuestoCategoria);
                                    string asociarCateogoriaResponse = await networkdatahelper.Receive();
                                    Console.WriteLine(asociarCateogoriaResponse);
                                    break;
                               case "4":
                                    // Console.WriteLine("4 - Asociar foto a repuesto");
                                    //CRF5 Asociar foto a repuesto.
                                    //El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                    string repuestosExistentesParaFoto = await networkdatahelper.Receive();
                                    List<string> listaRepuestosExistentesParaFoto = repuestosExistentesParaFoto.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if (string.Equals("", repuestosExistentesParaFoto))
                                    {
                                        Console.WriteLine("No hay ningun repuesto creado aun. ");
                                        await networkdatahelper.Send("exit");
                                        break;
                                    }


                                    FileHandler fileHandler = new FileHandler();
                                    Console.WriteLine("Por favor escribir el path del archivo a transferir");
                                    string path = Console.ReadLine() ?? string.Empty;
                                    bool cancelar = false;
                                    bool fileExists = await fileHandler.FileExists(path);
                                    while ((string.IsNullOrEmpty(path) || path.Equals("exit", StringComparison.Ordinal) || !fileExists) && !cancelar)
                                    {
                                        if (path.Equals("exit", StringComparison.Ordinal))
                                        {
                                            await networkdatahelper.Send("exit");
                                            cancelar = true;
                                            break;
                                        }
                                        if (string.IsNullOrEmpty(path))
                                        {
                                            Console.WriteLine("El path no puede ser vacio, escriba uno o escriba exit para salir");
                                            path = Console.ReadLine() ?? string.Empty;
                                            continue;
                                        }
                                        if (!fileExists)
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
                                    await networkdatahelper.Send(nombreRepuesto);
                                    string responseNombreRepuesto = await networkdatahelper.Receive();
                                    if (string.Equals(responseNombreRepuesto, "El repuesto no existe."))
                                    {
                                        Console.WriteLine(responseNombreRepuesto);
                                        break;
                                    }
                                    // me fijo si existia el repuesto
                                    FileCommsHandler fileCommsHandler = new FileCommsHandler(tcpClient);
                                    await fileCommsHandler.SendFile(path);
                                    string responseFotoAsociada = await networkdatahelper.Receive();
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
                                    await networkdatahelper.Send(opcionListado);
                                    if (opcionListado != "1")
                                    {
                                        Console.WriteLine("Escribir el nombre: ");
                                        string nombreABuscar = Console.ReadLine() ?? string.Empty;
                                        await networkdatahelper.Send(nombreABuscar);
                                    }
                                    string repuestosExistentes = await networkdatahelper.Receive();
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

                                    //CRF7 Consultar un repuesto específico.
                                    //El sistema deberá poder buscar un repuesto específico.
                                    //También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.

                                    string nombreRepuestosExistentes = await networkdatahelper.Receive();
                                    List<string> listaNombreRepuestosExistentes = nombreRepuestosExistentes.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if (string.Equals("", listaNombreRepuestosExistentes))
                                    {
                                        Console.WriteLine("No hay ningun repuesto creado aun. ");
                                        break;
                                    }
                                    Console.WriteLine("Repuestos: ");
                                    for (int i = 0; i < listaNombreRepuestosExistentes.Count; i++)
                                    {
                                        string nombreRepuestoIterado = listaNombreRepuestosExistentes[i];
                                        Console.WriteLine($"{i} - {nombreRepuestoIterado}");
                                    }
                                    Console.WriteLine("Ingrese el numero del repuesto para mas detalles: ");
                                    string opcionDetalleSeleccionada = Console.ReadLine() ?? string.Empty;
                                    int number;
                                    bool isNumber = int.TryParse(opcionDetalleSeleccionada, out number);
                                    if (!isNumber || listaNombreRepuestosExistentes.Count <= number || number < 0)
                                    {
                                        Console.WriteLine("La opcion ingresada no es valida");
                                        await networkdatahelper.Send("exit");
                                        break;
                                    }
                                    await networkdatahelper.Send(listaNombreRepuestosExistentes[int.Parse(opcionDetalleSeleccionada)]);
                                    string detallesOError = await networkdatahelper.Receive();
                                    if (string.Equals(detallesOError, "El repuesto no existe."))
                                    {
                                        Console.WriteLine(detallesOError);
                                        break;
                                    }
                                    List<string> responseRepuestoYFoto = detallesOError.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    string detalle = responseRepuestoYFoto[0];
                                    bool tieneFoto = bool.Parse(responseRepuestoYFoto[1]);
                                    Console.WriteLine(detalle);
                                    if (!tieneFoto) break;
                                    Console.WriteLine("Desea descargar la foto?");
                                    Console.WriteLine("1 - Si");
                                    Console.WriteLine("2 - No");
                                    string opcionDetalleSeleccionadaFoto = Console.ReadLine() ?? string.Empty;
                                    int numberFoto;
                                    bool isNumberFoto = int.TryParse(opcionDetalleSeleccionadaFoto, out numberFoto);
                                    if (!isNumberFoto || numberFoto > 1 || numberFoto <= 0)
                                    {
                                        await networkdatahelper.Send("NO");
                                        break;
                                    }
                                    await networkdatahelper.Send("SI");
                                    FileCommsHandler fileCommsHandler2 = new FileCommsHandler(tcpClient);
                                    string nombreArchivo = await fileCommsHandler2.ReceiveFile();
                                    Console.WriteLine("La imagen {0} fue descargada", nombreArchivo);
                                    break;
                                case "7":
                                    // Console.WriteLine("7 - Enviar y recibir mensajes");
                                    //CRF8 Enviar y recibir mensajes.
                                    //El sistema debe permitir que un mecánico envíe mensajes a otro,
                                    //y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.

                                    string nombresMecanicos = await networkdatahelper.Receive();
                                    List<string> listaNombresMecanicos = nombresMecanicos.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    if (listaNombresMecanicos.Count < 2)
                                    {
                                        Console.WriteLine("No hay ningun mecanico registrado aun. ");
                                        await networkdatahelper.Send("exit");
                                        break;
                                    }
                                    Console.WriteLine("Mecanicos: ");
                                    for (int i = 0; i < listaNombresMecanicos.Count; i++)
                                    {
                                        if (string.Equals(listaNombresMecanicos[i],userConnected)) continue;
                                        string nombreMecanicoIterado = listaNombresMecanicos[i];
                                        Console.WriteLine($"- {nombreMecanicoIterado}");
                                    }
                                    Console.WriteLine("Ingrese nombre del mecanico a enviarle el mensaje: ");
                                    string nombreMecanicoElegido = Console.ReadLine() ?? string.Empty;
                                    Console.WriteLine("Ingrese mensaje a enviar: ");
                                    string textoMensaje = Console.ReadLine() ?? string.Empty;
                                    Mensaje nuevoMensaje = new Mensaje(userConnected, nombreMecanicoElegido, textoMensaje);
                                    await networkdatahelper.Send(nuevoMensaje.ToString());
                                    string respuestaEnviarMensaje = await networkdatahelper.Receive();
                                    Console.WriteLine(respuestaEnviarMensaje);
                                    break;
                                case "8":
                                    // Console.WriteLine("8 - Recibir mensajes");
                                    Console.WriteLine("Ingrese la opcion deseada: ");
                                    Console.WriteLine("1 - Leer nuevos mensajes");
                                    Console.WriteLine("2 - Ver historial mensajes");
                                    string opcionLeer = Console.ReadLine() ?? string.Empty;
                                    int numberOpcionLeer;
                                    bool isNumberOpcionLeer = int.TryParse(opcionLeer, out numberOpcionLeer);
                                    if (!isNumberOpcionLeer || 3 <= numberOpcionLeer || numberOpcionLeer <= 0)
                                    {
                                        Console.WriteLine("La opcion ingresada no es valida");
                                        await networkdatahelper.Send("exit");
                                        break;
                                    }
                                    await networkdatahelper.Send(opcionLeer);
                                    string mensajesAImprimir = await networkdatahelper.Receive();
                                    if (string.Equals("", mensajesAImprimir))
                                    {
                                        Console.WriteLine(string.Equals(opcionLeer,"1") ? "No hay mensajes nuevos. " : "No hay mensajes en el historial");
                                        break;
                                    }
                                    List<string> listaMensajesAImprimir = mensajesAImprimir.Split(ProtocolSpecification.fieldsSeparator).ToList();
                                    for (int i = 0; i < listaMensajesAImprimir.Count/5; i++)
                                    {
                                        Mensaje mensajeIterado = new Mensaje(
                                            listaMensajesAImprimir[i*5],
                                            listaMensajesAImprimir[i*5 + 1],
                                            listaMensajesAImprimir[i*5 + 2],
                                            DateTime.Parse(listaMensajesAImprimir[i*5 + 3]),
                                            bool.Parse(listaMensajesAImprimir[i*5 + 4])
                                        );
                                        mensajeIterado.ImprimirCliente();
                                        Console.WriteLine($"-------------------------");
                                    }
                                    break;
                                case "9":
                                    // Console.WriteLine("9 - Salir");
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
            //CierraConexionCliente(tcpClient);

            /*static void CierraConexionCliente(Socket socketClient)
            {
                socketClient.Shutdown(SocketShutdown.Both); // Desconecto ambos sentidos de la connecion
                socketClient.Close();
                socketClient.Dispose();
            }*/

             

        }
    }
}
