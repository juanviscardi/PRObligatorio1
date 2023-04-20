using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Repuesto
    {
        // Auto-implemented properties.

        public string Name { get; set; }
        public string Proveedor { get; set; }
        public string Marca { get; set; }
        public string Foto { get; set; }


        public Repuesto()
        {
        }

        public Repuesto(string name, string proveedor, string marca)
        {
            this.Name = name;
            this.Proveedor = proveedor;
            this.Marca = marca;
        }
        public override string ToString ()
        {
            return this.Name + ProtocolSpecification.fieldsSeparator +
                                this.Proveedor + ProtocolSpecification.fieldsSeparator +
                                this.Marca;
        }
    }
}
