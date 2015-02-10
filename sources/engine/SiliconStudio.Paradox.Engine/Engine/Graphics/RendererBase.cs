// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Base implementation of <see cref="IGraphicsRenderer"/>
    /// </summary>
    public abstract class RendererBase : ComponentBase, IGraphicsRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RendererBase"/> class.
        /// </summary>
        protected RendererBase() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase" /> class.
        /// </summary>
        /// <param name="name">The name attached to this component</param>
        protected RendererBase(string name)
            : base(name)
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RendererExtendedBase"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(0)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the debug.
        /// </summary>
        /// <value>The name of the debug.</value>
        [DataMemberIgnore]
        public string DebugName { get; set; }

        public virtual void Load()
        {
        }

        public virtual void Unload()
        {
        }

        protected virtual void BeginRendering(RenderContext context)
        {
            if (DebugName != null || Name != null)
            {
                context.GraphicsDevice.BeginProfile(Color.Green, DebugName ?? Name);
            }
        }

        protected virtual void EndRendering(RenderContext context)
        {
            if (DebugName != null || Name != null)
            {
                context.GraphicsDevice.EndProfile();
            }
        }

        protected virtual void OnRendering(RenderContext context)
        {
        }

        public void Draw(RenderContext context)
        {
            if (Enabled)
            {
                BeginRendering(context);
                OnRendering(context);
                EndRendering(context);
            }
        }
    }
}