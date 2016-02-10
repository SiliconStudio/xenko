using System;
using System.ComponentModel;
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
        Android = 1 << 5,
        Linux = 1 << 6
    }

    [DataContract]
    public abstract class Configuration
    {
        [DataMemberIgnore]
        public bool OfflineOnly { get; protected set; }
    }

    [DataContract]
    public class ConfigurationOverride
    {
        [DataMember(10)]
        public ConfigPlatforms Platforms;

        [DataMember(20)]
        [DefaultValue(-1)]
        public int SpecificFilter = -1;

        [DataMember(30)]
        public Configuration Configuration;
    }
}
