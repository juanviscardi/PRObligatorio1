using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class ConsoleClientMenu
    {
        public static void GetMenu(string userType)
        {
            switch(userType)
            {
                case "admin":
                    {
                        HeadMenuClient("ADMIN MAIN MENU", "");

                        Console.WriteLine("1 - Anadir usuario");
                        Console.WriteLine("2 - Salir");
                        break;
                    }
                case "mecanico":
                    {
                        HeadMenuClient("Mecanico MAIN MENU", "");

                        Console.WriteLine("1 - Alta de repuesto");
                        Console.WriteLine("2 - Alta de Categoría de repuesto");
                        Console.WriteLine("3 - Asociar Categorías a los repuestos");
                        Console.WriteLine("4 - Asociar foto a repuesto");
                        Console.WriteLine("5 - Consultar repuestos existentes");
                        Console.WriteLine("6 - Consultar un repuesto específico");
                        Console.WriteLine("7 - Enviar y recibir mensajes");
                        Console.WriteLine("8 - Salir");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public static void DisplayPhotoMenu(string userLogged)
        {
            HeadMenuClient("CLIENT PHOTO", userLogged);

            Console.WriteLine("1. UPLOAD");
            Console.WriteLine("2. COMMENT PHOTO");
            Console.WriteLine("3. LIST ALL PHOTOS FOR REPUESTO");
            Console.WriteLine("4. LIST COMMENTS FROM REPUESTO PHOTOS");
            Console.WriteLine("5. BACK ");

        }

        private static void HeadMenuClient(string title, string userLogged)
        {
            //Console.Clear();
            string dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            Console.WriteLine("==============================================================");
            Console.WriteLine("User Logged: " + userLogged);
            Console.WriteLine(dateTime);
            Console.WriteLine("                           " + title);
            Console.WriteLine("============================================================== \n");
        }

    }
}
