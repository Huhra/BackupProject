using Core.Backup.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Backup.Parameters
{
    public static class ConfigManager
    {
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
                        var serialized = Serializer.SerializeObject(_config);
                        db.Parameters.Add(new Parameter
                        {
                            Xml = serialized
                        });
                        db.SaveChanges();
                        return _config;
                    }
                    _config = Serializer.Deserialize<Configuration>(param.Xml);
                    return _config;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
