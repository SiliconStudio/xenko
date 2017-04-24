// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ShadowMapCascadeLevel
    {
        public Matrix ViewProjReceiver;
        public Vector4 CascadeTextureCoordsBorder;
        public Vector3 Offset;
        private float Padding;
    }
}
