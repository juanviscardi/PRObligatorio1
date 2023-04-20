using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    internal class ConsoleClientMenu
    {
        public static void DisplayMainMenu(string userLogged)
        {
            HeadMenuClient("CLIENT MAIN MENU", userLogged);

            Console.WriteLine("1. REGISTER");
            Console.WriteLine("2. LOGIN");
            Console.WriteLine("3. EXIT");
        }

        public static void DisplayMainLoggedMenu(string userLogged)
        {
            HeadMenuClient("CLIENT MAIN MENU", userLogged);

            Console.WriteLine("1. USERS");
            Console.WriteLine("2. PHOTOS");
            Console.WriteLine("3. LOGOUT");
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
            if (userLogged == "")
            {
                userLogged = "Waiting for somebody";
            }
            Console.Clear();
            string dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            Console.WriteLine("==============================================================");
            Console.WriteLine("User Logged: " + userLogged);
            Console.WriteLine(dateTime);
            Console.WriteLine("                           " + title);
            Console.WriteLine("============================================================== \n");
        }

    }
}
