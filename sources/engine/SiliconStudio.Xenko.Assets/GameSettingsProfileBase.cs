// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Default game settings profile. This is currently used internally only.
    /// </summary>
    [DataContract()]
    public abstract class GameSettingsProfileBase : IGameSettingsProfile
    {
        [DataMember(10)]
        public GraphicsPlatform GraphicsPlatform { get; set; }

        public abstract IEnumerable<GraphicsPlatform> GetSupportedGraphicsPlatforms();
    }
}