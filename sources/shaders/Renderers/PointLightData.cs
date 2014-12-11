// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PointLightData
    {
        public Vector3 LightPosition;
        public float LightRadius;

        public Color3 DiffuseColor;
        public float LightIntensity;
    }
}