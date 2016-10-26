// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using System.Collections.Generic;
using System.Linq;
using CoreLocation;
using CoreMotion;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    public class InputSourceiOS : InputSourceBase
    {
        private CMMotionManager motionManager;
        private CLLocationManager locationManager;
        private bool locationManagerActivated;
        private float firstNorthValue = float.NegativeInfinity;
        private PointeriOS pointer;

        private NamedAccelerometerSensor accelerometerSensor;
        private NamedUserAccelerationSensor userAccelerationSensor;
        private NamedGyroscopeSensor gyroscopeSensor;
        private NamedOrientationSensor orientationSensor;
        private NamedGravitySensor gravitySensor;
        private NamedCompassSensor compassSensor;
        private List<NamedSensor> sensors = new List<NamedSensor>();
        
        public override void Initialize(InputManager inputManager)
        {
            var context = inputManager.Game.Context as GameContextiOS;
            var uiControl = context.Control;
            var gameController = context.Control.GameViewController;

            pointer = new PointeriOS(uiControl, gameController);
            RegisterDevice(pointer);

            // Create sensor managers
            motionManager = new CMMotionManager();
            locationManager = new CLLocationManager();

            // Set desired sensor sampling intervals 
            double updateInterval = 1/InputManager.DesiredSensorUpdateRate;
            motionManager.AccelerometerUpdateInterval = updateInterval;
            motionManager.GyroUpdateInterval = updateInterval;
            motionManager.DeviceMotionUpdateInterval = updateInterval;

            // Determine supported sensors
            if (motionManager.AccelerometerAvailable)
            {
                accelerometerSensor = new NamedAccelerometerSensor("iOS");
                RegisterDevice(accelerometerSensor);
            }
            if (CLLocationManager.HeadingAvailable)
            {
                compassSensor = new NamedCompassSensor("iOS");
                RegisterDevice(compassSensor);
            }
            if (motionManager.GyroAvailable)
            {
                gyroscopeSensor = new NamedGyroscopeSensor("iOS");
                RegisterDevice(gyroscopeSensor);
            }
            if (motionManager.DeviceMotionAvailable)
            {
                gravitySensor = new NamedGravitySensor("iOS");
                userAccelerationSensor = new NamedUserAccelerationSensor("iOS");
                orientationSensor = new NamedOrientationSensor("iOS");
                RegisterDevice(gravitySensor);
                RegisterDevice(userAccelerationSensor);
                RegisterDevice(orientationSensor);
            }
            sensors.AddRange(registeredInputDevices.OfType<NamedSensor>());
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext.ContextType == AppContextType.iOS;
        }

        public override void Update()
        {
            base.Update();

            // Enable/disable supported sensors and update enabled sensors
            if (accelerometerSensor != null)
            {
                bool enable = accelerometerSensor.IsEnabled;
                if (enable != motionManager.AccelerometerActive)
                {
                    if(accelerometerSensor.IsEnabled)
                        motionManager.StartAccelerometerUpdates();
                    else
                        motionManager.StopAccelerometerUpdates();
                }
                if (enable)
                {
                    var accelerometerData = motionManager.AccelerometerData;
                    accelerometerSensor.AccelerationInternal = accelerometerData != null ? CmAccelerationToVector3(accelerometerData.Acceleration) : Vector3.Zero;
                }
            }
            if (compassSensor != null)
            {
                bool enable = compassSensor.IsEnabled;
                if (enable != locationManagerActivated)
                {
                    if (compassSensor.IsEnabled)
                        locationManager.StartUpdatingHeading();
                    else
                        locationManager.StopUpdatingHeading();
                    locationManagerActivated = compassSensor.IsEnabled;
                }
                if (enable)
                {
                    compassSensor.HeadingInternal = GetNorthInRadian(locationManager);
                }
            }
            if (gyroscopeSensor != null)
            {
                bool enable = gyroscopeSensor.IsEnabled;
                if (enable != motionManager.GyroActive)
                {
                    if (gyroscopeSensor.IsEnabled)
                        motionManager.StartGyroUpdates();
                    else
                        motionManager.StopGyroUpdates();
                }
                if (enable)
                {
                    var gyroData = motionManager.GyroData;
                    gyroscopeSensor.RotationRateInternal = gyroData != null ? CmRotationRateToVector3(gyroData.RotationRate) : Vector3.Zero;
                }
            }
            if (userAccelerationSensor != null)
            {
                bool enable = userAccelerationSensor.IsEnabled || gravitySensor.IsEnabled || orientationSensor.IsEnabled;
                if (enable != motionManager.DeviceMotionActive)
                {
                    if (enable)
                        motionManager.StartDeviceMotionUpdates();
                    else
                        motionManager.StopDeviceMotionUpdates();
                }
                if (enable)
                {
                    var motion = motionManager.DeviceMotion;
                    // Update orientation sensor
                    if (motion != null && motion.Attitude != null)
                    {
                        var q = motionManager.DeviceMotion.Attitude.Quaternion;
                        var quaternion = new Quaternion((float)q.x, (float)q.z, -(float)q.y, (float)q.w);

                        if (compassSensor != null)
                            // re-adjust the orientation to align with the north (common behavior on other platforms) TODO current implementation only takes in account the first value.
                        {
                            if (firstNorthValue <= 0)
                                firstNorthValue = GetNorthInRadian(locationManager);

                            quaternion = Quaternion.RotationY(-firstNorthValue)*quaternion;
                        }

                        orientationSensor.FromQuaternion(quaternion);
                    }
                    else
                    {
                        orientationSensor.Reset();
                    }

                    // Update gravity sensor
                    gravitySensor.VectorInternal = motion != null ? CmAccelerationToVector3(motion.Gravity) : Vector3.Zero;

                    // Update user acceleration
                    userAccelerationSensor.AccelerationInternal = motion != null ? CmAccelerationToVector3(motion.Gravity) : Vector3.Zero;
                }
            }
        }

        public override void Pause()
        {
            motionManager.StopAccelerometerUpdates();
            locationManager.StopUpdatingHeading();
            motionManager.StopGyroUpdates();
            motionManager.StopDeviceMotionUpdates();
            locationManagerActivated = false;
        }

        public override void Resume()
        {
            // Automatic resume when update is called
        }

        private static Vector3 CmAccelerationToVector3(CMAcceleration acceleration)
        {
            return InputManager.G*new Vector3((float)acceleration.X, (float)acceleration.Z, -(float)acceleration.Y);
        }

        private static Vector3 CmRotationRateToVector3(CMRotationRate rate)
        {
            return new Vector3((float)rate.x, (float)rate.z, -(float)rate.y);
        }

        private static float GetNorthInRadian(CLLocationManager location)
        {
            var heading = location.Heading;
            if (heading == null)
                return 0f;

            var headingDegree = heading.TrueHeading < 0 ? heading.MagneticHeading : heading.TrueHeading; // TrueHeading is set to a negative value when invalid
            return MathUtil.DegreesToRadians((float)headingDegree);
        }
    }
}

#endif