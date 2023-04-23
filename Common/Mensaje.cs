namespace Common
{
    public class Mensaje
    {
        public string remitente { get; set; }
        public string destinatario { get; set; }
        public string cuerpoMensaje { get; set; }
        public DateTime fecha { get; set; }
        public bool visto { get; set; }


        public Mensaje(string remitente, string destinatario, string cuerpoMensaje, DateTime fecha, bool visto)
        {
            this.remitente = remitente;
            this.destinatario = destinatario;
            this.cuerpoMensaje = cuerpoMensaje;
            this.fecha = fecha;
            this.visto = visto;
        }

        public Mensaje(string remitente, string destinatario, string cuerpoMensaje)
        {
            this.remitente = remitente;
            this.destinatario = destinatario;
            this.cuerpoMensaje = cuerpoMensaje;
            this.fecha = DateTime.Now;
            this.visto = false;
        }

        public Mensaje(string mensajeCompleto)
        {
            List<string> listMensajeCompleto = mensajeCompleto.Split(ProtocolSpecification.fieldsSeparator).ToList();
            this.remitente = listMensajeCompleto[0];
            this.destinatario = listMensajeCompleto[1];
            this.cuerpoMensaje = listMensajeCompleto[2];
            this.fecha = DateTime.Parse(listMensajeCompleto[3]);
            this.visto = bool.Parse(listMensajeCompleto[4]);
        }

        public override string ToString()
        {
            return this.remitente + ProtocolSpecification.fieldsSeparator +
                    this.destinatario + ProtocolSpecification.fieldsSeparator +
                    this.cuerpoMensaje + ProtocolSpecification.fieldsSeparator +
                    this.fecha + ProtocolSpecification.fieldsSeparator +
                    this.visto;
        }

        public void ImprimirServer()
        {
            Console.WriteLine("Mensaje enviado por: {0}", this.remitente);
            Console.WriteLine("Mensaje enviado a: {0}", this.destinatario);
            Console.WriteLine("Mensaje: {0}", this.cuerpoMensaje);
            Console.WriteLine("Fecha: " + this.fecha);
            Console.WriteLine("mensaje visto: {0}",this.visto ? "Si" : "No");
        }

        public void ImprimirCliente()
        {
            Console.WriteLine("Mensaje enviado por: {0}", this.remitente);
            Console.WriteLine("Mensaje: {0}", this.cuerpoMensaje);
            Console.WriteLine("Fecha: " + this.fecha);
        }
    }
}
