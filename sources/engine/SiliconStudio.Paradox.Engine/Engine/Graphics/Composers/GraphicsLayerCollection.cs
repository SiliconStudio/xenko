// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// A Collection of <see cref="GraphicsLayer"/>.
    /// </summary>
    [DataContract("GraphicsLayerCollection")]
    public sealed class GraphicsLayerCollection : SafeList<GraphicsLayer>
    {
    }
}