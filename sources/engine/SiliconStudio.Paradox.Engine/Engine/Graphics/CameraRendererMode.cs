// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;

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
        public static readonly PropertyKey<List<EntityComponentRenderableAttribute>> RendererTypesKey = new PropertyKey<List<EntityComponentRenderableAttribute>>("CameraRendererMode.RendererTypesKey", typeof(CameraRendererMode));

        private GraphicsRendererCollection<IEntityComponentRenderer> renderers;

        protected CameraRendererMode()
        {
            renderers = new GraphicsRendererCollection<IEntityComponentRenderer>();
        }

        /// <summary>
        /// Gets the main effect used for rendering model in this mode.
        /// </summary>
        /// <returns>System.String.</returns>
        public abstract string GetMainModelEffect();

        /// <summary>
        /// Gets or sets the effect mixin that will applied on top of the default Forward effect mixin.
        /// </summary>
        /// <value>The effect overrider.</value>
        /// <userdoc>
        /// The effect overrider allows to override a global effect used when rendering in forward mode. The overrider can
        /// provide an effect that will be 'mixin' after the forward effect, allowing to change the behavior of the default 
        /// forward effect.
        /// </userdoc>
        [Display("Effect Mixin")]
        [DataMember(10)]
        [DefaultValue(null)]
        public IEffectMixinProvider EffectMixin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EffectMixin"/> overrides completely the default effect of the
        /// rendering mode.
        /// </summary>
        /// <value><c>true</c> if [effect mixin overrides]; otherwise, <c>false</c>.</value>
        [Display("Mixin Overrides?")]
        [DataMember(20)]
        [DefaultValue(false)]
        public bool EffectMixinOverrides { get; set; }

        /// <summary>
        /// Draws entities from a specified <see cref="SceneCameraRenderer" />.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnRendering(RenderContext context)
        {
            // Pre-create all renderers
            // TODO: We should handle cases where we are removing components types to improve performance
            var rendererTypes = context.Tags.GetSafe(RendererTypesKey);
            for (int i = 0; i < rendererTypes.Count; i++)
            {
                var rendererType = rendererTypes[i].Type;
                var currentType = i < renderers.Count ? renderers[i].GetType() : null;

                if (currentType != rendererType)
                {
                    var renderer = (IEntityComponentRenderer)Activator.CreateInstance(rendererType);
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
    }
}