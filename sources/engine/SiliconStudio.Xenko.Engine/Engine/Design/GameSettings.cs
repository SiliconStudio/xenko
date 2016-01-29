// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Data;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine.Design
{
    [DataContract]
    public class PlatformConfigurations
    {
        [DataMember]
        internal Dictionary<Type, IConfiguration> Configurations = new Dictionary<Type, IConfiguration>();

        public T Get<T>() where T : IConfiguration, new()
        {
            IConfiguration configuration;
            if (Configurations.TryGetValue(typeof(T), out configuration))
            {
                return (T)configuration;
            }

            return new T();
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
