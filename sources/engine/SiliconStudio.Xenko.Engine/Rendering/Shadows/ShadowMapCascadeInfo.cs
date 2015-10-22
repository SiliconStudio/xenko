// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ShadowMapCascadeInfo
    {
        public ShadowMapCascadeLevel CascadeLevels;
        public Matrix ViewProjCaster;
        public Vector4 CascadeTextureCoords;
    }
}