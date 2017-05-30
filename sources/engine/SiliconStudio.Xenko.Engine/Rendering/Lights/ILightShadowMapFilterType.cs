// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Common interface of a shadowmap filter.
    /// </summary>
    public interface ILightShadowMapFilterType
    {
        bool RequiresCustomBuffer();
    }
}
