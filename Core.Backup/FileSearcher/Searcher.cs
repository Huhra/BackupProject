using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.FileSearcher
{
    public static class Searcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Searcher));

        public static IEnumerable<string> GetFolders(string folder)
        {
            var info = new DirectoryInfo(folder);
            yield return folder;
            string[] directories = new string[0];
            try
            {
                directories = System.IO.Directory.GetDirectories(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            foreach (var dir in directories.SelectMany(d => GetFolders(d)))
                yield return dir;
        }

        public static IEnumerable<string> GetFiles(string folder)
        {
            string[] files = new string[0];
            try
            {
                files = System.IO.Directory.GetFiles(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                //if (!searchParameters.IgnoredFiles.Contains(fileInfo.Name))
                yield return file;
            }
        }

        public static IEnumerable<string> GetAllFiles(string rootFolder)
        {
            var folders = GetFolders(rootFolder);
            foreach (var file in folders.SelectMany(folder => GetFiles(folder)))
                yield return file;
        }
    }
}
