using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Backup.Copy;
using Core.Backup.Helper;
using log4net;

namespace Core.Backup
{
    public class ProgressWorker
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ProgressWorker));
        private readonly DateTime _startTime = DateTime.Now;
        private readonly IReadOnlyList<File> _files;
        private readonly int _filesCount;
        private readonly object _lockProgress = new object();

        private double _totalSizeMb;
        private int _filesCopied;
        private double _totalCopied;

        public delegate void ProgressChanged(CopyProgress progress);
        public static event ProgressChanged OnProgress;

        public ProgressWorker(IReadOnlyList<File> files)
        {
            _files = files;
            _filesCount = files.Count;
            RaiseEvent(new InitCopyProgress(_filesCount));
            Task.Factory.StartNew(DoAsyncProgress);
        }

        private async Task DoAsyncProgress()
        {
            await GetFilesSize();
        }

        private double GetFileSize(string path)
        {
            var fileInfo = new FileInfo(path);
            var mbSize = SizeConverter.ConvertBytesToMegabytes(fileInfo.Length);
            return mbSize;
        }

        private async Task GetFilesSize()
        {
            await Task.Yield();
            double totalSize = 0;
            foreach (var file in _files.AsParallel())
            {
                try
                {
                    var mbSize = GetFileSize(file.FullPath);
                    lock (_lockProgress)
                        totalSize += mbSize;
                }
                catch (Exception e)
                {
                    Logger.Warn($"Unable to get file size for file {file.Name}, {e.Message}");
                }
            }
            _totalSizeMb = totalSize;
            RaiseEvent(new NormalCopyProgress(0, _filesCopied, _filesCount, _totalCopied, _totalSizeMb, TimeSpan.Zero));
        }

        public void SetFileCopied(File file)
        {
            Task.Factory.StartNew(() =>
            {
                var fileSize = GetFileSize(file.FullPath);
                int filesCopied;
                double totalCopied;
                lock (_lockProgress)
                {
                    _filesCopied += 1;
                    _totalCopied += fileSize;
                    filesCopied = _filesCopied;
                    totalCopied = _totalCopied;
                }
                var timeSpent = DateTime.Now - _startTime;
                if (_filesCopied == _filesCount)
                {
                    RaiseEvent(new EndCopyProgress(_filesCount, _totalSizeMb, timeSpent));
                }
                else
                {
                    var percentage = (_totalCopied * 100.0) / _totalSizeMb;
                    var remainingTime = new TimeSpan((long)(timeSpent.Ticks * (100.0 - percentage)));
                    RaiseEvent(new NormalCopyProgress(percentage, filesCopied, _filesCount, totalCopied, _totalSizeMb, remainingTime));
                }
            });
        }

        private void RaiseEvent(CopyProgress progress)
        {
            OnProgress?.Invoke(progress);
            progress.Log();
        }
    }
}
