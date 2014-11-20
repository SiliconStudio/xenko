// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DirectLightData
    {
        public Vector3 LightDirection;
        public float LightIntensity;

        public Color3 DiffuseColor;
    };
}