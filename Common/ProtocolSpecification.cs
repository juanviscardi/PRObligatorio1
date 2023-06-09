﻿namespace Common
{
    public static class ProtocolSpecification
    {
        //Protocolo para comandos
        //public const int fixedHeaderSize = 3;
        //public const int fixedCmdSize = 4;
        public const int fixedLength = 4;
        public const int fixedSize = 8;

        //Para archivos/foto
        public const int FixedFileSize = 8;
        public const int FixedDataSize = 4;
        public const int MaxPacketSize = 32768; //32KB

        //separador para usurio/password o para repuestos o algo mas
        public const string fieldsSeparator = ";";
        public const string valuesSeparator = ":";

        public const int FixedBackpack = 100;


        /*
        public static long CalculateFileParts(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1;
        }*/
        public static async Task<long> CalculateFileParts(long fileSize)
        {
            var fileParts = fileSize / MaxPacketSize;
            return await Task.Run(() => fileParts * MaxPacketSize == fileSize ? fileParts : fileParts + 1);
        }
    }
}
