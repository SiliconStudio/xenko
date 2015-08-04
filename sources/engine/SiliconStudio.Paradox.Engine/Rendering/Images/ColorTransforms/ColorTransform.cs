// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// Base class for a <see cref="ColorTransformBase"/> to be used in a <see cref="ColorTransformGroup"/>.
    /// </summary>
    public abstract class ColorTransform : ColorTransformBase
    {
        protected ColorTransform(string colorTransformShader)
            : base(colorTransformShader)
        {
        }
    }
}