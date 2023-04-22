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

        public string Id { get; set; }  
        public string Name { get; set; }
        public string Proveedor { get; set; }
        public string Marca { get; set; }
        public string Foto { get; set; }
        public List<string> Categorias { get; set; }


        //public Repuesto()
        //{
        //    this.Categorias = new List<string>();
        //    this.Id = "";
        //}

        public Repuesto(string id, string name, string proveedor, string marca)
        {
            this.Id = id;
            this.Name = name;
            this.Proveedor = proveedor;
            this.Marca = marca;
            this.Categorias = new List<string>();
        }
        public override string ToString ()
        {
            return this.Id + ProtocolSpecification.fieldsSeparator + 
                    this.Name + ProtocolSpecification.fieldsSeparator +
                    this.Proveedor + ProtocolSpecification.fieldsSeparator +
                    this.Marca;
        }

        public string ToStringListar()
        {
            return $"Id: {Id}, Nombre: {Name}, Proveedor: {Proveedor}, Marca: {Marca}, Foto: {Foto}, Categorias: {string.Join(", ", Categorias)}";
        }



        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Repuesto))
            { return false; }
            Repuesto otroRepuesto = (Repuesto)obj;
            return Name == otroRepuesto.Name;
        }
    }
}
