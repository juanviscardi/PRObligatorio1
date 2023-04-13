﻿using System;
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
        public const int MaxPacketSize = 32768; //32KB

        //separador para usurio/password o para repuestos o algo mas
        public const char fieldsSeparator = ';';
        public const char valuesSeparator = ':';

        public static long CalculateFileParts(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1;
        }
    }
}
