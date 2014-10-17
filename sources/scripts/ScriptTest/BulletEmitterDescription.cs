using System;
using System.Runtime.InteropServices;

using SiliconStudio.Paradox.Games.Mathematics;
using SiliconStudio.Paradox.Games.Serialization;

namespace ScriptTest
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    [SerializableExtended]
    public struct BulletEmitterDescription
    {
        [FieldOffset(0)]
        public Vector3 Center;

        [FieldOffset(12)]
        public float ScaleFromCenter;

        [FieldOffset(16)]
        public Vector3 Target;

        [FieldOffset(28)]
        public float MaxTimeTarget;

        [FieldOffset(32)]
        public Vector3 VelocityUp;
        
        [FieldOffset(44)]
        public float MaxTimeUp;

        [FieldOffset(48)]
        public Vector3 TargetOld;

        [FieldOffset(60)]
        public float BulletSize;

        [FieldOffset(64)]
        public float VelocityTarget;

        [FieldOffset(68)]
        public float Opacity;

        [FieldOffset(72)]
        public float DistanceDragonRepulse;

        [FieldOffset(76)]
        public float DecayDragonRepulse;

        [FieldOffset(80)]
        public float VelocityRepulse;

        [FieldOffset(84)]
        public float AnimationTime;

        public override string ToString()
        {
            return string.Format("Center: {0}, ScaleFromCenter: {1}, Target: {2}, MaxTimeTarget: {3}, VelocityUp: {4}, MaxTimeUp: {5}, BulletSize: {6}, Velocity: {7}", Center, ScaleFromCenter, Target, MaxTimeTarget, VelocityUp, MaxTimeUp, BulletSize, VelocityTarget);
        }
    };
}