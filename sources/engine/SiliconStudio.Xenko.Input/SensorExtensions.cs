using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public static class SensorExtensions
    {
        /// <summary>
        /// Returns the sensor reading as a float
        /// </summary>
        /// <param name="sensor">The sensor</param>
        /// <returns>a single float reading</returns>
        public static float AsFloat(this ISensorDevice sensor)
        {
            return sensor.Values.Count > 0 ? sensor.Values[0] : 0.0f;
        }

        /// <summary>
        /// Returns the sensor reading as a Vector3
        /// </summary>
        /// <param name="sensor">The sensor</param>
        /// <returns>a single vector3 reading</returns>
        public static Vector3 AsVector(this ISensorDevice sensor)
        {
            return sensor.Values.Count >= 3 ? new Vector3(sensor.Values[0], sensor.Values[1], sensor.Values[2]) : Vector3.Zero;
        }
    }
}