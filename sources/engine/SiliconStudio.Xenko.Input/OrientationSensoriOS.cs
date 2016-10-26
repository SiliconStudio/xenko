// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class OrientationSensoriOS : SensoriOS, IOrientationSensor
    {
        public float Yaw => yaw;
        public float Pitch => pitch;
        public float Roll => roll;
        public Quaternion Quaternion => quaternion;
        public Matrix RotationMatrix => rotationMatrix;

        private float yaw;
        private float pitch;
        private float roll;
        private Quaternion quaternion;
        private Matrix rotationMatrix;

        public OrientationSensoriOS() : base("Orientation")
        {
        }

        internal void FromQuaternion(Quaternion q)
        {
            quaternion = q;
            rotationMatrix = Matrix.RotationQuaternion(quaternion);
            
            yaw = (float)Math.Asin(2 * (q.W * q.Y - q.Z * q.X));
            pitch = (float)Math.Atan2(2 * (q.W * q.X + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y));
            roll = (float)Math.Atan2(2 * (q.W * q.Z + q.X * q.Y), 1 - 2 * (q.Y * q.Y + q.Z * q.Z));
        }

        internal void Reset()
        {
            yaw = 0;
            pitch = 0;
            roll = 0;
            quaternion = Quaternion.Identity;
            rotationMatrix = Matrix.Identity;
        }
    }
}
#endif