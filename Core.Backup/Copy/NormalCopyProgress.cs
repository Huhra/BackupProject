using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Copy
{
    public class NormalCopyProgress : CopyProgress
    {
        public double MbCopied { get; }
        public double MbTotal { get; }
        public int FilesCopied { get; }
        public int FilesTotal { get; }
        public TimeSpan RemainingTime { get; }
        public double Percentage { get; }

        public NormalCopyProgress(double percentage, int filesCopied, int filesTotal, double mbCopied, double mbTotal, TimeSpan remainingTime)
            : base(CopyProgressType.Normal)
        {
            Percentage = percentage;
            MbCopied = mbCopied;
            MbTotal = mbTotal;
            FilesCopied = filesCopied;
            FilesTotal = filesTotal;
            RemainingTime = remainingTime;
        }

        public override void Log()
        {
            Logger.Info($"NormalCopyProgress: {Percentage:N1}%, {FilesCopied}/{FilesTotal}, {MbCopied:N1}Mb/{MbTotal:N1}Mb, {RemainingTime:g} remaining");
        }
    }
}
