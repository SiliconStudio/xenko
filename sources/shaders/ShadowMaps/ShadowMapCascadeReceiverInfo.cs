// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.ShadowMaps
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ShadowMapCascadeReceiverInfo
    {
        public Matrix ViewProjReceiver;
        public Vector4 CascadeTextureCoordsBorder;
        public Vector3 Offset;
        private float Padding;
    }
}