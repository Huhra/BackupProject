using log4net;
using System.IO;

namespace BackupProject.Core
{
    public class Watcher
    {
        private static ILog Logger = LogManager.GetLogger(typeof(Watcher));

        public Watcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"C:\Users\etien\";
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter =
                //The name of the file.
                NotifyFilters.FileName |
                //The name of the directory.
                NotifyFilters.DirectoryName |
                //The attributes of the file or folder.
                NotifyFilters.Attributes |
                //The size of the file or folder.
                NotifyFilters.Size |
                //The date the file or folder last had anything written to it.
                NotifyFilters.LastWrite |
                //The date the file or folder was last opened.
                NotifyFilters.LastAccess |
                //The time the file or folder was created.
                NotifyFilters.CreationTime |
                //The security settings of the file or folder.
                NotifyFilters.Security;

            // Only watch text files.
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Logger.Info($"{e.ChangeType}: {e.OldFullPath}|{e.OldName} -> {e.FullPath}|{e.Name}");
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Logger.Info($"{e.ChangeType}: {e.FullPath}|{e.Name}");
        }
    }
}
