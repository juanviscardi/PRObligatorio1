using Common;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMng = new SettingsManager();
        static List<Usuario> usuarios = new();
        static List<Repuesto> repuestos = new();
        static List<string> categorias = new();
        static List<Mensaje> mensajes = new();

        private static readonly Object _agregarUsuario= new Object();
        private static readonly Object _agregarRepuesto = new Object();
        private static readonly Object _agregarCategoria = new Object();
        private static readonly Object _asociarCategoria = new Object();

        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Aplicacion de Servidor.....!!!");

            var socketServer = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            //Sustituimos ip y port por los valores del archivo
            string serverIp = settingsMng.ReadSettings(ServerConfig.serverIPConfigKey);
            int serverPort = int.Parse(settingsMng.ReadSettings(ServerConfig.serverPortconfigKey));

            //var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);
            var localEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            Console.WriteLine("Servidor inicializado con IP {0} y Puerto {1}", serverIp, serverPort);

            // Asociamos el socket con el endpoint
            socketServer.Bind(localEndPoint);

            // Nuestro Socket pasa a estar en modo escucha,
            // el fixedBackpack me dice que puedo manejar la espera de varios a la vez, es el backlog,
            // la cola de conexiones que aceptara
            socketServer.Listen(ProtocolSpecification.FixedBackpack); 

            int clientes = 0;
            bool salir = false;

            Console.WriteLine("Esperando por Clientes.....\n");

            while (!salir)
            {
                Socket socketClient = socketServer.Accept();
                // El Accept es bloqueante
                // Espera a que llegue una nueva conexion,
                // por eso fue el fixedBackpack en el listen sino bloquea y no podra hacer nada mas.
                clientes++;
                int nro = clientes;

                Console.WriteLine("Acepte un nuevo pedido de conexion");

                //Feo pero funciona - Algo para saber quien soy
                string algo = socketClient.RemoteEndPoint.ToString()
                                  ?? string.Empty;

                string[] datos = algo.Split(ProtocolSpecification.valuesSeparator);
                Console.WriteLine("Se conecto {0} en el puerto {1} \n", datos[0], datos[1]);

                new Thread(() => HandleClient(socketClient)).Start();
                // Lanzamos un nuevo hilo para manejar al nuevo cliente

            }
            // Cierro el socket
            //socketServer.Shutdown(SocketShutdown.Both);
            //socketServer.Close();
            //socketServer.Dispose();
        }

        static void HandleClient(Socket socketClient)
        {

            bool clientIsConnected = true;
            string userType = "error";
            string usernameConnected = "";
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(socketClient);
            
            while (clientIsConnected)
            {
                string ipAddressPuerto = socketClient.RemoteEndPoint.ToString() ?? string.Empty;
                string[] datos = ipAddressPuerto.Split(":"); //IPAddress : Puerto
                try
                {
                    if (string.Equals(userType, "error"))
                {
                        // log in
                        string messageUsuarioContrasena = networkdatahelper.Receive();
                        string[] usuarioContrasena = messageUsuarioContrasena.Split(ProtocolSpecification.fieldsSeparator);
                        string usuario = usuarioContrasena[0];
                        string pass = usuarioContrasena[1];
                        string adminUsername = settingsMng.ReadSettings(ServerConfig.usernameConfigKey);
                        string adminPassword = settingsMng.ReadSettings(ServerConfig.passwordConfigKey);
                        if (string.Equals(usuario, adminUsername) && string.Equals(pass, adminPassword))
                        {
                            usernameConnected = usuario;
                            userType = "admin";
                            Console.WriteLine("Usuario: {0} hizo login como Administrador", usuario);
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
                                Console.WriteLine("Usuario: {0} hizo login como Mecanico", usuario);
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
                    string cmd = networkdatahelper.Receive();
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

                                        string altaUsuarioRequest = networkdatahelper.Receive();
                                        string[] altaUsuarioRequestConTodo = altaUsuarioRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var userName = altaUsuarioRequestConTodo[0];
                                        var userPassword = altaUsuarioRequestConTodo[1];

                                        Usuario user = new Usuario(
                                                           userName,
                                                           userPassword,
                                                           "mecanico");

                                        
                                        lock (_agregarUsuario)
                                        {
                                            if (!usuarios.Contains(user))
                                            {
                                                usuarios.Add(user);
                                                networkdatahelper.Send("exito");
                                                Console.WriteLine("Se Creo un nuevo usuario");
                                                Console.WriteLine(user.ToString());
                                                Console.WriteLine();
                                            }
                                            else 
                                            {
                                                networkdatahelper.Send("el usuario ya existe");
                                            }

                                        }
                                        break;
                                    case "2":
                                        // Salir;
                                        clientIsConnected = false;
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
                                        string altaRepuestoRequest = networkdatahelper.Receive();
                                        string[] altaRepuestoRequestConTodo = altaRepuestoRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var repuestoName = altaRepuestoRequestConTodo[0];
                                        var repuestoProveedor = altaRepuestoRequestConTodo[1];
                                        var repuestoMarca = altaRepuestoRequestConTodo[2];
                                        Repuesto repu = new Repuesto(
                                                           repuestos.Count().ToString(),
                                                           repuestoName,
                                                           repuestoProveedor,
                                                           repuestoMarca);
                                        lock (_agregarRepuesto)
                                        {
                                            if (!repuestos.Contains(repu))
                                            {
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
                                        // SRF3.Crear Categoría de repuesto. El sistema debe permitir crear una categoría para los repuestos.
                                        // CRF3. Alta de Categoría de repuesto. El sistema debe permitir crear una Categoría para los repuestos.
                                        string categoria = networkdatahelper.Receive();
                                        lock (_agregarCategoria)
                                        {
                                            if (!categorias.Contains(categoria))
                                            {
                                                categorias.Add(categoria);
                                                networkdatahelper.Send("exito");
                                            }
                                            else
                                            {
                                                networkdatahelper.Send("la categoria ya existe");
                                            }

                                        }

                                        break;
                                    case "3":
                                        //SRF4. Asociar Categorías a un repuesto. El sistema debe permitir asociar categorías a los repuestos.
                                        //CRF4. Asociar Categorías a los repuestos. El sistema debe permitir asociar categorías a losrepuestos.
                                        // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                        // envio nombre de repuestos existentes para que se listen en el cliente
                                        List<string> repuestosExistentesNamesResponse = new List<string>();
                                        repuestos.ToList().ForEach(x => {
                                            repuestosExistentesNamesResponse.Add(x.Name);
                                        });
                                        networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesNamesResponse));
                                        // envio nombre de categorias existentes para que se listen en el cliente
                                        networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, categorias));
                                        // recivo el nombre del repuesto elegio y la categoria elegida
                                        string asociarCategoriaRequest = networkdatahelper.Receive();
                                        if(string.Equals(asociarCategoriaRequest, "exit"))
                                        {
                                            break;
                                        }
                                        string[] asociarCategoriaRequestConTodo = asociarCategoriaRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        string repuestoName4 = asociarCategoriaRequestConTodo[0];
                                        string categoria4 = asociarCategoriaRequestConTodo[1];
                                        lock (_asociarCategoria)
                                        {
                                            Repuesto repuesto4 = repuestos.Find(x => string.Equals(repuestoName4, x.Name));
                                            if (repuesto4 != null)
                                            {
                                                if (!repuesto4.Categorias.Contains(categoria4)) // Verifica si la categoría ya está presente en el Repuesto
                                                {
                                                    if(!categorias.Contains(categoria4))
                                                    {
                                                        networkdatahelper.Send("La categoría no existe");
                                                        break;
                                                    }
                                                    repuesto4.Categorias.Add(categoria4); // Agrega la nueva categoría al Repuesto
                                                    networkdatahelper.Send("exito");
                                                }
                                                else
                                                {
                                                    networkdatahelper.Send("La categoría ya está presente en el Repuesto.");
                                                }
                                            }
                                            else
                                            {
                                                networkdatahelper.Send("El repuesto no existe.");
                                            }
                                        }
                                        break;
                                    case "4":
                                        //SRF5 Asociar una foto al repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                        //CRF5. Asociar foto a repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                        
                                        List<string> repuestosExistentesParaFotoResponse = new List<string>();
                                        repuestos.ToList().ForEach(x => {
                                            repuestosExistentesParaFotoResponse.Add(x.Name);
                                        });
                                        networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesParaFotoResponse));
                                        string nombreRepuestoOExit = networkdatahelper.Receive();
                                        if(string.Equals(nombreRepuestoOExit,"exit"))
                                        {
                                            break;
                                        }
                                        Repuesto repuesto5 = repuestos.Find(x => string.Equals(nombreRepuestoOExit, x.Name));
                                        if (repuesto5 != null)
                                        {
                                            networkdatahelper.Send("El repuesto existe.");
                                        }
                                        else
                                        {
                                            networkdatahelper.Send("El repuesto no existe.");
                                            break;
                                        }
                                        FileCommsHandler fileCommsHandler = new FileCommsHandler(socketClient);
                                        string nombreArchivo = fileCommsHandler.ReceiveFile();
                                        // el archivo queda guardado en el bin
                                        repuesto5.Foto = nombreArchivo;
                                        networkdatahelper.Send("Se asocio la foto al repuesto.");

                                        break;
                                    case "5":
                                        // SRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes,incluyendo búsquedas por palabras claves.
                                        // CRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                                        
                                        string opcionListado = networkdatahelper.Receive();
                                        List<string> repuestosExistentesResponse = new List<string>();
                                        switch (opcionListado)
                                        {
                                            case "1":
                                                // Console.WriteLine("1 - Listar todos");
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    repuestosExistentesResponse.Add(x.ToStringListar());
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "2":
                                                // Console.WriteLine("2 - Buscar por nombre repuesto");
                                                string opcionListadoNombre = networkdatahelper.Receive();
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Name, opcionListadoNombre))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "3":
                                                // Console.WriteLine("3 - Buscar por categoria");
                                                string opcionListadoCategoria = networkdatahelper.Receive();
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    if (x.Categorias.Contains(opcionListadoCategoria))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "4":
                                                // Console.WriteLine("4 - Buscar por nombre archivo foto");
                                                string opcionListadoFoto = networkdatahelper.Receive();
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Foto, opcionListadoFoto))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "5":
                                                // Console.WriteLine("5 - Buscar por nombre de proveedor");
                                                string opcionListadoProveedor = networkdatahelper.Receive();
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Proveedor, opcionListadoProveedor))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "6":
                                                // Console.WriteLine("6 - Buscar por nombre de marca")
                                                string opcionListadoMarca = networkdatahelper.Receive();
                                                repuestos.ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Marca, opcionListadoMarca))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                        }
                                        break;
                                    case "6":
                                        // SRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.
                                        
                                        // CRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.

                                        List<string> nombreRepuestosExistentes = new List<string>();
                                        repuestos.ToList().ForEach(x => {
                                            nombreRepuestosExistentes.Add(x.Name);
                                        });
                                        networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreRepuestosExistentes));
                                        string nombreRepuestoQuieroDetalles = networkdatahelper.Receive();
                                        if(string.Equals(nombreRepuestoQuieroDetalles, "exit"))
                                        {
                                            break;
                                        }
                                        Repuesto repuesto6 = repuestos.Find(x => string.Equals(nombreRepuestoQuieroDetalles, x.Name));
                                        if (repuesto6 != null)
                                        {
                                            bool tieneFoto = true;
                                            if(string.IsNullOrEmpty(repuesto6.Foto))
                                            {
                                                tieneFoto = false;
                                            }
                                            string response = repuesto6.ToStringListar() + ProtocolSpecification.fieldsSeparator + tieneFoto;
                                            networkdatahelper.Send(response);
                                        }
                                        else
                                        {
                                            networkdatahelper.Send("El repuesto no existe.");
                                        }
                                        string enviarFoto = networkdatahelper.Receive();
                                        if (string.Equals(enviarFoto, "NO")) break;
                                        FileCommsHandler fileCommsHandler2 = new FileCommsHandler(socketClient);
                                        fileCommsHandler2.SendFile(repuesto6.Foto);
                                        break;
                                    case "7":

                                        //SRF8. Enviar y recibir mensajes entre mecánicos. El sistema debe permitir que un mecánico
                                        //envíe mensajes a otro, y que el mecánico receptor chequee sus mensajes sin leer, así como
                                        //también revisar su historial de mensajes.

                                        // mandar todos los mecanicos
                                        List<string> nombreMecanicos = new List<string>();
                                        usuarios.ToList().ForEach(x => {
                                            nombreMecanicos.Add(x.userName);
                                        });
                                        networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreMecanicos));
                                        string requestEnviarMensaje = networkdatahelper.Receive();
                                        // validar inputs
                                        if (string.Equals(requestEnviarMensaje, "exit")) break;
                                        Mensaje mensajeEnviado = new Mensaje(requestEnviarMensaje);
                                        if(string.Equals(mensajeEnviado.remitente, mensajeEnviado.destinatario))
                                        {
                                            networkdatahelper.Send("No se puede enviar un mensaje a si mismo");
                                            break;
                                        }
                                        Usuario usuarioDestintario = usuarios.Find(x => string.Equals(mensajeEnviado.destinatario, x.userName));
                                        if (usuarioDestintario == null)
                                        {
                                            networkdatahelper.Send("El destinatario no es un mecanica valido.");
                                            break;
                                        }
                                        if(string.IsNullOrEmpty(mensajeEnviado.cuerpoMensaje))
                                        {
                                            networkdatahelper.Send("El cuerpo del mensaje no puede estar vacio.");
                                            break;
                                        }
                                        // enviar respuesta a imprimir en el cliente
                                        mensajes.Add(mensajeEnviado);
                                        mensajeEnviado.ImprimirServer();
                                        networkdatahelper.Send("Mensaje enviado. ");
                                        // CRF8.Enviar y recibir mensajes. El sistema debe permitir que un mecánico envíe mensajes
                                        // a otro, y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.

                                        break;
                                    case "8":
                                        // Console.WriteLine("8 - Leer Mensajes");

                                        // receive opcion 1 para mensajes nuevo o 2 para todos, exit en otro caso
                                        // devolver los mensajes pedidos y marcarlos como leido = true
                                        string opcionElegidoLeer = networkdatahelper.Receive();
                                        if(string.Equals(opcionElegidoLeer, "exit"))
                                        {
                                            break;
                                        }
                                        // Console.WriteLine("1 - Leer nuevos mensajes");
                                        List<Mensaje> mensajesNuevos;
                                        if (string.Equals(opcionElegidoLeer, "1"))
                                        {
                                            mensajesNuevos = mensajes.FindAll(x => string.Equals(x.destinatario, usernameConnected) && !x.visto);
                                            networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
                                        } else 
                                        {
                                            mensajesNuevos = mensajes.FindAll(x => string.Equals(x.destinatario, usernameConnected));
                                            networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
                                        }
                                        mensajesNuevos.ForEach(x => x.visto = true);
                                        Console.WriteLine("Mensajes enviados al cliente: ");
                                        mensajesNuevos.ForEach(x => x.ImprimirServer());
                                        break;
                                    case "9":
                                        // Console.WriteLine("9 - Salir");
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
                    Console.WriteLine("ERROR:" + "Cliente desconectado de manera forzada \n" + socketClient.RemoteEndPoint + "\n");
                    clientIsConnected = false;
                }

            }
            Console.WriteLine(socketClient.RemoteEndPoint);
            socketClient.Shutdown(SocketShutdown.Both);
            socketClient.Close();
            socketClient.Dispose();
            Console.WriteLine("Cliente desconectado");
        }
    }
}
