using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Data
{
    public enum ConfigPlatforms
    {
        Default,
        Windows,
        WindowsStore,
        iOS,
        [Display("iOS High")]
        iOS_High,
        [Display("iOS Mid")]
        iOS_Mid,
        [Display("iOS Low")]
        iOS_Low,
        Android,
        [Display("Android High")]
        Android_High,
        [Display("Android Mid")]
        Android_Mid,
        [Display("Android Low")]
        Android_Low
    }

    public interface IConfiguration
    {
        ConfigPlatforms Platform { get; set; }
    }

    [DataContract("IntegerConfiguration")]
    [Display("Integer Value")]
    public class IntegerConfiguration : IConfiguration
    {
        [DataMember(0)]
        public ConfigPlatforms Platform { get; set; }

        [DataMember(10)]
        public int TestValue;
    }

    [DataContract]
    public class ConfigurationContainer
    {
        [DataMember(10)]
        [NotNullItems]
        public List<IConfiguration> Configurations { get; } = new List<IConfiguration>();
    }
}
