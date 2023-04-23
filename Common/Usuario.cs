namespace Common
{
    public class Usuario
    {
        // Auto-implemented properties.

        public string userName { get; set; }
        public string userPassword { get; set; }
        public string userTipo { get; set; }
        public bool connected { get; set; }
        //public string Foto { get; set; }


        public Usuario()
        {
        }

        public Usuario(string name, string password, string tipo)
        {
            this.userName = name;
            this.userPassword = password;
            this.userTipo = tipo;
            connected = false;
        }
        public override string ToString()
        {
            return this.userName + ProtocolSpecification.fieldsSeparator +
                                this.userPassword + ProtocolSpecification.fieldsSeparator +
                                this.userTipo;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Usuario))
            { return false; } 
            Usuario otroUsuario = (Usuario)obj; 
            return userName == otroUsuario.userName; 
        }
    }
}
