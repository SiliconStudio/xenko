// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines the type of rendering (Forward, Deferred...etc.)
    /// </summary>
    [DataContract("CameraRendererMode")]
    public abstract class CameraRendererMode : RendererBase
    {
        // TODO: Where should we put this key?
        public static readonly PropertyKey<EntityComponentRendererTypeCollection> RendererTypesKey = new PropertyKey<EntityComponentRendererTypeCollection>("CameraRendererMode.RendererTypesKey", typeof(CameraRendererMode));

        private readonly Dictionary<Type, IEntityComponentRenderer> componentTypeToRenderer = new Dictionary<Type, IEntityComponentRenderer>();
        private readonly List<EntityComponentRendererType> sortedRendererTypes;
        private readonly EntityComponentRendererBatch batchRenderer;

        /// <summary>
        /// Occurs when a renderer is created.
        /// </summary>
        public event EventHandler<EntityComponentRendererEventArgs> RendererCreated;

        protected CameraRendererMode()
        {
            sortedRendererTypes = new List<EntityComponentRendererType>();
            batchRenderer = new EntityComponentRendererBatch();
            RendererOverrides = new Dictionary<Type, IEntityComponentRenderer>();
            RenderComponentTypes = new HashSet<Type>();
            SkipComponentTypes = new HashSet<Type>();
        }

        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        /// <userdoc>The name of the effect to use to render models (a '.xksl' or '.xkfx' filename without the extension).</userdoc>
        [DataMember(10)]
        public abstract string ModelEffect { get; set; }// TODO: This is not a good extensibility point. Check how to improve this

        /// <summary>
        /// Gets the renderer overrides.
        /// </summary>
        /// <value>The renderer overrides.</value>
        [DataMemberIgnore]
        public Dictionary<Type, IEntityComponentRenderer> RendererOverrides { get; private set; }

        /// <summary>
        /// Gets the filter on the types to render.
        /// </summary>
        /// <value>The filter renderer types.</value>
        [DataMemberIgnore]
        public HashSet<Type> RenderComponentTypes { get; private set; }

        /// <summary>
        /// Gets the filter on the types to skip.
        /// </summary>
        /// <value>The filter renderer types.</value>
        [DataMemberIgnore]
        public HashSet<Type> SkipComponentTypes { get; private set; }

        [DataMemberIgnore]
        public EntityComponentRendererBatch Renderers
        {
            get
            {
                return batchRenderer;
            }
        }

        /// <summary>
        /// Gets the default <see cref="RasterizerState" /> for models drawn by this render mode.
        /// </summary>
        /// <param name="isGeomertryInverted"><c>true</c> if the rendered gometry is inverted through scaling, <c>false</c> otherwise.</param>
        /// <returns>The rasterizer state.</returns>
        public virtual RasterizerStateDescription GetDefaultRasterizerState(bool isGeomertryInverted)
        {
            return isGeomertryInverted ? Context.GraphicsDevice.RasterizerStates.CullFront : Context.GraphicsDevice.RasterizerStates.CullBack;
        }

        /// <summary>
        /// Draws entities from a specified <see cref="SceneCameraRenderer" />.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void DrawCore(RenderDrawContext context)
        {
            // Pre-create all batchRenderer
            // TODO: We should handle cases where we are removing components types to improve performance
            var rendererTypes = context.RenderContext.Tags.GetSafe(RendererTypesKey);

            // Gets the renderer types
            sortedRendererTypes.Clear();
            sortedRendererTypes.AddRange(rendererTypes);
            sortedRendererTypes.Sort();
            
            // clear current renderer batching
            batchRenderer.Clear();

            // rebuild the renderer batch
            for (int i = 0; i < sortedRendererTypes.Count; i++)
            {
                var componentType = sortedRendererTypes[i].ComponentType;

                // If a Filter on a component types is set, skip 
                if (RenderComponentTypes.Count > 0 && !RenderComponentTypes.Contains(componentType))
                {
                    continue;
                }
                if (SkipComponentTypes.Contains(componentType))
                {
                    continue;
                }

                // check in existing overrides
                IEntityComponentRenderer renderer;
                RendererOverrides.TryGetValue(componentType, out renderer);

                // check in existing default renderer
                if (renderer == null)
                    componentTypeToRenderer.TryGetValue(componentType, out renderer);
                
                // create the default renderer if not existing
                if (renderer == null)
                {
                    renderer = CreateRenderer(sortedRendererTypes[i]);
                    componentTypeToRenderer[componentType] = renderer;
                }

                batchRenderer.Add(renderer);
            }

            // Call the batch renderer
            batchRenderer.Draw(context);
        }

        protected override void Destroy()
        {
            base.Destroy();

            foreach (var renderer in componentTypeToRenderer.Values)
            {
                renderer.Dispose();
            }
            componentTypeToRenderer.Clear();
        }

        private IEntityComponentRenderer CreateRenderer(EntityComponentRendererType rendererType)
        {
            var renderer = CreateRendererCore(rendererType);
            var handler = RendererCreated;
            if (handler != null)
            {
                handler(this, new EntityComponentRendererEventArgs(this, renderer));
            }
            return renderer;
        }

        protected virtual IEntityComponentRenderer CreateRendererCore(EntityComponentRendererType rendererType)
        {
            return (IEntityComponentRenderer)Activator.CreateInstance(rendererType.RendererType);
        }
    }
}