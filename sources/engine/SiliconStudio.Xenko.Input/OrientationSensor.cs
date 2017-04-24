// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// This class represents a sensor of type Orientation. It measures the orientation of device in the real world.
    /// </summary>
    public class OrientationSensor : SensorBase
    {
        /// <summary>
        /// Gets the value of the yaw (in radian). The yaw is the rotation around the vertical axis of the device, that is the Oz axis.
        /// </summary>
        public float Yaw { get; internal set; }

        /// <summary>
        /// Gets the value of the pitch (in radian). The pitch is the rotation around the lateral axis of the device, that is the Ox axis.
        /// </summary>
        public float Pitch { get; internal set; }

        /// <summary>
        /// Gets the value of the roll (in radian). The roll is the rotation around the longitudinal axis of the device, that is the Oy axis.
        /// </summary>
        public float Roll { get; internal set; }

        /// <summary>
        /// Gets the quaternion specifying the current rotation of the device.
        /// </summary>
        public Quaternion Quaternion { get; internal set; }

        /// <summary>
        /// Gets the rotation matrix specifying the current rotation of the device.
        /// </summary>
        public Matrix RotationMatrix { get; internal set; }

        internal override void ResetData()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
            Quaternion = Quaternion.Identity;
            RotationMatrix = Matrix.Identity;
        }

        internal void FromQuaternion(Quaternion q)
        {
            Quaternion = q;
            RotationMatrix = Matrix.RotationQuaternion(q);
            
            Yaw = (float)Math.Asin(2 * (q.W * q.Y - q.Z * q.X));
            Pitch = (float)Math.Atan2(2 * (q.W * q.X + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y)); 
            Roll = (float)Math.Atan2(2 * (q.W * q.Z + q.X * q.Y), 1 - 2 * (q.Y * q.Y + q.Z * q.Z));
        }
    }
}
