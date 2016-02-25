// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Engine
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

        /// <summary>
        /// A property key to get the current render system from the <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<NextGenRenderSystem> CurrentRenderSystem = new PropertyKey<NextGenRenderSystem>("SceneInstance.CurrentRenderSystem", typeof(SceneInstance));

        /// <summary>
        /// A property key to get the current visibility group from the <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<VisibilityGroup> CurrentVisibilityGroup = new PropertyKey<VisibilityGroup>("SceneInstance.CurrentVisibilityGroup", typeof(SceneInstance));

        private readonly Dictionary<TypeInfo, Type> registeredRenderProcessorTypes = new Dictionary<TypeInfo, Type>();
        private Scene previousScene;
        private Scene scene;

        public TrackingCollection<VisibilityGroup> VisibilityGroups { get; }

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
        public SceneInstance(IServiceRegistry services, Scene sceneEntityRoot, ExecutionMode executionMode = ExecutionMode.Runtime) : base(services)
        {
            if (services == null) throw new ArgumentNullException("services");

            ExecutionMode = executionMode;
            VisibilityGroups = new TrackingCollection<VisibilityGroup>();
            VisibilityGroups.CollectionChanged += VisibilityGroups_CollectionChanged;
            Scene = sceneEntityRoot;
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
        /// <returns>SiliconStudio.Xenko.Engine.SceneInstance.</returns>
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
        public void Draw(RenderDrawContext context, RenderFrame toFrame)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (toFrame == null) throw new ArgumentNullException("toFrame");

            // If no scene, then we can return immediately
            if (Scene == null)
            {
                return;
            }

            var commandList = context.CommandList;

            bool hasGraphicsBegin = false;

            // Update global time
            var gameTime = context.RenderContext.Time;
            // TODO GRAPHICS REFACTOR
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.Time, (float)gameTime.Total.TotalSeconds);
            //context.GraphicsDevice.Parameters.Set(GlobalKeys.TimeStep, (float)gameTime.Elapsed.TotalSeconds);

            try
            {
                commandList.Begin();
                hasGraphicsBegin = true;

                // Always clear the state of the GraphicsDevice to make sure a scene doesn't start with a wrong setup 
                commandList.ClearState();

                // Draw the main scene using the current compositor (or the provided override)
                var graphicsCompositor = Scene.Settings.GraphicsCompositor;

                if (graphicsCompositor != null)
                {
                    // Push context (pop after using)
                    using (context.RenderContext.PushTagAndRestore(RenderFrame.Current, toFrame))
                    using (context.RenderContext.PushTagAndRestore(SceneGraphicsLayer.Master, toFrame))
                    using (context.RenderContext.PushTagAndRestore(Current, this))
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
                    commandList.End();
                }
            }
        }

        public VisibilityGroup GetOrCreateVisibilityGroup(NextGenRenderSystem renderSystem)
        {
            // Find if it exists
            VisibilityGroup visibilityGroup = null;
            foreach (var currentVisibilityGroup in VisibilityGroups)
            {
                if (currentVisibilityGroup.RenderSystem == renderSystem)
                {
                    visibilityGroup = currentVisibilityGroup;
                    break;
                }
            }

            // If first time, let's create and register it
            if (visibilityGroup == null)
                VisibilityGroups.Add(visibilityGroup = new VisibilityGroup(renderSystem));

            return visibilityGroup;
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

        protected internal override void Reset()
        {
            if (previousScene != null)
            {
                previousScene.Entities.CollectionChanged -= Entities_CollectionChanged;
            }
            base.Reset();
        }

        private void Load()
        {
            previousScene = Scene;

            OnSceneChanged();

            // If Scene is null, early exit
            if (Scene == null)
            {
                return;
            }

            // Add Loaded entities
            foreach (var entity in Scene.Entities)
                Add(entity);

            // Listen to future changes in Scene.Entities
            Scene.Entities.CollectionChanged += Entities_CollectionChanged;

            HandleRendererTypes();
        }

        private void Entities_CollectionChanged(object sender, Core.Collections.TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add((Entity)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove((Entity)e.Item);
                    break;
            }
        }

        private void HandleRendererTypes()
        {
            ComponentTypeAdded += EntitySystemOnComponentTypeAdded;
            foreach (var componentType in ComponentTypes)
            {
                EntitySystemOnComponentTypeAdded(null, componentType);
            }
        }

        private void VisibilityGroups_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            var visibilityGroup = (VisibilityGroup)e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var registeredRenderProcessorType in registeredRenderProcessorTypes)
                    {
                        CreateRenderProcessor(registeredRenderProcessorType.Value, visibilityGroup);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // TODO GRAPHICS REFACTOR
                    throw new NotImplementedException();
            }
        }

        private void EntitySystemOnComponentTypeAdded(object sender, TypeInfo type)
        {
            var rendererTypeAttributes = type.GetCustomAttributes<DefaultEntityComponentRendererAttribute>();
            foreach (var rendererTypeAttribute in rendererTypeAttributes)
            {
                var processorType = AssemblyRegistry.GetType(rendererTypeAttribute.TypeName);
                if (processorType == null || !typeof(IEntityComponentRenderProcessor).GetTypeInfo().IsAssignableFrom(processorType.GetTypeInfo()))
                {
                    continue;
                }

                registeredRenderProcessorTypes.Add(type, processorType);

                // Create a render processor for each visibility group
                foreach (var visibilityGroup in VisibilityGroups)
                {
                    CreateRenderProcessor(processorType, visibilityGroup);
                }
            }
        }

        private void CreateRenderProcessor(Type processorType, VisibilityGroup visibilityGroup)
        {
            // Create
            var processor = (EntityProcessor)Activator.CreateInstance(processorType);

            // Set visibility group
            ((IEntityComponentRenderProcessor)processor).VisibilityGroup = visibilityGroup;

            // Add processor
            Processors.Add(processor);
        }

        private void OnSceneChanged()
        {
            EventHandler<EventArgs> handler = SceneChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}