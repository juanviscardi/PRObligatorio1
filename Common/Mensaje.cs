using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
