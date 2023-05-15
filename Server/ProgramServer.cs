using Common;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMng = new SettingsManager();
        static List<Usuario> usuarios = new List<Usuario>();
        static List<Repuesto> repuestos = new List<Repuesto>();
        static List<string> categorias = new List<string>();
        static List<Mensaje> mensajes = new List<Mensaje>();

        private static readonly SemaphoreSlim _agregarUsuario = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _agregarRepuesto = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _agregarCategoria = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly SemaphoreSlim _asociarCategoria = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Iniciando Aplicacion de Servidor.....!!!");

            //Sustituimos ip y port por los valores del archivo
            string serverIp = settingsMng.ReadSettings(ServerConfig.serverIPConfigKey);
            int serverPort = int.Parse(settingsMng.ReadSettings(ServerConfig.serverPortconfigKey));

            var localEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            var tcpListener = new TcpListener(localEndPoint);
            Console.WriteLine("Servidor inicializado con IP {0} y Puerto {1}", serverIp, serverPort);

            tcpListener.Start(ProtocolSpecification.FixedBackpack); //Cantidad maxima de clientes que puedo encolar sin atender

            int clientes = 0;
            bool salir = false;

            Console.WriteLine("Esperando por Clientes.....");

            while (!salir)
            {
                clientes++;
                int nro = clientes;

                // Espera a que llegue una nueva conexion
                // Console.WriteLine("Acepte un nuevo pedido de conexion");

                var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                await Task.Run(() => Task.FromResult(HandleClient(tcpClientSocket).ConfigureAwait(false))); // Pedir un "hilo" del CLR prestado
            }

        }

        private static List<Repuesto> GetRepuestos()
        {
            return repuestos;
         }

     private static async Task HandleClient(TcpClient tcpClientSocket)
        {

            bool clientIsConnected = true;
            string userType = "error";
            string usernameConnected = "";
            NetworkDataHelper networkdatahelper = new NetworkDataHelper(tcpClientSocket);
            string[] ipAddressPuerto = tcpClientSocket.Client.RemoteEndPoint.ToString().Split(":");
            Console.WriteLine("Tratando de Conectar desde IP: {0} usando el puerto: {1}", ipAddressPuerto[0], ipAddressPuerto[1]);

            while (clientIsConnected)
            {
                try
                {
                    if (string.Equals(userType, "error"))
                {
                        // log in
                        string messageUsuarioContrasena = await networkdatahelper.Receive();
                        string[] usuarioContrasena = messageUsuarioContrasena.Split(ProtocolSpecification.fieldsSeparator);
                        string usuario = usuarioContrasena[0];
                        string pass = usuarioContrasena[1];
                        string adminUsername = settingsMng.ReadSettings(ServerConfig.usernameConfigKey);
                        string adminPassword = settingsMng.ReadSettings(ServerConfig.passwordConfigKey);
                        if (string.Equals(usuario, adminUsername) && string.Equals(pass, adminPassword))
                        {
                            usernameConnected = usuario;
                            userType = "admin";

                            Console.WriteLine();
                            Console.WriteLine("Usuario: {0} hizo login como Administrador", usuario);
                            Console.WriteLine();
                            await networkdatahelper.Send("admin");
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
                                await networkdatahelper.Send("mecanico");
                            }
                            else
                            {
                                Console.WriteLine("Usuario: {0} no se conecto", usuario);
                                await networkdatahelper.Send("error");
                            }
                        }
                        continue;
                    }
                    string cmd = await networkdatahelper.Receive();
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

                                        string altaUsuarioRequest = await networkdatahelper.Receive();
                                        string[] altaUsuarioRequestConTodo = altaUsuarioRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var userName = altaUsuarioRequestConTodo[0];
                                        var userPassword = altaUsuarioRequestConTodo[1];

                                        Usuario user = new Usuario(
                                                           userName,
                                                           userPassword,
                                                           "mecanico");

                                        
                                        await _agregarUsuario.WaitAsync();
                                        
                                            if (!usuarios.Contains(user))
                                            {
                                                usuarios.Add(user);
                                                await networkdatahelper.Send("exito");
                                                Console.WriteLine("Se Creo un nuevo usuario");
                                                Console.WriteLine(user.ToString());
                                                Console.WriteLine();
                                            }
                                            else 
                                            {
                                                await networkdatahelper.Send("el usuario ya existe");
                                            }

                                        _agregarUsuario.Release();

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
                                        string altaRepuestoRequest = await networkdatahelper.Receive();
                                        string[] altaRepuestoRequestConTodo = altaRepuestoRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        var repuestoName = altaRepuestoRequestConTodo[0];
                                        var repuestoProveedor = altaRepuestoRequestConTodo[1];
                                        var repuestoMarca = altaRepuestoRequestConTodo[2];
                                        Repuesto repu = new Repuesto(
                                                           GetRepuestos().Count().ToString(),
                                                           repuestoName,
                                                           repuestoProveedor,
                                                           repuestoMarca);
                                        await _agregarRepuesto.WaitAsync();
                                        
                                            if (!GetRepuestos().Contains(repu))
                                            {
                                            GetRepuestos().Add(repu);
                                                await networkdatahelper.Send("exito");
                                            }
                                            else
                                            {
                                                await networkdatahelper.Send("el repuesto ya existe");
                                            }
                                        _agregarRepuesto.Release();
                                        break; 
                       

                                    case "2":
                                        // Console.WriteLine("2 - Alta de Categoría de repuesto");
                                        // SRF3.Crear Categoría de repuesto. El sistema debe permitir crear una categoría para los repuestos.
                                        // CRF3. Alta de Categoría de repuesto. El sistema debe permitir crear una Categoría para los repuestos.
                                        string categoria = await networkdatahelper.Receive();
                                        await _agregarCategoria.WaitAsync();
                                        
                                            if (!categorias.Contains(categoria))
                                            {
                                                categorias.Add(categoria);
                                                await networkdatahelper.Send("exito");
                                            }
                                            else
                                            {
                                                await networkdatahelper.Send("la categoria ya existe");
                                            }
                                        _agregarCategoria.Release();

                                        break;
                                    case "3":
                                        //SRF4. Asociar Categorías a un repuesto. El sistema debe permitir asociar categorías a los repuestos.
                                        //CRF4. Asociar Categorías a los repuestos. El sistema debe permitir asociar categorías a losrepuestos.
                                        // Console.WriteLine("3 - Asociar Categorías a los repuestos");
                                        // envio nombre de repuestos existentes para que se listen en el cliente
                                        List<string> repuestosExistentesNamesResponse = new List<string>();
                                        GetRepuestos().ToList().ForEach(x => {
                                            repuestosExistentesNamesResponse.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesNamesResponse));
                                        // envio nombre de categorias existentes para que se listen en el cliente
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, categorias));
                                        // recivo el nombre del repuesto elegio y la categoria elegida
                                        string asociarCategoriaRequest = await networkdatahelper.Receive();
                                        if(string.Equals(asociarCategoriaRequest, "exit"))
                                        {
                                            break;
                                        }
                                        string[] asociarCategoriaRequestConTodo = asociarCategoriaRequest.Split(ProtocolSpecification.fieldsSeparator);
                                        string repuestoName4 = asociarCategoriaRequestConTodo[0];
                                        string categoria4 = asociarCategoriaRequestConTodo[1];
                                        await _asociarCategoria.WaitAsync();
                                        
                                            var repuesto4 = GetRepuestos().Find(x => string.Equals(repuestoName4, x.Name));
                                            if (repuesto4 != null)
                                            {
                                                if (!repuesto4.Categorias.Contains(categoria4)) // Verifica si la categoría ya está presente en el Repuesto
                                                {
                                                    if(!categorias.Contains(categoria4))
                                                    {
                                                        await networkdatahelper.Send("La categoría no existe");
                                                        break;
                                                    }
                                                    repuesto4.Categorias.Add(categoria4); // Agrega la nueva categoría al Repuesto
                                                    await networkdatahelper.Send("exito");
                                                }
                                                else
                                                {
                                                    await networkdatahelper.Send("La categoría ya está presente en el Repuesto.");
                                                }
                                            }
                                            else
                                            {
                                                await networkdatahelper.Send("El repuesto no existe.");
                                            }
                                        
                                        _asociarCategoria.Release();

                                        break;
                                    case "4":
                                        //SRF5 Asociar una foto al repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                        //CRF5. Asociar foto a repuesto. El sistema debe permitir subir una foto y asociarla a un repuesto específico.
                                        
                                        List<string> repuestosExistentesParaFotoResponse = new List<string>();
                                        GetRepuestos().ToList().ForEach(x => {
                                            repuestosExistentesParaFotoResponse.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesParaFotoResponse));
                                        string nombreRepuestoOExit = await networkdatahelper.Receive();
                                        if(string.Equals(nombreRepuestoOExit,"exit"))
                                        {
                                            break;
                                        }
                                        var repuesto5 = GetRepuestos().Find(x => string.Equals(nombreRepuestoOExit, x.Name));
                                        if (repuesto5 != null)
                                        {
                                            await networkdatahelper.Send("El repuesto existe.");
                                        }
                                        else
                                        {
                                            await networkdatahelper.Send("El repuesto no existe.");
                                            break;
                                        }
                                        FileCommsHandler fileCommsHandler = new FileCommsHandler(tcpClientSocket);
                                        string nombreArchivo = await fileCommsHandler.ReceiveFile();
                                        // el archivo queda guardado en el bin
                                        repuesto5.Foto = nombreArchivo;
                                        await networkdatahelper.Send("Se asocio la foto al repuesto.");

                                        break;
                                    case "5":
                                        // SRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes,incluyendo búsquedas por palabras claves.
                                        // CRF6. Consultar repuestos existentes. El sistema deberá poder buscar repuestos existentes, incluyendo búsquedas por palabras claves.
                                        
                                        string opcionListado = await networkdatahelper.Receive();
                                        List<string> repuestosExistentesResponse = new List<string>();
                                        switch (opcionListado)
                                        {
                                            case "1":
                                                // Console.WriteLine("1 - Listar todos");
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    repuestosExistentesResponse.Add(x.ToStringListar());
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "2":
                                                // Console.WriteLine("2 - Buscar por nombre repuesto");
                                                string opcionListadoNombre = await networkdatahelper.Receive();
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Name, opcionListadoNombre))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "3":
                                                // Console.WriteLine("3 - Buscar por categoria");
                                                string opcionListadoCategoria = await networkdatahelper.Receive();
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (x.Categorias.Contains(opcionListadoCategoria))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "4":
                                                // Console.WriteLine("4 - Buscar por nombre archivo foto");
                                                string opcionListadoFoto = await networkdatahelper.Receive();
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Foto, opcionListadoFoto))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "5":
                                                // Console.WriteLine("5 - Buscar por nombre de proveedor");
                                                string opcionListadoProveedor = await networkdatahelper.Receive();
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Proveedor, opcionListadoProveedor))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                            case "6":
                                                // Console.WriteLine("6 - Buscar por nombre de marca")
                                                string opcionListadoMarca = await networkdatahelper.Receive();
                                                GetRepuestos().ToList().ForEach(x =>
                                                {
                                                    if (string.Equals(x.Marca, opcionListadoMarca))
                                                    {
                                                        repuestosExistentesResponse.Add(x.ToStringListar());
                                                    }
                                                });
                                                await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, repuestosExistentesResponse));
                                                break;
                                        }
                                        break;
                                    case "6":
                                        // SRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.
                                        
                                        // CRF7. Consultar un repuesto específico. El sistema deberá poder buscar un repuesto
                                        // específico.También deberá ser capaz de descargar la imagen asociada, en caso de existir la misma.

                                        List<string> nombreRepuestosExistentes = new List<string>();
                                        GetRepuestos().ToList().ForEach(x => {
                                            nombreRepuestosExistentes.Add(x.Name);
                                        });
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreRepuestosExistentes));
                                        string nombreRepuestoQuieroDetalles = await networkdatahelper.Receive();
                                        if(string.Equals(nombreRepuestoQuieroDetalles, "exit"))
                                        {
                                            break;
                                        }
                                        var repuesto6 = GetRepuestos().Find(x => string.Equals(nombreRepuestoQuieroDetalles, x.Name));
                                        if (repuesto6 != null)
                                        {
                                            bool tieneFoto = true;
                                            if(string.IsNullOrEmpty(repuesto6.Foto))
                                            {
                                                tieneFoto = false;
                                            }
                                            string response = repuesto6.ToStringListar() + ProtocolSpecification.fieldsSeparator + tieneFoto;
                                            await networkdatahelper.Send(response);
                                        }
                                        else
                                        {
                                            await networkdatahelper.Send("El repuesto no existe.");
                                        }
                                        string enviarFoto = await networkdatahelper.Receive();
                                        if (string.Equals(enviarFoto, "NO")) break;
                                        FileCommsHandler fileCommsHandler2 = new FileCommsHandler(tcpClientSocket);
                                        await fileCommsHandler2.SendFile(repuesto6.Foto);
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
                                        await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, nombreMecanicos));
                                        string requestEnviarMensaje = await networkdatahelper.Receive();
                                        // validar inputs
                                        if (string.Equals(requestEnviarMensaje, "exit")) break;
                                        Mensaje mensajeEnviado = new Mensaje(requestEnviarMensaje);
                                        if(string.Equals(mensajeEnviado.remitente, mensajeEnviado.destinatario))
                                        {
                                            await networkdatahelper.Send("No se puede enviar un mensaje a si mismo");
                                            break;
                                        }
                                        var usuarioDestintario = usuarios.Find(x => string.Equals(mensajeEnviado.destinatario, x.userName));
                                        if (usuarioDestintario == null)
                                        {
                                            await networkdatahelper.Send("El destinatario no es un mecanica valido.");
                                            break;
                                        }
                                        if(string.IsNullOrEmpty(mensajeEnviado.cuerpoMensaje))
                                        {
                                            await networkdatahelper.Send("El cuerpo del mensaje no puede estar vacio.");
                                            break;
                                        }
                                        // enviar respuesta a imprimir en el cliente
                                        mensajes.Add(mensajeEnviado);
                                        mensajeEnviado.ImprimirServer();
                                        await networkdatahelper.Send("Mensaje enviado. ");
                                        // CRF8.Enviar y recibir mensajes. El sistema debe permitir que un mecánico envíe mensajes
                                        // a otro, y que el mecánico receptor chequee sus mensajes sin leer, así como también revisar su historial de mensajes.

                                        break;
                                    case "8":
                                        // Console.WriteLine("8 - Leer Mensajes");

                                        // receive opcion 1 para mensajes nuevo o 2 para todos, exit en otro caso
                                        // devolver los mensajes pedidos y marcarlos como leido = true
                                        string opcionElegidoLeer = await networkdatahelper.Receive();
                                        if(string.Equals(opcionElegidoLeer, "exit"))
                                        {
                                            break;
                                        }
                                        // Console.WriteLine("1 - Leer nuevos mensajes");
                                        List<Mensaje> mensajesNuevos;
                                        if (string.Equals(opcionElegidoLeer, "1"))
                                        {
                                            mensajesNuevos = mensajes.FindAll(x => string.Equals(x.destinatario, usernameConnected) && !x.visto);
                                            await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
                                        } else 
                                        {
                                            mensajesNuevos = mensajes.FindAll(x => string.Equals(x.destinatario, usernameConnected));
                                            await networkdatahelper.Send(string.Join(ProtocolSpecification.fieldsSeparator, mensajesNuevos));
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
                catch
                {
                    Console.WriteLine("ERROR: Cliente desconectado de manera forzada \n" + tcpClientSocket.Client.RemoteEndPoint.ToString() + "\n");
                    clientIsConnected = false;
                }

            }
            tcpClientSocket.Client.Close();
            Console.WriteLine("Cliente desconectado.");

        }
    }
}
