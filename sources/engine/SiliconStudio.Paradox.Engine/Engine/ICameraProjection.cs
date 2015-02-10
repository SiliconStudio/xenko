// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Interface used for the <see cref="CameraComponent.Projection"/>.
    /// </summary>
    public interface ICameraProjection
    {
        Matrix CalculateProjection(float aspectRatio, float nearPlane, float farPlane);
    }
}