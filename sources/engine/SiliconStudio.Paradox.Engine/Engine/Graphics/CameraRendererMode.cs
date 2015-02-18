// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Defines the type of rendering (Forward, Deferred...etc.)
    /// </summary>
    public abstract class CameraRendererMode : RendererBase
    {
        // TODO: Where should we put this key?
        public static readonly PropertyKey<EntityComponentRendererTypeCollection> RendererTypesKey = new PropertyKey<EntityComponentRendererTypeCollection>("CameraRendererMode.RendererTypesKey", typeof(CameraRendererMode));

        private readonly List<EntityComponentRendererType> sortedRendererTypes;
        private readonly GraphicsRendererCollection<IEntityComponentRenderer> renderers;

        protected CameraRendererMode()
        {
            sortedRendererTypes = new List<EntityComponentRendererType>();
            renderers = new GraphicsRendererCollection<IEntityComponentRenderer>();
            RendererOverrides = new Dictionary<Type, IEntityComponentRenderer>();
        }

        /// <summary>
        /// Gets the renderer overrides.
        /// </summary>
        /// <value>The renderer overrides.</value>
        [DataMemberIgnore]
        public Dictionary<Type, IEntityComponentRenderer> RendererOverrides { get; private set; }

        /// <summary>
        /// Gets or sets the effect to use to render the models in the scene.
        /// </summary>
        /// <value>The main model effect.</value>
        public abstract string MainModelEffect { get; set; } // TODO: This is not a good extensibility point. Check how to improve this

        /// <summary>
        /// Draws entities from a specified <see cref="SceneCameraRenderer" />.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void DrawCore(RenderContext context)
        {
            // Pre-create all renderers
            // TODO: We should handle cases where we are removing components types to improve performance
            var rendererTypes = context.Tags.GetSafe(RendererTypesKey);

            // Gets the renderer types
            sortedRendererTypes.Clear();
            sortedRendererTypes.AddRange(rendererTypes);
            sortedRendererTypes.Sort();

            for (int i = 0; i < sortedRendererTypes.Count; i++)
            {
                var componentType = sortedRendererTypes[i].ComponentType;

                // Check an existing overrides
                IEntityComponentRenderer renderer;
                RendererOverrides.TryGetValue(componentType, out renderer);

                var rendererType = renderer != null ? renderer.GetType() : sortedRendererTypes[i].RendererType;
                var currentType = i < renderers.Count ? renderers[i].GetType() : null;

                if (currentType != rendererType)
                {
                    if (renderer == null)
                    {
                        renderer = CreateRenderer(sortedRendererTypes[i]);
                    }

                    if (i == renderers.Count)
                    {
                        renderers.Add(renderer);
                    }
                    else
                    {
                        renderers.Insert(i, renderer);
                    }
                }
            }

            renderers.Draw(context);
        }

        protected virtual IEntityComponentRenderer CreateRenderer(EntityComponentRendererType rendererType)
        {
            return (IEntityComponentRenderer)Activator.CreateInstance(rendererType.RendererType);
        }
    }
}