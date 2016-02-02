using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Data
{
    [Flags]
    public enum ConfigPlatforms
    {
        None = 0,
        Windows = 1 << 0,
        Windows10 = 1 << 1,
        WindowsStore = 1 << 2,
        WindowsPhone = 1 << 3,
        iOS = 1 << 4,
        Android = 1 << 5
    }

    [DataContract]
    public abstract class Configuration
    {
    }

    [DataContract]
    public class ConfigurationOverride
    {
        [DataMember(10)]
        public ConfigPlatforms Platforms;

        [DataMember(20)]
        public string SpecificFilter;

        [DataMember(30)]
        public Configuration Configuration;
    }
}
