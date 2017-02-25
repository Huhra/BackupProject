using System;
using System.Xml.Serialization;

namespace Core.Backup.Parameters
{
    [Serializable]
    public class Configuration
    {
        [XmlAttribute("AlreadyRan")]
        public bool AlreadyRan { get; set; }

        [XmlAttribute("RootDirectory")]
        public string RootDirectory { get; set; }

        [XmlElement("StartHour")]
        public int StartHour { get; set; }
    }
}
