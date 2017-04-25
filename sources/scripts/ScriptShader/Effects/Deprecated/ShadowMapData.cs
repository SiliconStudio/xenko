// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Intermediate ShadowMap calculation structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ShadowMapData
    {
        public Matrix ViewProjCaster0;
        public Matrix ViewProjCaster1;
        public Matrix ViewProjCaster2;
        public Matrix ViewProjCaster3;

        public Matrix ViewProjReceiver0;
        public Matrix ViewProjReceiver1;
        public Matrix ViewProjReceiver2;
        public Matrix ViewProjReceiver3;

        public Vector3 Offset0;
        public Vector3 Offset1;
        public Vector3 Offset2;
        public Vector3 Offset3;

        public Color3 LightColor;
    }
}
