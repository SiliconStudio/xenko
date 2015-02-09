// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI;

namespace SiliconStudio.Paradox
{
    public interface IScriptContext : IVirtualResolution
    {
        IServiceRegistry Services { get; }

        object Parameter { get; set; }

        IGame Game { get; }

        AssetManager Asset { get; }

        GraphicsDevice GraphicsDevice { get; }

        InputManager Input { get; }

        EntitySystem Entities { get; }

        ScriptSystem Script { get; }

        SceneSystem SceneSystem { get; }

        EffectSystem EffectSystem { get; }

        AudioSystem Audio { get; }

        UISystem UI { get; }
    }

    public interface IScript : IScriptContext
    {
        Task Execute();
    }

    public abstract class ScriptContext : ComponentBase, IScriptContext
    {
        private readonly IGraphicsDeviceService graphicsDeviceService;

        private readonly IVirtualResolution virtualResolutionProvider;

        protected ScriptContext(IServiceRegistry registry)
        {
            Services = registry;

            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();

            Game = Services.GetSafeServiceAs<IGame>();
            virtualResolutionProvider = Services.GetSafeServiceAs<IVirtualResolution>();
            Asset = (AssetManager)Services.GetSafeServiceAs<IAssetManager>();
            Input = Services.GetSafeServiceAs<InputManager>();
            Entities = Services.GetSafeServiceAs<EntitySystem>();
            Script = Services.GetSafeServiceAs<ScriptSystem>();
            SceneSystem = Services.GetSafeServiceAs<SceneSystem>();
            EffectSystem = Services.GetSafeServiceAs<EffectSystem>();
            Audio = Services.GetSafeServiceAs<AudioSystem>();
            UI = Services.GetSafeServiceAs<UISystem>();
        }

        public AudioSystem Audio { get; private set; }

        public IServiceRegistry Services { get; private set; }

        public object Parameter { get; set; }

        public IGame Game { get; private set; }

        public AssetManager Asset { get; private set; }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDeviceService.GraphicsDevice;
            }
        }

        public UISystem UI { get; private set; }

        public InputManager Input { get; private set; }

        public EntitySystem Entities { get; private set; }

        public ScriptSystem Script { get; private set; }

        public SceneSystem SceneSystem { get; private set; }

        public EffectSystem EffectSystem { get; private set; }

        protected override void Destroy()
        {
        }

        public Vector3 VirtualResolution 
        { 
            get { return virtualResolutionProvider.VirtualResolution; }
            set { virtualResolutionProvider.VirtualResolution = value; }
        }

        public event EventHandler<EventArgs> VirtualResolutionChanged
        {
            add { virtualResolutionProvider.VirtualResolutionChanged += value;}
            remove { virtualResolutionProvider.VirtualResolutionChanged -= value; }
        }
    }
}