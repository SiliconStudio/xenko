using System.Collections.Generic;
using Android.Hardware;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class AccelerometerAndroid : SensorAndroid, IAccelerometerSensor
    {
        public Vector3 Acceleration => acceleration;
        private Vector3 acceleration;

        public AccelerometerAndroid() : base(SensorType.Accelerometer)
        {
        }
        public override void UpdateSensorData(IReadOnlyList<float> newValues)
        {
            acceleration = this.AsVector();
        }

    }
}