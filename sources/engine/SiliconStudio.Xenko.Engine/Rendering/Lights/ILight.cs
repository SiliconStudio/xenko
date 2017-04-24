// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base interface for all lights.
    /// </summary>
    public interface ILight
    {
        bool Update(LightComponent lightComponent);
    }
}
