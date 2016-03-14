using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Data
{
    [Flags]
    public enum ConfigPlatforms
    {
        None = 0,
        Windows = 1 << PlatformType.Windows,
        Windows10 = 1 << PlatformType.Windows10,
        WindowsStore = 1 << PlatformType.WindowsStore,
        WindowsPhone = 1 << PlatformType.WindowsPhone,
        iOS = 1 << PlatformType.iOS,
        Android = 1 << PlatformType.Android,
        Linux = 1 << PlatformType.Linux
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
        [InlineProperty]
        public ConfigPlatforms Platforms;

        [DataMember(20)]
        [DefaultValue(-1)]
        public int SpecificFilter = -1;

        [DataMember(30)]
        public Configuration Configuration;
    }
}
