using Core.Backup.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Core.Backup.Parameters
{
    public static class ConfigManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConfigManager));
        private static Configuration _config;

        public static Configuration GetConfig()
        {
            try
            {
                if (_config != null)
                    return _config;
                using (var db = new BackupDbEntities())
                {
                    var param = db.Parameters.FirstOrDefault();
                    if (param == null)
                    {
                        _config = Configuration.Default();
                        var serialized = _config.SerializeObject();
                        db.Parameters.Add(new Parameter
                        {
                            Xml = serialized
                        });
                        db.SaveChanges();
                        return _config;
                    }
                    _config = param.Xml.Deserialize<Configuration>();
                    return _config;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return Configuration.Default();
            }
        }

        public static bool SetConfig(Configuration config)
        {
            try
            {
                _config = config;
                using (var db = new BackupDbEntities())
                {
                    var serialized = _config.SerializeObject();
                    var param = db.Parameters.FirstOrDefault();
                    if (param == null)
                    {
                        db.Parameters.Add(new Parameter
                        {
                            Xml = serialized
                        });
                    }
                    else
                        param.Xml = serialized;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return false;
            }
        }
    }
}
