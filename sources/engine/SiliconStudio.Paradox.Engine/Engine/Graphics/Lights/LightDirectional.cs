// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A directional light.
    /// </summary>
    [DataContract("LightDirectional")]
    [Display("Directional")]
    public class LightDirectional : DirectLightBase
    {
        public LightDirectional()
        {
            ShadowImportance = LightShadowImportance.High;
        }

        // TODO: Add support for disk based sun
        protected override float ComputeScreenCoverage(CameraComponent camera, Vector3 position, Vector3 direction, float width, float height)
        {
            // As the directional light is covering the whole screen, we take the max of current width, height
            return Math.Max(width, height);
        }
    }
}