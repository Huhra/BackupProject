using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Copy
{
    public class EndCopyProgress : CopyProgress
    {
        public EndCopyProgress(int filesCount, int filesTotalSize, TimeSpan totalTime, bool cancelled = false)
        {
            FilesCount = filesCount;
            FilesTotalSize = filesTotalSize;
            TotalTime = totalTime;
            Cancelled = cancelled;
        }

        public bool Cancelled { get; set; }

        public TimeSpan TotalTime { get; set; }

        public int FilesTotalSize { get; set; }

        public int FilesCount { get; set; }
    }
}
