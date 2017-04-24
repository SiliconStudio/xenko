// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Defines default values.
    /// </summary>
    internal partial class ImageScalerShaderKeys
    {
        static ImageScalerShaderKeys()
        {
            // Default value of 1.0f
            Color = ParameterKeys.NewValue(Color4.White);
        }
    }
}
