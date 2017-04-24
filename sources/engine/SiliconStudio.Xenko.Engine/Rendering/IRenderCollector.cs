// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    public interface IRenderCollector
    {
        /// <summary>
        /// Executed before extract. Should create views, update RenderStages, etc...
        /// </summary>
        /// <param name="context"></param>
        void Collect(RenderContext context);
    }
}
