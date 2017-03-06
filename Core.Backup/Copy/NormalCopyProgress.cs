using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Copy
{
    public class NormalCopyProgress : CopyProgress
    {
        public int FilesCopied { get; }
        public int FilesTotal { get; }
        public TimeSpan RemainingTime { get; }

        public NormalCopyProgress(int filesCopied, int filesTotal, TimeSpan remainingTime)
        {
            FilesCopied = filesCopied;
            FilesTotal = filesTotal;
            RemainingTime = remainingTime;
        }
    }
}
