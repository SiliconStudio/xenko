// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    /// <summary>
    /// Base settings for Windows profile.
    /// </summary>
    [DataContract("WindowsGameSettingsProfile")]
    public class WindowsGameSettingsProfile : GameSettingsProfileBase
    {
        public WindowsGameSettingsProfile()
        {
            GraphicsPlatform = GraphicsPlatform.Direct3D11;
        }

        public override IEnumerable<GraphicsPlatform> GetSupportedGraphicsPlatforms()
        {
            return new[] { GraphicsPlatform.Direct3D11, GraphicsPlatform.OpenGL, GraphicsPlatform.OpenGLES, };
        }
    }
}