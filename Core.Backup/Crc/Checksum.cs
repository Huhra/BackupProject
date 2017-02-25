using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Crc
{
    public static class Checksum
    {
        public static string GetChecksum(string file)
        {
            using (var stream = new BufferedStream(System.IO.File.OpenRead(file), 1200000))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
    }
}
