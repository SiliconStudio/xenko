// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Rendering.Images
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
