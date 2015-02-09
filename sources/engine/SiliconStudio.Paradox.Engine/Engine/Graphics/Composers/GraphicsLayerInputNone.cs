// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Defines a none input layer.
    /// </summary>
    /// <userdoc>
    /// The layer doesn't take any specific input.
    /// </userdoc>
    [DataContract("GraphicsLayerInputNone")]
    [Display("None")]
    public sealed class GraphicsLayerInputNone : IGraphicsLayerInput
    {
    }
}