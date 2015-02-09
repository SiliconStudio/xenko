// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Output to the Direct (same as the output of the master layer).
    /// </summary>
    [DataContract("GraphicsComposerOutputDirect")]
    [Display("Direct")]
    public sealed class GraphicsComposerOutputDirect : IGraphicsComposerOutput
    {
    }
}