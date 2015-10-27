// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Base interface for all lights.
    /// </summary>
    public interface ILight
    {
        bool Update(LightComponent lightComponent);
    }
}