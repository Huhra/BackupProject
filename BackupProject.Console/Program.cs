using BackupProject.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupProject.Console
{
    class Program
    {
        private static ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            Logger.Info("Starting program");
            var watcher = new Watcher();
            Logger.Info("The watch has started");
            System.Console.ReadLine();
        }
    }
}
