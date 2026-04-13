using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SistemaInventario.Services
{
    public class FacturaCompressionService
    {
        public byte[] ComprimirXmlAGzipBytes(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new Exception("El XML está vacío y no se puede comprimir.");

            byte[] xmlBytes = Encoding.UTF8.GetBytes(xml);

            using var outputStream = new MemoryStream();

            using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                gzipStream.Write(xmlBytes, 0, xmlBytes.Length);
            }

            return outputStream.ToArray();
        }

        public string ConvertirBytesABase64(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new Exception("No existen bytes para convertir a Base64.");

            return Convert.ToBase64String(bytes);
        }
    }
}