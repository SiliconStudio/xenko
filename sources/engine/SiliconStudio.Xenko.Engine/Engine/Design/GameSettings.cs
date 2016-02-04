// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Graphics;

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

    /// <summary>
    /// Stores some default parameters for the game.
    /// </summary>
    [DataContract("GameSettings")]
    [ContentSerializer(typeof(DataContentSerializer<GameSettings>))]
    public sealed class GameSettings
    {
        public const string AssetUrl = "GameSettings";

        public GameSettings()
        {
            EffectCompilation = EffectCompilationMode.Local;
        }

        public Guid PackageId { get; set; }

        public string PackageName { get; set; }

        public string DefaultSceneUrl { get; set; }

        public int DefaultBackBufferWidth { get; set; }

        public int DefaultBackBufferHeight { get; set; }

        public GraphicsProfile DefaultGraphicsProfileUsed { get; set; }

        public ColorSpace ColorSpace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether effect compile should be allowed, and if yes, should it be done locally (if possible) or remotely?
        /// </summary>
        public EffectCompilationMode EffectCompilation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether effect compile (local or remote) should be recorded and sent to effect compile server for GameStudio notification.
        /// </summary>
        public bool RecordUsedEffects { get; set; }

        /// <summary>
        /// Gets or sets configuration for the actual running platform as compiled during build
        /// </summary>
        public PlatformConfigurations Configurations { get; set; }
    }
}
