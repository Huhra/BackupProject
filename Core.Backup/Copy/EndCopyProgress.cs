using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Copy
{
    public class EndCopyProgress : CopyProgress
    {
        public TimeSpan TotalTime { get; }

        public double FilesTotalSize { get; }

        public int FilesCount { get; }

        public EndCopyProgress(int filesCount, double filesTotalSize, TimeSpan totalTime)
            : base (CopyProgressType.End)
        {
            FilesCount = filesCount;
            FilesTotalSize = filesTotalSize;
            TotalTime = totalTime;
        }

        public override void Log()
        {
            Logger.Info($"EndCopyProgress, FilesCount: {FilesCount}, FilesTotalSize: {FilesTotalSize:N1}Mb, TotalTime: {TotalTime:g}");
        }
    }
}
