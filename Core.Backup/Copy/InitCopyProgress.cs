using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Copy
{
    public class InitCopyProgress : CopyProgress
    {
        public int NumberFiles { get; }

        public InitCopyProgress(int numberFiles)
            : base(CopyProgressType.Init)
        {
            NumberFiles = numberFiles;
        }

        public override void Log()
        {
            Logger.Info($"InitCopyFiles: {NumberFiles}");
        }
    }
}
