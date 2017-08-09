// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Profiling;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Script component.
    /// </summary>
    [DataContract("ScriptComponent", Inherited = true)]
    [DefaultEntityComponentProcessor(typeof(ScriptProcessor), ExecutionMode = ExecutionMode.Runtime)]
    [Display(Expand = ExpandRule.Once)]
    [AllowMultipleComponents]
    [ComponentOrder(1000)]
    public abstract class ScriptComponent : EntityComponent
    {
        public const uint LiveScriptingMask = 128;

        /// <summary>
        /// The global profiling key for scripts. Activate/deactivate this key to activate/deactivate profiling for all your scripts.
        /// </summary>
        public static readonly ProfilingKey ScriptGlobalProfilingKey = new ProfilingKey("Script");

        private static readonly Dictionary<Type, ProfilingKey> ScriptToProfilingKey = new Dictionary<Type, ProfilingKey>();

        private ProfilingKey profilingKey;

        private IGraphicsDeviceService graphicsDeviceService;
        private Logger logger;

        protected ScriptComponent()
        {
        }

        internal void Initialize(IServiceRegistry registry)
        {
            Services = registry;

            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            Game = Services.GetSafeServiceAs<IGame>();
            Content = (ContentManager)Services.GetSafeServiceAs<IContentManager>();
            Input = Services.GetSafeServiceAs<InputManager>();
            Script = Services.GetSafeServiceAs<ScriptSystem>();
            SceneSystem = Services.GetSafeServiceAs<SceneSystem>();
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            Audio = Services.GetSafeServiceAs<AudioSystem>();
            SpriteAnimation = Services.GetSafeServiceAs<SpriteAnimationSystem>();
            GameProfiler = Services.GetSafeServiceAs<GameProfilingSystem>();
            DebugText = Services.GetSafeServiceAs<DebugTextSystem>();
        }

        /// <summary>
        /// Gets the profiling key to activate/deactivate profiling for the current script class.
        /// </summary>
        [DataMemberIgnore]
        public ProfilingKey ProfilingKey
        {
            get
            {
                if (profilingKey != null)
                    return profilingKey;

                var scriptType = GetType();
                if (!ScriptToProfilingKey.TryGetValue(scriptType, out profilingKey))
                {
                    profilingKey = new ProfilingKey(ScriptGlobalProfilingKey, scriptType.FullName);
                    ScriptToProfilingKey[scriptType] = profilingKey;
                }

                return profilingKey;
            }
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
        public ContentManager Content { get; private set; }

        [DataMemberIgnore]
        public GameProfilingSystem GameProfiler { get; private set; }

        [DataMemberIgnore]
        public GraphicsDevice GraphicsDevice => graphicsDeviceService?.GraphicsDevice;

        [DataMemberIgnore]
        public InputManager Input { get; private set; }

        [DataMemberIgnore]
        public ScriptSystem Script { get; private set; }

        [DataMemberIgnore]
        public SceneSystem SceneSystem { get; private set; }

        [DataMemberIgnore]
        public EffectSystem EffectSystem { get; private set; }

        [DataMemberIgnore]
        public DebugTextSystem DebugText { get; private set; }

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

        private int priority;

        /// <summary>
        /// The priority this script will be scheduled with (compared to other scripts).
        /// </summary>
        /// <userdoc>The execution priority for this script. It applies to async, sync and startup scripts. Lower values mean earlier execution.</userdoc>
        [DefaultValue(0)]
        [DataMember(10000)]
        public int Priority
        {
            get { return priority; }
            set { priority = value; PriorityUpdated(); }
        }

        /// <summary>
        /// Determines whether the script is currently undergoing live reloading.
        /// </summary>
        public bool IsLiveReloading { get; internal set; }


        /// <summary>
        /// Internal helper function called when <see cref="Priority"/> is changed.
        /// </summary>
        protected internal virtual void PriorityUpdated()
        {
        }

        /// <summary>
        /// Called when the script's update loop is canceled.
        /// </summary>
        public virtual void Cancel()
        {
        }
    }
}
