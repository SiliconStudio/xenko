// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Output to the Direct (same as the output of the master layer).
    /// </summary>
    [DataContract("GraphicsComposerOutputCurrent")]
    [Display("Current")]
    public sealed class GraphicsComposerOutputCurrent: IGraphicsComposerOutput
    {
        public void Dispose()
        {
        }

        public RenderFrame GetRenderFrame(RenderContext context)
        {
            // Get the current (or master) render frame
            return context.Parameters.Get(RenderFrame.Current) ?? context.Parameters.Get(RenderFrame.Master);
        }
    }
}