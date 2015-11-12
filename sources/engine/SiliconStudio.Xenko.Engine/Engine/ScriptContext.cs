// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine
{
    public interface IScriptContext
    {
        IServiceRegistry Services { get; }

        IGame Game { get; }

        AssetManager Asset { get; }

        GraphicsDevice GraphicsDevice { get; }

        InputManager Input { get; }

        ScriptSystem Script { get; }

        SceneSystem SceneSystem { get; }

        EffectSystem EffectSystem { get; }

        AudioSystem Audio { get; }

        SpriteAnimationSystem SpriteAnimation { get; }
    }

    [DataContract("ScriptContext")]
    public abstract class ScriptContext : ComponentBase, IScriptContext
    {
        private IGraphicsDeviceService graphicsDeviceService;
        private Logger logger;

        protected ScriptContext()
        {
        }

        protected ScriptContext(IServiceRegistry registry)
        {
            Initialize(registry);
        }

        internal void Initialize(IServiceRegistry registry)
        {
            Services = registry;

            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            Game = Services.GetSafeServiceAs<IGame>();
            Asset = (AssetManager)Services.GetSafeServiceAs<IAssetManager>();
            Input = Services.GetSafeServiceAs<InputManager>();
            Script = Services.GetSafeServiceAs<ScriptSystem>();
            SceneSystem = Services.GetSafeServiceAs<SceneSystem>();
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            Audio = Services.GetSafeServiceAs<AudioSystem>();
            SpriteAnimation = Services.GetSafeServiceAs<SpriteAnimationSystem>();
        }

        [DataMember(-10), Display(Browsable = false)]
        public override Guid Id
        {
            get { return base.Id; }
            set { base.Id = value; }
        }

        [DataMemberIgnore]
        public AudioSystem Audio { get; private set; }

        [DataMemberIgnore]
        public SpriteAnimationSystem SpriteAnimation { get; private set; }

        [DataMemberIgnore]
        public IServiceRegistry Services { get; private set; }

        [DataMemberIgnore]
        public IGame Game { get; private set; }

        [DataMemberIgnore]
        public AssetManager Asset { get; private set; }

        [DataMemberIgnore]
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDeviceService.GraphicsDevice;
            }
        }

        [DataMemberIgnore]
        public InputManager Input { get; private set; }

        [DataMemberIgnore]
        public ScriptSystem Script { get; private set; }

        [DataMemberIgnore]
        public SceneSystem SceneSystem { get; private set; }

        [DataMemberIgnore]
        public EffectSystem EffectSystem { get; private set; }

        protected override void Destroy()
        {
        }

        [DataMemberIgnore]
        protected Logger Log
        {
            get
            {
                if (logger != null)
                {
                    return logger;
                }

                var className = GetType().FullName;
                logger = GlobalLogger.GetLogger(className);
                return logger;
            }
        }
    }
}