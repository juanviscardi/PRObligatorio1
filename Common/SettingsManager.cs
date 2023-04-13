using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Common
{
    public class SettingsManager
    {
        public string ReadSettings(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key] ?? string.Empty;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error leyendo la configuracion");
                return string.Empty;
            }

        }
    }
}
