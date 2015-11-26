// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Fonts;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Main Game class system.
    /// </summary>
    public class Game : GameBase
    {
        private readonly GameFontSystem gameFontSystem;

        private readonly LogListener logListener;
        private GameSettings gameSettings; // for easy transfer from PrepareContext to Initialize

        /// <summary>
        /// Gets the graphics device manager.
        /// </summary>
        /// <value>The graphics device manager.</value>
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

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
        /// Gets the audio system.
        /// </summary>
        /// <value>The audio.</value>
        public AudioSystem Audio { get; private set; }

        /// <summary>
        /// Gets the UI system.
        /// </summary>
        /// <value>The UI.</value>
        protected UISystem UI { get; private set; }

        /// <summary>
        /// Gets the sprite animation system.
        /// </summary>
        /// <value>The sprite animation system.</value>
        public SpriteAnimationSystem SpriteAnimation { get; private set; }

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

            // Create and register all core services
            Input = new InputManager(Services);
            Script = new ScriptSystem(Services);
            SceneSystem = new SceneSystem(Services);
            Audio = new AudioSystem(Services);
            UI = new UISystem(Services);
            gameFontSystem = new GameFontSystem(Services);
            SpriteAnimation = new SpriteAnimationSystem(Services);

            // ---------------------------------------------------------
            // Add common GameSystems - Adding order is important 
            // (Unless overriden by gameSystem.UpdateOrder)
            // ---------------------------------------------------------

            // Add the input manager
            GameSystems.Add(Input);

            // Add the scheduler system
            // - Must be after Input, so that scripts are able to get latest input
            // - Must be before Entities/Camera/Audio/UI, so that scripts can apply 
            // changes in the same frame they will be applied
            GameSystems.Add(Script);

            // Add the UI System
            GameSystems.Add(UI);

            // Add the Audio System
            GameSystems.Add(Audio);

            // Add the Font system
            GameSystems.Add(gameFontSystem);

            //Add the sprite animation System
            GameSystems.Add(SpriteAnimation);

            Asset.Serializer.LowLevelSerializerSelector = ParameterContainerExtensions.DefaultSceneSerializerSelector;

            // Creates the graphics device manager
            GraphicsDeviceManager = new GraphicsDeviceManager(this);

            AutoLoadDefaultSettings = true;
        }

        protected override void Destroy()
        {
            base.Destroy();
            
            if (logListener != null)
                GlobalLogger.GlobalMessageLogged -= logListener;
        }

        protected internal override void PrepareContext()
        {
            base.PrepareContext();

            // Init assets
            if (Context.InitializeDatabase)
            {
                InitializeAssetDatabase();

                if (Asset.Exists(GameSettings.AssetUrl))
                {
                    gameSettings = Asset.Load<GameSettings>(GameSettings.AssetUrl);

                    // Set ShaderProfile even if AutoLoadDefaultSettings is false (because that is what shaders in effect logs are compiled against, even if actual instantiated profile is different)
                    if (gameSettings.DefaultGraphicsProfileUsed > 0)
                    {
                        var deviceManager = (GraphicsDeviceManager)graphicsDeviceManager;
                        deviceManager.ShaderProfile = gameSettings.DefaultGraphicsProfileUsed;
                    }
                }

                // Load several default settings
                if (AutoLoadDefaultSettings)
                {
                    var deviceManager = (GraphicsDeviceManager)graphicsDeviceManager;
                    if (gameSettings.DefaultGraphicsProfileUsed > 0)
                    {
                        deviceManager.PreferredGraphicsProfile = new[] { gameSettings.DefaultGraphicsProfileUsed };
                    }
                    if (gameSettings.DefaultBackBufferWidth > 0) deviceManager.PreferredBackBufferWidth = gameSettings.DefaultBackBufferWidth;
                    if (gameSettings.DefaultBackBufferHeight > 0) deviceManager.PreferredBackBufferHeight = gameSettings.DefaultBackBufferHeight;
                    deviceManager.PreferredColorSpace = gameSettings.ColorSpace;
                    SceneSystem.InitialSceneUrl = gameSettings.DefaultSceneUrl;
                }
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            EffectSystem = new EffectSystem(Services);

            // If requested in game settings, compile effects remotely and/or notify new shader requests
            if (gameSettings != null)
            {
                EffectSystem.Compiler = EffectSystem.CreateEffectCompiler(EffectSystem, gameSettings.PackageId, gameSettings.EffectCompilation, gameSettings.RecordUsedEffects);
            }

            GameSystems.Add(EffectSystem);

            GameSystems.Add(SceneSystem);

            // TODO: data-driven?
            Asset.Serializer.RegisterSerializer(new ImageSerializer());
            Asset.Serializer.RegisterSerializer(new SoundEffectSerializer(Audio.AudioEngine));
            Asset.Serializer.RegisterSerializer(new SoundMusicSerializer(Audio.AudioEngine));

            // enable multi-touch by default
            Input.MultiTouchEnabled = true;
        }

        internal static void InitializeAssetDatabase()
        {
            using (Profiler.Begin(GameProfilingKeys.ObjectDatabaseInitialize))
            {
                // Create and mount database file system
                var objDatabase = new ObjectDatabase("/data/db", "index", "/local/db");
                
                // Only set a mount path if not mounted already
                var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
                var databaseFileProvider = new DatabaseFileProvider(objDatabase, mountPath);

                AssetManager.GetFileProvider = () => databaseFileProvider;
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
                        GraphicsDevice.BackBuffer.Save(stream, ImageFileType.Png);
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
    }
}