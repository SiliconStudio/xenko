// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

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
    public interface IScriptContext
    {
        IServiceRegistry Services { get; }

        object Parameter { get; set; }

        IGame Game { get; }

        AssetManager Asset { get; }

        GraphicsDevice GraphicsDevice { get; }

        InputManager Input { get; }

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

    [DataContract("ScriptContext")]
    public abstract class ScriptContext : ComponentBase, IScriptContext
    {
        private IGraphicsDeviceService graphicsDeviceService;

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
            UI = Services.GetSafeServiceAs<UISystem>();
        }

        [DataMemberIgnore]
        public AudioSystem Audio { get; private set; }

        [DataMemberIgnore]
        public IServiceRegistry Services { get; private set; }

        public object Parameter { get; set; }

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
        public UISystem UI { get; private set; }

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
    }
}