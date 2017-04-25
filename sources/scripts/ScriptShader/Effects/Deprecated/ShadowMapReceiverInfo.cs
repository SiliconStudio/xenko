// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct ShadowMapReceiverInfo
    {
        public Matrix WorldViewProjReceiver0;
        public Matrix WorldViewProjReceiver1;
        public Matrix WorldViewProjReceiver2;
        public Matrix WorldViewProjReceiver3;

        public Vector4 CascadeTextureCoordsBorder0;
        public Vector4 CascadeTextureCoordsBorder1;
        public Vector4 CascadeTextureCoordsBorder2;
        public Vector4 CascadeTextureCoordsBorder3;

        public Vector3 Offset0;
        private float Padding0;
        public Vector3 Offset1;
        private float Padding1;
        public Vector3 Offset2;
        private float Padding2;
        public Vector3 Offset3;
        private float Padding3;

        public Vector3 ShadowLightDirection;
        private float Padding4;

        public Vector3 ShadowLightDirectionVS;
        private float Padding5;

        public Color3 ShadowLightColor;
        public float ShadowMapDistance;
    }
}
