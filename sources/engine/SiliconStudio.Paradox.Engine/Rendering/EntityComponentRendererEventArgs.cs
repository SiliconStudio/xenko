// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// An event published by <see cref="CameraRendererMode"/> when a new renderer is created.
    /// </summary>
    public sealed class EntityComponentRendererEventArgs : EventArgs
    {
        public EntityComponentRendererEventArgs(CameraRendererMode cameraRendererMode, IEntityComponentRenderer renderer)
        {
            CameraRendererMode = cameraRendererMode;
            Renderer = renderer;
        }

        /// <summary>
        /// Gets the camera renderer mode that created the renderer.
        /// </summary>
        /// <value>The camera renderer mode.</value>
        public CameraRendererMode CameraRendererMode { get; private set; }

        /// <summary>
        /// Gets the new renderer created.
        /// </summary>
        /// <value>The renderer.</value>
        public IEntityComponentRenderer Renderer { get; private set; }
    }
}