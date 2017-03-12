using Core.Backup.Crc;
using Core.Backup.FileSearcher;
using Core.Backup.Parameters;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Backup
{
    public class BackupWorker
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BackupWorker));
        private static readonly object _dbLock = new object();

        private readonly DateTime _30hoursAgo;
        private readonly Action<string> _showMessage;

        public BackupWorker(Action<string> showMessage)
        {
            Logger.Info("BackupWorker ctor");
            _showMessage = showMessage;
            _30hoursAgo = DateTime.Now.Subtract(TimeSpan.FromHours(30));
        }

        public Directory GetDirectory(string path)
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
                    Logger.Info($"Creating folder: {di.Name}");
                    db.SaveChanges();
                }
                return directory;
            }
        }

        public File GetFile(BackupDbEntities db, string path, Directory directory)
        {
            var file = db.Files.FirstOrDefault(fi => fi.FullPath == path);
            if (file == null)
            {
                var crc = GetCrc(path);
                var fi = new FileInfo(path);

                StatusFlag status = StatusFlag.Unchanged;
                //if (fi.LastWriteTime < _30hoursAgo)
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
            }
            else
            {
                CheckFileDiff(db, file);
            }
            return file;
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

        public void CleanDatabase()
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

        public async Task DeleteFiles(Configuration config)
        {
            await Task.Yield();
            using (var db = new BackupDbEntities())
            {
                if (config.DeleteFiles)
                {
                    var deletedFiles = db.Files.Where(x => x.IsNew == (long)StatusFlag.Deleted).ToList();
                    foreach (var deletedFile in deletedFiles.AsParallel())
                    {
                        await Task.Yield();
                        //TODO delete file
                        Logger.Info($"We should delete file: {deletedFile.FullPath}");
                        db.Files.Remove(deletedFile);
                    }
                    db.SaveChanges();
                }
            }
        }

        private Dictionary<int, BackupDbEntities> _entities
            = new Dictionary<int, BackupDbEntities>();

        private BackupDbEntities GetDbContextForThread()
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            lock (_dbLock)
            {
                if (_entities.ContainsKey(threadId))
                    return _entities[threadId];
                var db = new BackupDbEntities();
                _entities.Add(threadId, db);
                return db;
            }
        }

        private void SaveAllContexts()
        {
            lock (_dbLock)
            {
                foreach (var db in _entities.Values)
                    db.SaveChanges();
            }
        }

        public async Task CopyFiles(Configuration config)
        {
            await Task.Yield();
            using (var db = new BackupDbEntities())
            {
                var remoteFolder = new DirectoryInfo(config.RemoteDirectory);
                var rootDirectory = new DirectoryInfo(config.RootDirectory);
                var changedFiles = db.Files.Where(x => x.IsNew == (long)StatusFlag.Changed).ToList();
                var progress = new ProgressWorker(changedFiles);
                var rand = new Random(DateTime.Now.Millisecond);
                foreach (var changedFile in changedFiles.OrderBy(f => rand.Next()).AsParallel())
                {
                    try
                    {
                        var fi = new FileInfo(changedFile.FullPath);
                        var directoryFullPath = fi.Directory.FullName;
                        string dirDiff = directoryFullPath.Replace(rootDirectory.FullName, "");
                        var remoteDir = remoteFolder.FullName + dirDiff;
                        if (!System.IO.Directory.Exists(remoteDir))
                            System.IO.Directory.CreateDirectory(remoteDir);
                        var destinationPath = Path.Combine(remoteDir, fi.Name);
                        System.IO.File.Copy(changedFile.FullPath, destinationPath, true);
                        progress.SetFileCopied(changedFile);
                        changedFile.IsNew = (long)StatusFlag.Unchanged;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"We were not able to upload file: {changedFile.FullPath}, {ex.Message}");
                    }
                }
                db.SaveChanges();
            }
        }

        public async Task DoBackup()
        {
            await Task.Yield();
            var config = ConfigManager.GetConfig();
            if (config.LastRootDirectory != config.RootDirectory)
            {
                Logger.Info("Cleaning database because folders are different");
                CleanDatabase();
                config.LastRootDirectory = config.RootDirectory;
                ConfigManager.SetConfig(config);
            }
            _showMessage?.Invoke("Starting backup process, checking files differences");
            await CheckDifferentFiles(config);
            await DeleteFiles(config);
            _showMessage?.Invoke("Starting files copy");
            await CopyFiles(config);
        }

        public async Task CheckDifferentFiles(Configuration config)
        {
            await Task.Yield();
            Logger.Info($"Starting to look for folders and files in {config.RootDirectory}");
            var folders = Searcher.GetFolders(config.RootDirectory).ToList();
            Logger.Info($"Folders count: {folders.Count}");
            int count = 0;
            int folderIndex = 0;
            int folderTotal = folders.Count;
            foreach (var folder in folders.AsParallel())
            {
                try
                {
                    var directory = GetDirectory(folder);
                    var files = Searcher.GetFiles(folder).AsParallel().Select(filePath =>
                    {
                        var db = GetDbContextForThread();
                        var file = GetFile(db, filePath, directory);
                        return file.Id;
                    }).ToList();
                    lock (_dbLock)
                    {
                        folderIndex += 1;
                        count += files.Count;
                        Logger.Info($"Folder: {folderIndex}/{folderTotal}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
            SaveAllContexts();
            Logger.Info($"Files checked count: {count}");
        }

        private string GetCrc(string file)
        {
            var crc = string.Empty;
            try
            {
                //var fi = new FileInfo(file);
                crc = Checksum.GetChecksum(file);
                //Logger.Info($"CRC: {crc}, File: {fi.Name}");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
            }
            return crc;
        }
    }
}
