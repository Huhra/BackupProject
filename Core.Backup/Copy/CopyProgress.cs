using log4net;

namespace Core.Backup.Copy
{
    public abstract class CopyProgress
    {
        protected static readonly ILog Logger = LogManager.GetLogger(typeof(CopyProgress));
        public CopyProgressType ProgressType { get; }

        protected CopyProgress(CopyProgressType progressType)
        {
            ProgressType = progressType;
        }

        public abstract void Log();
    }
}
