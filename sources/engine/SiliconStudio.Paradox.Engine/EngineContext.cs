using System;
using System.Collections.Generic;

using Paradox.Effects;
using Paradox.EntityModel;
using Paradox.Framework;
using Paradox.Framework.Configuration;
using Paradox.Framework.Graphics;
using Paradox.Framework.Serialization;
using Paradox.Framework.Serialization.Assets;
using Paradox.Framework.Serialization.Contents;
using Paradox.Framework.Serialization.Packages;
using Paradox.Framework.Shaders.Utilities;
using Paradox.Framework.IO;
using Paradox.Framework.MicroThreading;
using Paradox.Input;

namespace Paradox
{
    public enum GameState
    {
        Running,
        Editing,
        Saving,
    }

    public class EngineContext
    {
        public AssetManager AssetManager { get; internal set; }

        public PackageManager PackageManager { get; internal set; }

        public IRenderSystem RenderSystem { get; set; }

        public RenderContextBase RenderContext { get; internal set; }

        public Scheduler Scheduler { get; internal set; }

        public ScriptManager ScriptManager { get; internal set; }

        public IntPtr WindowHandle { get; set; }

        public SimpleComponentRegistry SimpleComponentRegistry { get; internal set; }

        public IInputManager InputManager { get; internal set; }

        public TimeSpan CurrentTime { get; set; }

        // TODO: Should we really make such dependence?
        public EntityManager EntityManager { get; internal set; }

        public RootDataContext DataContext { get; internal set; }

        public EngineContext()
        {
            RenderContext = new RenderContext();
            RenderContext.AddReference();
        }

        public void Initialize()
        {
            RenderContext.Initialize();

            RenderSystem = new RenderSystem();
            RenderSystem.AddReference();
            RenderSystem.Init(RenderContext);

            PackageManager = new PackageManager();
            AssetManager = new AssetManager(new AssetSerializerContextGenerator(PackageManager, ParameterContainerExtensions.DefaultSceneSerializer));

            Scheduler = new Scheduler();
            SimpleComponentRegistry = new SimpleComponentRegistry();
            SimpleComponentRegistry.AddReference();

            EntityManager = new EntityManager();
            EntityManager.AddReference();

            InputManager = RenderContext.CreateInputManager();

            ScriptManager = new ScriptManager(this);

            DataContext = new RootDataContext();
            
        }
        public void Stop()
        {
            // TODO: Check meshes are disposed properly in new system (with AddRenderQueue).
            SimpleComponentRegistry.Release();
            EntityManager.Release();
            RenderSystem.Release();
            RenderContext.Release();
        }
        public void Render()
        {
            //var time1 = st2.ElapsedTicks;

            // This lock will protect rendering from Effect creation (it modifies RenderPass collections)
            RenderSystem.Render(RenderContext.RenderPassEnumerators);

            //var time2 = st2.ElapsedTicks;

            //if (lastTickCount == 0)
            //    Console.WriteLine("Main Timings: Other {0} Render {1}", (float)time1 * 1000.0f / (float)Stopwatch.Frequency, (float)(time2 - time1) * 1000.0f / (float)Stopwatch.Frequency);
        }
    }
}
