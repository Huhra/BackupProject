using Core.Backup.Crc;
using Core.Backup.FileSearcher;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup
{
    public class BackupWorker
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BackupWorker));
        private static readonly object _dbLock = new object();

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
                        file = new File
                        {
                            FullPath = path,
                            IsNew = 1,
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
                    file.IsNew = 2;
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
                var changes = db.Files.Where(x => x.IsNew == 2).ToList();
                Console.WriteLine("");
            }
            return 0;
        }

        public async Task<int> Start()
        {
            await Task.Yield();
            var folders = Searcher.GetFolders(@"D:\Medias").ToList();
            Console.WriteLine($"Folders count: {folders.Count}");
            int count = 0;
            foreach (var folder in folders.AsParallel())
            {
                await Task.Yield();
                var directory = GetDirectory(folder);
                await Task.Yield();
                var files = Searcher.GetFiles(folder).AsParallel().Select(file =>
                {
                    return GetFile(file, directory);
                }).ToList();
                lock (_dbLock)
                    count += files.Count;
            }
            Console.WriteLine($"Files count: {count}");
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
