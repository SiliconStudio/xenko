// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
    /// <summary>
    /// Base interface for game settings for a particular profile
    /// </summary>
    public interface IGameSettingsProfile
    {
        /// <summary>
        /// Gets the GraphicsPlatform used by this profile.
        /// </summary>
        GraphicsPlatform GraphicsPlatform { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsPlatform"/> list supported by this profile.
        /// </summary>
        /// <returns></returns>
        IEnumerable<GraphicsPlatform> GetSupportedGraphicsPlatforms();
    }
}
