using Core.Backup;
using Core.Backup.FileSearcher;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console.Backup
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            System.Console.WriteLine("START");
            var backup = new BackupWorker();
            DoJob(backup);
            System.Console.WriteLine("DONE");
            System.Console.ReadLine();
        }

        private static async void DoJob(BackupWorker backup)
        {
            await Task.Yield();
            var start = DateTime.Now.ToLongTimeString();
            await backup.DoBackup();
            var end = DateTime.Now.ToLongTimeString();
            System.Console.WriteLine($"CheckDifferentFiles: {start}");
            System.Console.WriteLine($"End: {end}");
        }
    }
}
