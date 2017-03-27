// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Number of renderings per frame.
    /// </summary>
    /// <remarks>It can represent split viewports, like VR eyes, or multiplayer...</remarks>
    public interface IMultipleRenderViews
    {
        int ViewsCount { get; set; }

        int ViewsIndex { get; set; }
    }
}