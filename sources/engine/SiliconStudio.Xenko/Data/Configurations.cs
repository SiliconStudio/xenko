using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Data
{
    public enum ConfigFilters
    {
        None,
        GPU,
        ModelName
    }

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
        public ConfigFilters SpecificFilter;

        [DataMember(30)]
        public Configuration Configuration;
    }
}
