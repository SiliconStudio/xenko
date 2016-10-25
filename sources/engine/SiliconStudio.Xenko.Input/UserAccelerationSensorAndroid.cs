using System.Collections.Generic;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class UserAccelerationSensorAndroid : SensorAndroid, IUserAccelerationSensor
    {
        public Vector3 Acceleration => acceleration;
        private Vector3 acceleration;

        public UserAccelerationSensorAndroid() : base(SensorType.LinearAcceleration)
        {
        }
        public override void UpdateSensorData(IReadOnlyList<float> newValues)
        {
            acceleration = this.AsVector();
        }

    }
}