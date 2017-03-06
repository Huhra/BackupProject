using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using log4net;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Core.Backup.Parameters
{
    [Serializable]
    public class Configuration
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigManager));

        [XmlAttribute("RootDirectory")]
        public string RootDirectory { get; set; }

        [XmlAttribute("RemoteDirectory")]
        public string RemoteDirectory { get; set; }

        [XmlElement("StartHour")]
        public int StartHour { get; set; }
        [XmlAttribute("DeleteFiles")]
        public bool DeleteFiles { get; set; }

        public static Configuration Default()
        {
            return new Configuration
            {
                RootDirectory = Environment.CurrentDirectory,
                RemoteDirectory = @"\\remote-computer\Folder\",
                StartHour = 22,
                DeleteFiles = false
            };
        }

        public IList<TestConfigResult> TestConfiguration()
        {
            var result = new List<TestConfigResult>();
            var tmpPath = Path.GetTempFileName();
            try
            {
                if (System.IO.Directory.Exists(RootDirectory))
                    result.Add(TestConfigResult.RootDirectoryVisible);

                using (var file = System.IO.File.OpenWrite(tmpPath))
                using (var writer = new StreamWriter(file))
                    writer.WriteLine("This is a test file, it can be deleted");

                if (System.IO.Directory.Exists(RemoteDirectory))
                    result.Add(TestConfigResult.RemoteDirectoryVisible);

                try
                {
                    var fileName = Path.GetFileName(tmpPath);
                    var remotePath = Path.Combine(RemoteDirectory, fileName);
                    System.IO.File.Copy(tmpPath, remotePath);
                    result.Add(TestConfigResult.WriteRights);

                    System.IO.File.Delete(remotePath);
                    result.Add(TestConfigResult.DeleteRights);
                }
                catch (Exception remoteException)
                {
                    Logger.Warn($"Error during file copy / delete: {remoteException.Message}");
                }
            }
            catch (Exception e)
            {
                result.Add(TestConfigResult.UnknownError);
                Logger.Warn($"Error while testing configuration: {e.Message}");
            }
            if (System.IO.File.Exists(tmpPath))
                System.IO.File.Delete(tmpPath);
            return result;
        }

        public enum TestConfigResult
        {
            RootDirectoryVisible,
            RemoteDirectoryVisible,
            WriteRights,
            DeleteRights,
            UnknownError
        }
    }
}
