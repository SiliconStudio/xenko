// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine
{
    public interface ISceneRendererContext
    {
        /// <summary>
        /// The service registry.
        /// </summary>
        ServiceRegistry Services { get; }

        /// <summary>
        /// The list of game systems.
        /// </summary>
        GameSystemCollection GameSystems { get; }

        /// <summary>
        /// The current scene system.
        /// </summary>
        SceneSystem SceneSystem { get; }

        /// <summary>
        /// The graphics device.
        /// </summary>
        GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// The graphics context used during draw.
        /// </summary>
        GraphicsContext GraphicsContext { get; }

        /// <summary>
        ///  The content manager to load content.
        /// </summary>
        ContentManager Content { get; }

        GameTime DrawTime { get; }
    }
}
