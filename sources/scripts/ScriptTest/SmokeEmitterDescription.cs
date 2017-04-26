// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Runtime.InteropServices;

using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.Serialization;

namespace ScriptTest
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    [SerializableExtended]
    public struct SmokeEmitterDescription
    {
        [FieldOffset(0)]
        public Vector3 Position;

        [FieldOffset(12)]
        public float MaxTime;

        [FieldOffset(16)]
        public Vector3 Scatter;

        [FieldOffset(28)]
        public float DeltaSize;

        [FieldOffset(32)]
        public Vector3 Velocity;

        [FieldOffset(44)]
        public float InitialSize;

        [FieldOffset(48)]
        public float Opacity;

        public override string ToString()
        {
            return string.Format("Position: {0}, MaxTime: {1}, Scatter: {2}, DeltaSize: {3}, Velocity: {4}, InitialSize: {5}", Position, MaxTime, Scatter, DeltaSize, Velocity, InitialSize);
        }
    };
}
