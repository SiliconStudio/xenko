// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Linq;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class OrientationSensorAndroid : SensorAndroid, IOrientationSensor
    {
        public float Yaw => yaw;
        public float Pitch => pitch;
        public float Roll => roll;
        public Quaternion Quaternion => quaternion;
        public Matrix RotationMatrix => rotationMatrix;

        internal readonly float[] YawPitchRollArray = new float[3];
        private readonly float[] quaternionArray = new float[4];
        private readonly float[] rotationMatrixArray = new float[9];

        public float yaw;
        public float pitch;
        public float roll;
        public Quaternion quaternion;
        public Matrix rotationMatrix;

        public OrientationSensorAndroid() : base(SensorType.RotationVector)
        {
        }

        public override void Update()
        {
            base.Update();
            float[] rotationVector = Values.ToArray();
            if (rotationVector.Length < 3)
                return;
            SensorManager.GetQuaternionFromVector(quaternionArray, rotationVector);
            SensorManager.GetRotationMatrixFromVector(rotationMatrixArray, rotationVector);
            SensorManager.GetOrientation(rotationMatrixArray, YawPitchRollArray);

            quaternion = Quaternion.Identity;
            quaternion.W = +quaternionArray[0];
            quaternion.X = +quaternionArray[1];
            quaternion.Y = +quaternionArray[3];
            quaternion.Z = -quaternionArray[2];
            quaternion = Quaternion.RotationY(MathUtil.Pi)*quaternion; // align the orientation with north.
            rotationMatrix = Matrix.RotationQuaternion(quaternion);

            var q = quaternion;
            yaw = (float)Math.Asin(2*(q.W*q.Y - q.Z*q.X));
            pitch = (float)Math.Atan2(2*(q.W*q.X + q.Y*q.Z), 1 - 2*(q.X*q.X + q.Y*q.Y));
            roll = (float)Math.Atan2(2*(q.W*q.Z + q.X*q.Y), 1 - 2*(q.Y*q.Y + q.Z*q.Z));
        }
    }
}

#endif