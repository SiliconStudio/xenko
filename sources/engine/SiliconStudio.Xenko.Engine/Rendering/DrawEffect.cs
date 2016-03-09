// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// The base class in charge of applying and drawing an effect.
    /// </summary>
    [DataContract]
    public abstract class DrawEffect : RendererBase
    {
        private ImageScaler scaler;

        /// <summary>
        /// Initializes a <see cref="DrawEffect"/>.
        /// </summary>
        protected DrawEffect(String name):
            base(name)
        {
            Enabled = true;
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Initializes the <see cref="DrawEffect"/> with the given <see cref="RenderContext"/>.
        /// </summary>
        protected DrawEffect() :
            this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawEffect" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="name">The name.</param>
        protected DrawEffect(RenderContext context, string name = null)
            : this(name)
        {
            Initialize(context);
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMemberIgnore]
        public ParameterCollection Parameters { get; protected set; }

        /// <summary>
        /// Gets a shared <see cref="ImageScaler"/>.
        /// </summary>
        [DataMemberIgnore]
        protected ImageScaler Scaler
        {
            get
            {
                // TODO
                // return scaler ?? (scaler = Context.GetSharedEffect<ImageScaler>());
                if (scaler == null)
                {
                    scaler = new ImageScaler();
                    scaler.Initialize(Context);
                }
                return scaler;
            }
        }

        /// <summary>
        /// Resets the state of this effect.
        /// </summary>
        public virtual void Reset()
        {
            SetDefaultParameters();
        }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="Reset"/> is called)
        /// </summary>
        protected virtual void SetDefaultParameters()
        {
        }

        /// <summary>
        /// Draws a full screen quad using iterating on each pass of this effect.
        /// </summary>
        public void Draw(RenderDrawContext context, string name)
        {
            var previousDebugName = Name;
            if (name != null)
            {
                Name = name;
            }
            base.Draw(context);

            Name = previousDebugName;
        }
        
        /// <summary>
        /// Draws a full screen quad using iterating on each pass of this effect.
        /// </summary>
        public void Draw(RenderDrawContext context, string nameFormat, params object[] args)
        {
            // TODO: this is alocating a string, we should try to not allocate here.
            Draw(context, name: string.Format(nameFormat, args));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("Effect {0}", Name);
        }
    }
}