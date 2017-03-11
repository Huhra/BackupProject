using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Helper
{
    public static class SizeConverter
    {
        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
        public static double ConvertMegabytesToGigabytes(double megabytes) // SMALLER
        {
            // 1024 megabyte in a gigabyte
            return megabytes / 1024.0;
        }

        public static double ConvertMegabytesToTerabytes(double megabytes) // SMALLER
        {
            // 1024 * 1024 megabytes in a terabyte
            return megabytes / (1024.0 * 1024.0);
        }
    }
}
