using Core.Backup.Crc;
using Core.Backup.FileSearcher;
using Core.Backup.Parameters;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Backup
{
    public class BackupWorker
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BackupWorker));
        private static readonly object _dbLock = new object();

        private static readonly DateTime _30hoursAgo;

        static BackupWorker()
        {
            _30hoursAgo = DateTime.Now.Subtract(TimeSpan.FromHours(30));
        }

        public Directory GetDirectory(string path)
        {
            lock (_dbLock)
            {
                using (var db = new BackupDbEntities())
                {
                    var directory = db.Directories.FirstOrDefault(dir => dir.FullPath == path);
                    if (directory == null)
                    {
                        var di = new DirectoryInfo(path);
                        directory = new Directory
                        {
                            FullPath = path,
                            IsNew = 1,
                            Name = di.Name
                        };
                        db.Directories.Add(directory);
                        db.SaveChanges();
                    }
                    return directory;
                }
            }
        }

        public File GetFile(string path, Directory directory)
        {
            lock (_dbLock)
            {
                using (var db = new BackupDbEntities())
                {
                    var file = db.Files.FirstOrDefault(fi => fi.FullPath == path);
                    if (file == null)
                    {
                        var crc = GetCrc(path);
                        var fi = new FileInfo(path);

                        StatusFlag status = StatusFlag.Unchanged;
                        if (fi.LastWriteTime > _30hoursAgo)
                            status = StatusFlag.Changed;

                        file = new File
                        {
                            FullPath = path,
                            IsNew = (long)status,
                            Name = fi.Name,
                            Crc = crc,
                            DirectoryId = directory.Id,
                            LastWriteTime = fi.LastWriteTime.Ticks
                        };
                        db.Files.Add(file);
                        db.SaveChanges();
                    }
                    else
                    {
                        CheckFileDiff(db, file);
                    }
                    return file;
                }
            }
        }

        private void CheckFileDiff(BackupDbEntities db, File file)
        {
            var fileInfo = new FileInfo(file.FullPath);
            if (file.LastWriteTime != fileInfo.LastWriteTime.Ticks)
            {
                var crc = GetCrc(file.FullPath);
                if (crc != file.Crc)
                {
                    file.IsNew = (long)StatusFlag.Changed;
                    db.SaveChanges();
                }
            }
        }

        public void Clean()
        {
            using (var db = new BackupDbEntities())
            {
                var files = db.Files.ToList();
                db.Files.RemoveRange(files);
                db.SaveChanges();
                var directories = db.Directories.ToList();
                db.Directories.RemoveRange(directories);
                db.SaveChanges();
            }
        }

        public int GetDifferentFiles()
        {
            using (var db = new BackupDbEntities())
            {
                var changes = db.Files.Where(x => x.IsNew ==
                    (long)StatusFlag.Changed || x.IsNew ==
                    (long)StatusFlag.Deleted).ToList();
                foreach (var deletedFile in changes.Where(x => x.IsNew == (long)StatusFlag.Deleted))
                {
                    Logger.Info($"We should delete file: {deletedFile.FullPath}");
                }

            }
            return 0;
        }

        public async Task<int> CheckDifferentFiles()
        {
            await Task.Yield();
            var config = ConfigManager.GetConfig();
            Logger.Info($"Starting to look for folders and files in {config.RootDirectory}");
            var folders = Searcher.GetFolders(config.RootDirectory).ToList();
            Logger.Info($"Folders count: {folders.Count}");
            int count = 0;
            foreach (var folder in folders.AsParallel())
            {
                await Task.Yield();
                var directory = GetDirectory(folder);
                await Task.Yield();
                var files = Searcher.GetFiles(folder).Select(filePath =>
                {
                    var file = GetFile(filePath, directory);
                    return file.Id;
                }).ToList();
                lock (_dbLock)
                    count += files.Count;
            }
            Logger.Info($"Files checked count: {count}");
            return count;
        }

        private string GetCrc(string file)
        {
            var crc = string.Empty;
            try
            {
                var fi = new FileInfo(file);
                crc = Checksum.GetChecksum(file);
                Logger.Info($"CRC: {crc}, File: {fi.Name}");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
            return crc;
        }
    }
}
