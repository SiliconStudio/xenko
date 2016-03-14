// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsDevice
    {
        /// <summary>
        /// Creates a new deferred context.
        /// </summary>
        /// <returns>A deferred graphics device context.</returns>
        public GraphicsDevice NewDeferred()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="profile">The graphics profile.</param>
        protected void Initialize(GraphicsAdapter adapter, GraphicsProfile profile, PresentationParameters presentationParameters, object windowHandle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes factories for this instance.
        /// </summary>
        protected void InitializeFactories()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the immediate graphics device context.
        /// </summary>
        /// <value>
        /// The immediate graphics device context.
        /// </value>
        public GraphicsDevice ImmediateContext
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is supporting deferred context.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is supporting deferred context; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeferredContextSupported
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the default presentation parameters associated with this graphics device.
        /// </summary>
        public PresentationParameters PresentationParameters
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this GraphicsDevice is in fullscreen.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this GraphicsDevice is fullscreen; otherwise, <c>false</c>.
        /// </value>
        public bool IsFullScreen
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
} 
#endif
