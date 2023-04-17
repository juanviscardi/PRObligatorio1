using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class repuesto
    {
        // Auto-implemented properties.

        public string Name { get; set; }
        public string Proveedor { get; set; }
        public string Marca { get; set; }


        public repuesto()
        {
        }

        public repuesto(string name, string proveedor, string marca)
        {
            this.Name = name;
            this.Proveedor = proveedor;
            this.Marca = marca;
        }
    }
}
