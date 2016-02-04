using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Engine.Design
{
    [DataContract]
    public class PlatformConfigurations
    {
        [DataMemberIgnore]
        internal Game CurrentGame;

        [DataMember]
        internal List<ConfigurationOverride> Configurations = new List<ConfigurationOverride>();

        public T Get<T>() where T : Configuration, new()
        {
            //find default
            var config = Configurations.Where(x => x.Platforms == ConfigPlatforms.None).FirstOrDefault(x => x.Configuration.GetType() == typeof(T));
            
            //perform logic by platform and if required even gpu/cpu/specs

            var platform = ConfigPlatforms.None;
            switch (Platform.Type)
            {
                case PlatformType.Shared:
                    break;
                case PlatformType.Windows:
                    platform = ConfigPlatforms.Windows;
                    break;
                case PlatformType.WindowsPhone:
                    platform = ConfigPlatforms.WindowsPhone;
                    break;
                case PlatformType.WindowsStore:
                    platform = ConfigPlatforms.WindowsStore;
                    break;
                case PlatformType.Android:
                    platform = ConfigPlatforms.Android;
                    break;
                case PlatformType.iOS:
                    platform = ConfigPlatforms.iOS;
                    break;
                case PlatformType.Windows10:
                    platform = ConfigPlatforms.Windows10;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //find per platform if available
            var platformConfig = Configurations.Where(x => x.Platforms.HasFlag(platform) && x.SpecificFilter == string.Empty).FirstOrDefault(x => x.Configuration.GetType() == typeof(T));
            if (platformConfig != null)
            {
                config = platformConfig;
            }

            if (CurrentGame?.GraphicsDevice != null)
            {
                //find per specific renderer settings
                platformConfig = Configurations.Where(x => x.Platforms.HasFlag(platform) && new Regex(x.SpecificFilter, RegexOptions.IgnoreCase).IsMatch(CurrentGame.GraphicsDevice.RendererName)).FirstOrDefault(x => x.Configuration.GetType() == typeof(T));
                if (platformConfig != null)
                {
                    config = platformConfig;
                }

                //find per specific device settings
                platformConfig = Configurations.Where(x => x.Platforms.HasFlag(platform) && new Regex(x.SpecificFilter, RegexOptions.IgnoreCase).IsMatch(CurrentGame.FullPlatformName)).FirstOrDefault(x => x.Configuration.GetType() == typeof(T));
                if (platformConfig != null)
                {
                    config = platformConfig;
                }
            }

            if (config == null)
            {
                return new T();
            }

            return (T)config.Configuration;
        }
    }
}