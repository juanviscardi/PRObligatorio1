using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ProtocolSpecification
    {
        //Protocolo para comandos
        public const int fixedHeaderSize = 3;
        public const int fixedCmdSize = 4;
        public const int fixedLength = 4;

        //separador para usurio/password o para repuestos o algo mas
        public const char fieldsSeparator = ';';
        public const char valuesSeparator = ':';

    }
}
