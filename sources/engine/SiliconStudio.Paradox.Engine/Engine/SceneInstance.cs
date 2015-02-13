// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A <see cref="Scene"/> instance that can be rendered.
    /// </summary>
    public sealed class SceneInstance : IDisposable
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SceneInstance");

        private readonly IServiceRegistry services;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneInstance"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="scene">The scene.</param>
        public SceneInstance(IServiceRegistry services, Scene scene) : this(services, null, scene)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneInstance" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="sceneChildComponent">The scene child component.</param>
        /// <param name="sceneEntityRoot">The scene entity root.</param>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// sceneEntityRoot</exception>
        internal SceneInstance(IServiceRegistry services, SceneChildComponent sceneChildComponent, Scene sceneEntityRoot)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (sceneEntityRoot == null) throw new ArgumentNullException("sceneEntityRoot");

            this.services = services;
            ChildComponent = sceneChildComponent;
            Scene = sceneEntityRoot;
            RendererTypes = new List<EntityComponentRendererType>();
            Load();
        }

        /// <summary>
        /// Gets the child component if this scene instance is instantiated by a child scene. This is null for a root scene.
        /// </summary>
        /// <value>The child component.</value>
        public SceneChildComponent ChildComponent { get; private set; }

        /// <summary>
        /// Gets the scene.
        /// </summary>
        /// <value>The scene.</value>
        public Scene Scene { get; private set; }

        /// <summary>
        /// Entity System dedicated to this scene.
        /// </summary>
        public EntityManager EntityManager { get; private set; }

        /// <summary>
        /// Gets the component renderers.
        /// </summary>
        /// <value>The renderers.</value>
        private List<EntityComponentRendererType> RendererTypes { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Unload();
        }

        /// <summary>
        /// Draws this scene instance with the specified context and <see cref="RenderFrame"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="toFrame">To frame.</param>
        /// <param name="compositorOverload">The compositor overload. Set this value to by-pass the default compositor of a scene.</param>
        /// <exception cref="System.ArgumentNullException">
        /// context
        /// or
        /// toFrame
        /// </exception>
        public void Draw(RenderContext context, RenderFrame toFrame, ISceneGraphicsCompositor compositorOverload = null)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (toFrame == null) throw new ArgumentNullException("toFrame");

            var graphicsDevice = context.GraphicsDevice;

            bool hasGraphicsBegin = false;

            // Update global time
            var gameTime = context.Time;
            context.GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            context.GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            try
            {
                // Update entities at draw time
                UpdateFromChild();
                EntityManager.Draw(gameTime);

                graphicsDevice.Begin();
                hasGraphicsBegin = true;

                graphicsDevice.ClearState();

                // Update the render context to use the main RenderFrame as current by default
                // TODO: Push/Pop values

                // Draw the main scene.
                var graphicsCompositor = compositorOverload ?? this.Scene.Settings.GraphicsCompositor;
                if (graphicsCompositor != null)
                {
                    using (var t1 = context.PushTagAndRestore(RenderFrame.Current, toFrame))
                    using (var t2 = context.PushTagAndRestore(SceneGraphicsLayer.Master, toFrame))
                    using (var t3 = context.PushTagAndRestore(EntityManager.Current, this.EntityManager))
                    using (var t4 = context.PushTagAndRestore(CameraRendererMode.RendererTypesKey, this.RendererTypes))
                    {
                        graphicsCompositor.Draw(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while rendering", ex);
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
        public void Update(GameTime time)
        {
            UpdateFromChild();
            EntityManager.Update(time);
        }

        private void UpdateFromChild()
        {
            // If this scene instance is coming from a SceneChildComponent, check that the Scene hasn't changed
            // If the scene has changed, we need to recreate a new EntityManager with the new scene
            if (ChildComponent != null && ChildComponent.Scene != Scene)
            {
                Scene = ChildComponent.Scene;
                Unload();
                Load();
            }
        }

        private void Load()
        {
            RendererTypes.Clear();

            // Create a new EntityManager
            EntityManager = new EntityManager(services);
            EntityManager.Processors.Add(new SceneProcessor(this));
            EntityManager.Processors.Add(new HierarchicalProcessor()); // Important to pre-register this processor
            EntityManager.Processors.Add(new TransformProcessor());
            EntityManager.Add(Scene);

            foreach (var componentType in EntityManager.RegisteredComponentTypes)
            {
                EntitySystemOnComponentTypeRegistered(componentType);
            }

            EntityManager.ComponentTypeRegistered += EntitySystemOnComponentTypeRegistered;
        }

        private void EntitySystemOnComponentTypeRegistered(Type type)
        {
            var rendererTypeAttribute = type.GetTypeInfo().GetCustomAttribute<DefaultEntityComponentRendererAttribute>();
            if (rendererTypeAttribute == null)
            {
                return;
            }
            var renderType = rendererTypeAttribute.Value.Type;

            if (renderType != null && typeof(IEntityComponentRenderer).IsAssignableFrom(renderType) && renderType.GetConstructor(Type.EmptyTypes) != null)
            {
                RendererTypes.Add(rendererTypeAttribute.Value);
                RendererTypes.Sort(EntityComponentRendererType.DefaultComparer);
            }
        }

        private void Unload()
        {
            if (EntityManager != null)
            {
                // TODO: Unload resources
                EntityManager.ComponentTypeRegistered -= EntitySystemOnComponentTypeRegistered;
                RendererTypes.Clear();
                EntityManager.Dispose();
                EntityManager = null;
            }
        }
    }
}