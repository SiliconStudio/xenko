// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Renderer interface for a end-user <see cref="ImageEffect"/> accessible from <see cref="SceneEffectRenderer"/>. See remarks.
    /// </summary>
    /// <remarks>
    /// An <see cref="IImageEffectRenderer"/> expect an input texture on slot 0, possibly a depth texture on slot 1 and a single
    /// output.
    /// </remarks>
    public interface IImageEffectRenderer : IImageEffect
    {
    }
}