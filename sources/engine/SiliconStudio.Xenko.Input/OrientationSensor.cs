// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal class OrientationSensor : Sensor, IOrientationSensor
    {
        private float yaw;
        private float pitch;
        private float roll;
        private Quaternion quaternion;
        private Matrix rotationMatrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrientationSensor"/> class.
        /// </summary>
        public OrientationSensor(IInputSource source, string systemName) : base(source, systemName, "Orientation")
        {
        }

        public float Yaw => yaw;

        public float Pitch => pitch;

        public float Roll => roll;

        public Quaternion Quaternion => quaternion;

        public Matrix RotationMatrix => rotationMatrix;

        public void FromQuaternion(Quaternion q)
        {
            quaternion = q;
            rotationMatrix = Matrix.RotationQuaternion(quaternion);
            
            yaw = (float)Math.Asin(2 * (q.W * q.Y - q.Z * q.X));
            pitch = (float)Math.Atan2(2 * (q.W * q.X + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y));
            roll = (float)Math.Atan2(2 * (q.W * q.Z + q.X * q.Y), 1 - 2 * (q.Y * q.Y + q.Z * q.Z));
        }

        public void Reset()
        {
            yaw = 0;
            pitch = 0;
            roll = 0;
            quaternion = Quaternion.Identity;
            rotationMatrix = Matrix.Identity;
        }
    }
}