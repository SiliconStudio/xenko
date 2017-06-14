// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Profiling;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Fonts;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.Streaming;
using SiliconStudio.Xenko.VirtualReality;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Main Game class system.
    /// </summary>
    public class Game : GameBase, ISceneRendererContext
    {
        /// <summary>
        /// Static event that will be fired when a game is initialized
        /// </summary>
        public static event EventHandler GameStarted;

        /// <summary>
        /// Static event that will be fired when a game is destroyed
        /// </summary>
        public static event EventHandler GameDestroyed;

        private readonly GameFontSystem gameFontSystem;

        private readonly LogListener logListener;

        private DatabaseFileProvider databaseFileProvider;

        /// <summary>
        /// Readonly game settings as defined in the GameSettings asset
        /// Please note that it will be populated during initialization
        /// It will be ok to read them after the GameStarted event or after initialization
        /// </summary>
        public GameSettings Settings { get; private set; } // for easy transfer from PrepareContext to Initialize

        /// <summary>
        /// Gets the graphics device manager.
        /// </summary>
        /// <value>The graphics device manager.</value>
        public GraphicsDeviceManager GraphicsDeviceManager { get; internal set; }

        /// <summary>
        /// Gets the script system.
        /// </summary>
        /// <value>The script.</value>
        public ScriptSystem Script { get; internal set; }

        /// <summary>
        /// Gets the input manager.
        /// </summary>
        /// <value>The input.</value>
        public InputManager Input { get; internal set; }

        /// <summary>
        /// Gets the scene system.
        /// </summary>
        /// <value>The scene system.</value>
        public SceneSystem SceneSystem { get; private set; }

        /// <summary>
        /// Gets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }
        
        /// <summary>
        /// Gets the streaming system.
        /// </summary>
        /// <value>The streaming system.</value>
        public StreamingManager Streaming { get; private set; }

        /// <summary>
        /// Gets the audio system.
        /// </summary>
        /// <value>The audio.</value>
        public AudioSystem Audio { get; private set; }

        /// <summary>
        /// Gets the sprite animation system.
        /// </summary>
        /// <value>The sprite animation system.</value>
        public SpriteAnimationSystem SpriteAnimation { get; private set; }

        /// <summary>
        /// Gets the game profiler system
        /// </summary>
        public DebugConsoleSystem DebugConsoleSystem { get; private set; }

        /// <summary>
        /// Gets the game profiler system
        /// </summary>
        public GameProfilingSystem ProfilerSystem { get; private set; }

        /// <summary>
        /// Gets the VR Device System
        /// </summary>
        public VRDeviceSystem VRDeviceSystem { get; private set; }

        /// <summary>
        /// Gets the font system.
        /// </summary>
        /// <value>The font system.</value>
        /// <exception cref="System.InvalidOperationException">The font system is not initialized yet</exception>
        public IFontFactory Font
        {
            get
            {
                if (gameFontSystem.FontSystem == null)
                    throw new InvalidOperationException("The font system is not initialized yet");

                return gameFontSystem.FontSystem;
            }
        }

        /// <summary>
        /// Gets or sets the console log mode. See remarks.
        /// </summary>
        /// <value>The console log mode.</value>
        /// <remarks>
        /// Defines how the console will be displayed when running the game. By default, on Windows, It will open only on debug
        /// if there are any messages logged.
        /// </remarks>
        public ConsoleLogMode ConsoleLogMode
        {
            get
            {
                var consoleLogListener = logListener as ConsoleLogListener;
                return consoleLogListener != null ? consoleLogListener.LogMode : default(ConsoleLogMode);
            }
            set
            {
                var consoleLogListener = logListener as ConsoleLogListener;
                if (consoleLogListener != null)
                {
                    consoleLogListener.LogMode = value;
                }
            }            
        }

        /// <summary>
        /// Gets or sets the default console log level.
        /// </summary>
        /// <value>The console log level.</value>
        public LogMessageType ConsoleLogLevel
        {
            get
            {
                var consoleLogListener = logListener as ConsoleLogListener;
                return consoleLogListener != null ? consoleLogListener.LogLevel : default(LogMessageType);
            }
            set
            {
                var consoleLogListener = logListener as ConsoleLogListener;
                if (consoleLogListener != null)
                {
                    consoleLogListener.LogLevel = value;
                }
            }
        }

        /// <summary>
        /// Automatically initializes game settings like default scene, resolution, graphics profile.
        /// </summary>
        public bool AutoLoadDefaultSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        public Game()
        {
            // Register the logger backend before anything else
            logListener = GetLogListener();

            if (logListener != null)
                GlobalLogger.GlobalMessageLogged += logListener;

            // Create all core services, except Input which is created during `Initialize'.
            // Registration takes place in `Initialize'.
            Script = new ScriptSystem(Services);
            Services.AddService(typeof(ScriptSystem), Script);

            SceneSystem = new SceneSystem(Services);
            Services.AddService(typeof(SceneSystem), SceneSystem);

            Streaming = new StreamingManager(Services);

            Audio = new AudioSystem(Services);
            Services.AddService(typeof(AudioSystem), Audio);
            Services.AddService(typeof(IAudioEngineProvider), Audio);

            gameFontSystem = new GameFontSystem(Services);
            Services.AddService(typeof(FontSystem), gameFontSystem.FontSystem);
            Services.AddService(typeof(IFontFactory), gameFontSystem.FontSystem);

            SpriteAnimation = new SpriteAnimationSystem(Services);
            Services.AddService(typeof(SpriteAnimationSystem), SpriteAnimation);

            DebugConsoleSystem = new DebugConsoleSystem(Services);
            Services.AddService(typeof(DebugConsoleSystem), DebugConsoleSystem);

            ProfilerSystem = new GameProfilingSystem(Services);
            Services.AddService(typeof(GameProfilingSystem), ProfilerSystem);

            VRDeviceSystem = new VRDeviceSystem(Services);
            Services.AddService(typeof(VRDeviceSystem), VRDeviceSystem);

            Content.Serializer.LowLevelSerializerSelector = ParameterContainerExtensions.DefaultSceneSerializerSelector;

            // Creates the graphics device manager
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Services.AddService(typeof(IGraphicsDeviceManager), GraphicsDeviceManager);
            Services.AddService(typeof(IGraphicsDeviceService), GraphicsDeviceManager);

            AutoLoadDefaultSettings = true;
        }

        /// <inheritdoc/>
        protected override void Destroy()
        {
            OnGameDestroyed(this);

            DestroyAssetDatabase();

            base.Destroy();
            
            if (logListener != null)
                GlobalLogger.GlobalMessageLogged -= logListener;
        }

        /// <inheritdoc/>
        protected override void PrepareContext()
        {
            base.PrepareContext();

            // Init assets
            if (Context.InitializeDatabase)
            {
                databaseFileProvider = InitializeAssetDatabase();

                var renderingSettings = new RenderingSettings();
                if (Content.Exists(GameSettings.AssetUrl))
                {
                    Settings = Content.Load<GameSettings>(GameSettings.AssetUrl);

                    renderingSettings = Settings.Configurations.Get<RenderingSettings>();

                    // Set ShaderProfile even if AutoLoadDefaultSettings is false (because that is what shaders in effect logs are compiled against, even if actual instantiated profile is different)
                    if (renderingSettings.DefaultGraphicsProfile > 0)
                    {
                        var deviceManager = (GraphicsDeviceManager)graphicsDeviceManager;
                        if (!deviceManager.ShaderProfile.HasValue)
                            deviceManager.ShaderProfile = renderingSettings.DefaultGraphicsProfile;
                    }
                }

                // Load several default settings
                if (AutoLoadDefaultSettings)
                {
                    var deviceManager = (GraphicsDeviceManager)graphicsDeviceManager;
                    if (renderingSettings.DefaultGraphicsProfile > 0)
                    {
                        deviceManager.PreferredGraphicsProfile = new[] { renderingSettings.DefaultGraphicsProfile };
                    }

                    if (renderingSettings.DefaultBackBufferWidth > 0) deviceManager.PreferredBackBufferWidth = renderingSettings.DefaultBackBufferWidth;
                    if (renderingSettings.DefaultBackBufferHeight > 0) deviceManager.PreferredBackBufferHeight = renderingSettings.DefaultBackBufferHeight;

                    deviceManager.PreferredColorSpace = renderingSettings.ColorSpace;
                    SceneSystem.InitialSceneUrl = Settings?.DefaultSceneUrl;
                    SceneSystem.InitialGraphicsCompositorUrl = Settings?.DefaultGraphicsCompositorUrl;
                    SceneSystem.SplashScreenUrl = Settings?.SplashScreenUrl;
                    SceneSystem.SplashScreenColor = Settings?.SplashScreenColor ?? Color4.Black;
                }
            }
        }

        public override void ConfirmRenderingSettings(bool gameCreation)
        {
            if (!AutoLoadDefaultSettings) return;

            var renderingSettings = Settings?.Configurations.Get<RenderingSettings>();
            if (renderingSettings == null) return;

            var deviceManager = (GraphicsDeviceManager)graphicsDeviceManager;

            if (gameCreation)
            {
                //execute the following steps only when the game is still at creation stage

                deviceManager.PreferredGraphicsProfile = Context.RequestedGraphicsProfile = new[] { renderingSettings.DefaultGraphicsProfile };

                //if our device height is actually smaller then requested we use the device one
                deviceManager.PreferredBackBufferHeight = Context.RequestedHeight = Math.Min(renderingSettings.DefaultBackBufferHeight, Window.ClientBounds.Height);
                //if our device width is actually smaller then requested we use the device one
                deviceManager.PreferredBackBufferWidth = Context.RequestedWidth = Math.Min(renderingSettings.DefaultBackBufferWidth, Window.ClientBounds.Width);
            }

            //these might get triggered even during game runtime, resize, orientation change

            if (renderingSettings.AdaptBackBufferToScreen)
            {
                var deviceAr = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;

                if (renderingSettings.DefaultBackBufferHeight > renderingSettings.DefaultBackBufferWidth)
                {
                    deviceManager.PreferredBackBufferWidth = Context.RequestedWidth = (int)(deviceManager.PreferredBackBufferHeight * deviceAr);
                }
                else
                { 
                    deviceManager.PreferredBackBufferHeight = Context.RequestedHeight = (int)(deviceManager.PreferredBackBufferWidth / deviceAr);
                }
            }
        }

        protected override void Initialize()
        {
            // ---------------------------------------------------------
            // Add common GameSystems - Adding order is important
            // (Unless overriden by gameSystem.UpdateOrder)
            // ---------------------------------------------------------

            // Add the input manager
            // Add it first so that it can obtained by the UI system
            Input = new InputManager(Services);
            Services.AddService(typeof(InputManager), Input);
            GameSystems.Add(Input);

            // Initialize the systems
            base.Initialize();

            // Add the scheduler system
            // - Must be after Input, so that scripts are able to get latest input
            // - Must be before Entities/Camera/Audio/UI, so that scripts can apply
            // changes in the same frame they will be applied
            GameSystems.Add(Script);

            // Add the Font system
            GameSystems.Add(gameFontSystem);

            //Add the sprite animation System
            GameSystems.Add(SpriteAnimation);

            GameSystems.Add(DebugConsoleSystem);
            GameSystems.Add(ProfilerSystem);

            EffectSystem = new EffectSystem(Services);
            Services.AddService(typeof(EffectSystem), EffectSystem);

            // If requested in game settings, compile effects remotely and/or notify new shader requests
            if (Settings != null)
            {
                EffectSystem.Compiler = EffectSystem.CreateEffectCompiler(EffectSystem, Settings.PackageId, Settings.EffectCompilation, Settings.RecordUsedEffects);
            }

            GameSystems.Add(EffectSystem);

            GameSystems.Add(Streaming);
            GameSystems.Add(SceneSystem);
            
            // Add the Audio System
            GameSystems.Add(Audio);

            // Add the VR System
            GameSystems.Add(VRDeviceSystem);

            // TODO: data-driven?
            Content.Serializer.RegisterSerializer(new ImageSerializer());

            OnGameStarted(this);
        }

        internal static DatabaseFileProvider InitializeAssetDatabase()
        {
            using (Profiler.Begin(GameProfilingKeys.ObjectDatabaseInitialize))
            {
                // Create and mount database file system
                var objDatabase = ObjectDatabase.CreateDefaultDatabase();
                
                // Only set a mount path if not mounted already
                var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
                var result = new DatabaseFileProvider(objDatabase, mountPath);

                ContentManager.GetFileProvider = () => result;

                return result;
            }
        }

        private void DestroyAssetDatabase()
        {
            if (databaseFileProvider != null)
            {
                ContentManager.GetFileProvider = null;
                databaseFileProvider.Dispose();
                databaseFileProvider = null;
            }
        }

        protected override void EndDraw(bool present)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            // Allow to make a screenshot using CTRL+c+F12 (on release of F12)
            if (Input.HasKeyboard)
            {
                if (Input.IsKeyDown(Keys.LeftCtrl)
                    && Input.IsKeyDown(Keys.C)
                    && Input.IsKeyReleased(Keys.F12))
                {
                    var currentFilePath = Assembly.GetEntryAssembly().Location;
                    var timeNow = DateTime.Now.ToString("s", CultureInfo.InvariantCulture).Replace(':', '_');
                    var newFileName = Path.Combine(
                        Path.GetDirectoryName(currentFilePath),
                        Path.GetFileNameWithoutExtension(currentFilePath) + "_" + timeNow + ".png");

                    Console.WriteLine("Saving screenshot: {0}", newFileName);

                    using (var stream = System.IO.File.Create(newFileName))
                    {
                        GraphicsDevice.Presenter.BackBuffer.Save(GraphicsContext.CommandList, stream, ImageFileType.Png);
                    }
                }
            }
#endif
            base.EndDraw(present);
        }

        /// <summary>
        /// Loads the content.
        /// </summary>
        protected virtual Task LoadContent()
        {
            return Task.FromResult(true);
        }

        internal override void LoadContentInternal()
        {
            base.LoadContentInternal();
            Script.AddTask(LoadContent);
        }
        protected virtual LogListener GetLogListener()
        {
            return new ConsoleLogListener();
        }

        private static void OnGameStarted(Game game)
        {
            GameStarted?.Invoke(game, null);
        }

        private static void OnGameDestroyed(Game game)
        {
            GameDestroyed?.Invoke(game, null);
        }
    }
}
