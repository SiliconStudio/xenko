// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// A processor that updates camera view and projection along the setup of <see cref="RenderTargetSetter"/>
    /// </summary>
    public class DelegateRenderer : Renderer
    {
        /// <summary>
        /// Gets or sets the action to perform when the renderer is loaded.
        /// </summary>
        public Action OnLoad { get; set; }

        /// <summary>
        /// Gets or sets the action to perform when the renderer is unloaded.
        /// </summary>
        public Action OnUnload { get; set; }

        /// <summary>
        /// Gets or sets the action to perform on rendering.
        /// </summary>
        public Action<RenderContext> Render { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public DelegateRenderer(IServiceRegistry services)
            : base(services)
        {
        }

        public override void Load()
        {
            base.Load();

            var handler = OnLoad;
            if (handler != null)
                handler();
        }

        public override void Unload()
        {
            base.Unload();

            var handler = OnUnload;
            if (handler != null)
                handler();
        }

        protected override void OnRendering(RenderContext context)
        {
            var handler = Render;
            if (handler != null)
                handler(context);
        }
    }
}