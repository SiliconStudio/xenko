// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public interface IPipeline : IGraphicsRenderer
    {
        
    }

    /// <summary>
    /// A Collection of <see cref="Pipeline"/>.
    /// </summary>
    [DataContract("PipelineCollection")]
    public sealed class PipelineCollection : GraphicsRendererCollection<IPipeline>
    {
        
    }
}