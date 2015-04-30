// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Manage a collection of entities within a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneInstance : EntityManager
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SceneInstance");

        /// <summary>
        /// A property key to get the current scene from the <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneInstance> Current = new PropertyKey<SceneInstance>("SceneInstance.Current", typeof(SceneInstance));

        private Scene previousScene;
        private Scene scene;
        private bool enableScripting = true;

        /// <summary>
        /// Occurs when the scene changed from a scene child component.
        /// </summary>
        public event EventHandler<EventArgs> SceneChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public SceneInstance(IServiceRegistry registry) : this(registry, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneInstance" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <param name="enableScripting">if set to <c>true</c> [enable scripting].</param>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// sceneEntityRoot</exception>
        public SceneInstance(IServiceRegistry services, Scene sceneEntityRoot, bool enableScripting = true) : base(services)
        {
            if (services == null) throw new ArgumentNullException("services");

            this.enableScripting = enableScripting;
            Scene = sceneEntityRoot;
            RendererTypes = new EntityComponentRendererTypeCollection();
            ComponentTypeAdded += EntitySystemOnComponentTypeAdded;
            Load();
        }

        /// <summary>
        /// Gets the scene.
        /// </summary>
        /// <value>The scene.</value>
        public Scene Scene
        {
            get
            {
                return scene;
            }

            set
            {
                if (scene != value)
                {
                    previousScene = scene;
                    scene = value;
                }
            }
        }

        /// <summary>
        /// Gets the component renderers.
        /// </summary>
        /// <value>The renderers.</value>
        private EntityComponentRendererTypeCollection RendererTypes { get; set; }

        protected override void Destroy()
        {
            // TODO: Dispose of Scene, graphics compositor...etc.

            Reset();
            base.Destroy();
        }

        /// <summary>
        /// Gets the current scene valid only from a rendering context. May be null.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>SiliconStudio.Paradox.Engine.SceneInstance.</returns>
        public static SceneInstance GetCurrent(RenderContext context)
        {
            return context.Tags.GetSafe(Current);
        }

        /// <summary>
        /// Draws this scene instance with the specified context and <see cref="RenderFrame"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="toFrame">To frame.</param>
        /// <param name="compositorOverride">The compositor overload. Set this value to by-pass the default compositor of a scene.</param>
        /// <exception cref="System.ArgumentNullException">
        /// context
        /// or
        /// toFrame
        /// </exception>
        public void Draw(RenderContext context, RenderFrame toFrame, ISceneGraphicsCompositor compositorOverride = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (toFrame == null) throw new ArgumentNullException("toFrame");

            // If no scene, then we can return immediately
            if (Scene == null)
            {
                return;
            }

            var graphicsDevice = context.GraphicsDevice;

            bool hasGraphicsBegin = false;

            // Update global time
            var gameTime = context.Time;
            context.GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            context.GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            try
            {
                graphicsDevice.Begin();
                hasGraphicsBegin = true;

                // Always clear the state of the GraphicsDevice to make sure a scene doesn't start with a wrong setup 
                graphicsDevice.ClearState();

                // Draw the main scene using the current compositor (or the provided override)
                var graphicsCompositor = compositorOverride ?? Scene.Settings.GraphicsCompositor;
                if (graphicsCompositor != null)
                {
                    // Push context (pop after using)
                    using (context.PushTagAndRestore(RenderFrame.Current, toFrame))
                    using (context.PushTagAndRestore(SceneGraphicsLayer.Master, toFrame))
                    using (context.PushTagAndRestore(Current, this))
                    using (context.PushTagAndRestore(CameraRendererMode.RendererTypesKey, RendererTypes))
                    {
                        graphicsCompositor.Draw(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An exception occurred while rendering", ex);
            }
            finally
            {
                if (hasGraphicsBegin)
                {
                    graphicsDevice.End();
                }
            }
        }

        /// <summary>
        /// Updates this scene at the specified time.
        /// </summary>
        /// <param name="time">The time.</param>
        public override void Update(GameTime time)
        {
            UpdateFromChild();
            base.Update(time);
        }

        internal override void Draw(RenderContext context)
        {
            UpdateFromChild();
            base.Draw(context);
        }

        private void UpdateFromChild()
        {
            // If this scene instance is coming from a ChildSceneComponent, check that the Scene hasn't changed
            // If the scene has changed, we need to recreate a new SceneInstance with the new scene
            if (previousScene != Scene)
            {
                Reset();
                Load();
            }
        }

        private void Load()
        {
            previousScene = Scene;
            RendererTypes.Clear();

            OnSceneChanged();

            // If Scene is null, early exit
            if (Scene == null)
            {
                return;
            }

            // Initialize processors
            if (enableScripting)
                AddProcessor(new ScriptProcessor());   // Order: -100000
            AddProcessor(new SceneProcessor(this));    // Order: -10000
            AddProcessor(new HierarchicalProcessor()); // Order: -1000  - Important to pre-register this processor
            AddProcessor(new TransformProcessor());    // Order: -100
            AddProcessor(new CameraProcessor());       // Order: -10    - By default, as a scene without a camera is not really possible
            Add(Scene);

            // TODO: RendererTypes could be done outside this instance.
            HandleRendererTypes();
        }

        private void HandleRendererTypes()
        {
            foreach (var componentType in ComponentTypes)
            {
                EntitySystemOnComponentTypeAdded(null, componentType);
            }

            // Make sure that we always have a camera component registered
            RendererTypes.Add(new EntityComponentRendererType(typeof(CameraComponent), typeof(CameraComponentRenderer), int.MinValue));
        }

        private void EntitySystemOnComponentTypeAdded(object sender, Type type)
        {
            var rendererTypeAttribute = type.GetTypeInfo().GetCustomAttribute<DefaultEntityComponentRendererAttribute>();
            if (rendererTypeAttribute == null)
            {
                return;
            }
            var renderType = Type.GetType(rendererTypeAttribute.TypeName);
            if (renderType != null && typeof(IEntityComponentRenderer).GetTypeInfo().IsAssignableFrom(renderType.GetTypeInfo()) && renderType.GetTypeInfo().DeclaredConstructors.Any(x => !x.IsStatic && x.GetParameters().Length == 0))
            {
                var entityComponentRendererType = new EntityComponentRendererType(type, renderType, rendererTypeAttribute.Order);
                RendererTypes.Add(entityComponentRendererType);
            }
        }

        private void OnSceneChanged()
        {
            EventHandler<EventArgs> handler = SceneChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}