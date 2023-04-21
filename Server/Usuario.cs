using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Usuario
    {
        // Auto-implemented properties.

        public string userName { get; set; }
        public string userPassword { get; set; }
        public string userTipo { get; set; }
        //public string Foto { get; set; }


        public Usuario()
        {
        }

        public Usuario(string name, string password, string tipo)
        {
            this.userName = name;
            this.userPassword = password;
            this.userTipo = tipo;
        }
        public override string ToString()
        {
            return this.userName + ProtocolSpecification.fieldsSeparator +
                                this.userPassword + ProtocolSpecification.fieldsSeparator +
                                this.userTipo;
        }
    }
}
