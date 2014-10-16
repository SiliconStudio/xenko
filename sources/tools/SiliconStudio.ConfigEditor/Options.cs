using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SiliconStudio.Paradox.ConfigEditor
{
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Options
    {
        private string paradoxPath;
        [XmlElement]
        public string ParadoxPath
        {
            get { return paradoxPath; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Invalid 'ParadoxPath' property value");
                paradoxPath = value;
            }
        }

        [XmlElement]
        public string ParadoxConfigFilename { get; set; }

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Options));

        public static Options Load()
        {
            try
            {
                var filename = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, ".config");
                return (Options)serializer.Deserialize(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }
            catch
            {
                return null;
            }
        }

        public void Save()
        {
            var filename = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, ".config");
            serializer.Serialize(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), this);
        }
    }
}
