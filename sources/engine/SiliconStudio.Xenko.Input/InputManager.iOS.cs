// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using System;
using CoreGraphics;
using UIKit;
using Foundation;
using CoreLocation;
using CoreMotion;
using OpenTK.Platform.iPhoneOS;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    internal class InputManageriOS: InputManager<iOSWindow>
    {		
        private CMMotionManager motionManager;
        private CLLocationManager locationManager;
        private bool locationManagerActivated;
        private float firstNorthValue = float.NegativeInfinity;

        public InputManageriOS(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = false;
            HasPointer = true;
        }

        public override void Initialize(GameContext<iOSWindow> gameContext)
        {
            UiControl = gameContext.Control;

            var gameController = gameContext.Control.GameViewController;

            var window = UiControl.MainWindow;
            window.UserInteractionEnabled = true;
            window.MultipleTouchEnabled = true;
            gameController.TouchesBeganDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesMovedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesEndedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesCancelledDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            UiControl.GameView.Resize += OnResize;

            OnResize(null, EventArgs.Empty);

            // create sensor managers
            motionManager = new CMMotionManager();
            locationManager = new CLLocationManager();

            // set desired sampling intervals 
            motionManager.AccelerometerUpdateInterval = 1/DesiredSensorUpdateRate;
            motionManager.GyroUpdateInterval = 1/DesiredSensorUpdateRate;
            motionManager.DeviceMotionUpdateInterval = 1/DesiredSensorUpdateRate;

            // Determine supported sensors
            Accelerometer.IsSupported = motionManager.AccelerometerAvailable;
            Compass.IsSupported = CLLocationManager.HeadingAvailable;
            Gyroscope.IsSupported = motionManager.GyroAvailable;
            UserAcceleration.IsSupported = motionManager.DeviceMotionAvailable;
            Gravity.IsSupported = motionManager.DeviceMotionAvailable;
            Orientation.IsSupported = motionManager.DeviceMotionAvailable;
        }

        internal override void CheckAndEnableSensors()
        {
            base.CheckAndEnableSensors();

            if(Accelerometer.ShouldBeEnabled)
                motionManager.StartAccelerometerUpdates();

            if((Compass.ShouldBeEnabled || Orientation.ShouldBeEnabled) && !locationManagerActivated)
            {
                locationManagerActivated = true;
                locationManager.StartUpdatingHeading();
            }

            if(Gyroscope.ShouldBeEnabled)
                motionManager.StartGyroUpdates();

            if((UserAcceleration.ShouldBeEnabled || Gravity.ShouldBeEnabled || Orientation.ShouldBeEnabled) && !motionManager.DeviceMotionActive)
                motionManager.StartDeviceMotionUpdates();
        }

        internal override void CheckAndDisableSensors()
        {
            base.CheckAndDisableSensors();

            if (Accelerometer.ShouldBeDisabled)
                motionManager.StopAccelerometerUpdates();

            if ((Compass.ShouldBeDisabled || Orientation.ShouldBeDisabled) && !Compass.IsEnabled && !Orientation.IsEnabled)
            {
                locationManagerActivated = false;
                locationManager.StopUpdatingHeading();
            }

            if (Gyroscope.ShouldBeDisabled)
                motionManager.StopGyroUpdates();

            if ((UserAcceleration.ShouldBeDisabled || Gravity.ShouldBeDisabled || Orientation.ShouldBeDisabled) && !UserAcceleration.IsEnabled && !Gravity.IsEnabled && !Orientation.IsEnabled)
                motionManager.StopDeviceMotionUpdates();
        }

        private static Vector3 CmAccelerationToVector3(CMAcceleration acceleration)
        {
            return G * new Vector3((float)acceleration.X, (float)acceleration.Z, -(float)acceleration.Y);
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

        internal override void UpdateEnabledSensorsData()
        {
            base.UpdateEnabledSensorsData();

            if (Accelerometer.IsEnabled)
            {
                var accelerometerData = motionManager.AccelerometerData;
                Accelerometer.Acceleration = accelerometerData != null? CmAccelerationToVector3(accelerometerData.Acceleration): Vector3.Zero;
            }

            if (Compass.IsEnabled)
            {
                Compass.Heading = GetNorthInRadian(locationManager);
            }

            if (Gyroscope.IsEnabled)
            {
                var gyroData = motionManager.GyroData;
                Gyroscope.RotationRate = gyroData != null? CmRotationRateToVector3(gyroData.RotationRate): Vector3.Zero;
            }

            if (UserAcceleration.IsEnabled)
            {
                var motion = motionManager.DeviceMotion;
                UserAcceleration.Acceleration = motion != null? CmAccelerationToVector3(motion.UserAcceleration): Vector3.Zero;
            }

            if (Gravity.IsEnabled)
            {
                var motion = motionManager.DeviceMotion;
                Gravity.Vector = motion != null? CmAccelerationToVector3(motion.Gravity) : Vector3.Zero;
            }

            if (Orientation.IsEnabled)
            {
                var motion = motionManager.DeviceMotion;
                if (motion != null && motion.Attitude != null)
                {
                    var q = motionManager.DeviceMotion.Attitude.Quaternion;
                    var quaternion = new Quaternion((float)q.x, (float)q.z, -(float)q.y, (float)q.w);

                    if (Compass.IsSupported) // re-adjust the orientation to align with the north (common behavior on other platforms) TODO current implementation only takes in account the first value.
                    {
                        if(firstNorthValue <= 0)
                            firstNorthValue = GetNorthInRadian(locationManager);

                        quaternion = Quaternion.RotationY(-firstNorthValue) * quaternion;
                    }

                    Orientation.FromQuaternion(quaternion);
                }
                else
                {
                    Orientation.ResetData();
                }
            }
        }

        public override void OnApplicationPaused(object sender, EventArgs e)
        {
            base.OnApplicationPaused(sender, e);

            if (Accelerometer.IsEnabled)
                motionManager.StartAccelerometerUpdates();

            if (Compass.IsEnabled)
                locationManager.StartUpdatingHeading();

            if (Gyroscope.IsEnabled)
                motionManager.StartGyroUpdates();

            if (UserAcceleration.IsEnabled || Gravity.IsEnabled || Orientation.IsEnabled)
                motionManager.StartDeviceMotionUpdates();
        }

        public override void OnApplicationResumed(object sender, EventArgs e)
        {
            base.OnApplicationResumed(sender, e);

            if (Accelerometer.IsEnabled)
                motionManager.StopAccelerometerUpdates();

            if (Compass.IsEnabled)
                locationManager.StopUpdatingHeading();

            if (Gyroscope.IsEnabled)
                motionManager.StopGyroUpdates();

            if (UserAcceleration.IsEnabled || Gravity.IsEnabled || Orientation.IsEnabled)
                motionManager.StopDeviceMotionUpdates();
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            ControlHeight = (float)UiControl.GameView.Frame.Height;
            ControlWidth = (float)UiControl.GameView.Frame.Width;
        }

        private void HandleTouches(NSSet touchesSet)
        {
            var touches = touchesSet.ToArray<UITouch>();

            if (touches != null)
            {
                foreach (var uitouch in touches)
                {
                    var id = uitouch.Handle.ToInt32();
                    var position = NormalizeScreenPosition(CGPointToVector2(uitouch.LocationInView(UiControl.GameView)));

                    HandlePointerEvents(id, position, GetState(uitouch));
                }
            }
        }

        private PointerState GetState(UITouch touch)
        {
            switch (touch.Phase)
            {
                case UITouchPhase.Began:
                    return PointerState.Down;
                case UITouchPhase.Moved:
                case UITouchPhase.Stationary:
                    return PointerState.Move;
                case UITouchPhase.Ended:
                    return PointerState.Up;
                case UITouchPhase.Cancelled:
                    return PointerState.Cancel;
            }

            throw new ArgumentException("Got an invalid Touch event in GetState");
        }

        private Vector2 CGPointToVector2(CGPoint point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        public override bool MultiTouchEnabled
        {
            get { return UiControl.GameView.MultipleTouchEnabled; }
            set { UiControl.GameView.MultipleTouchEnabled = value; }
        }
    }
}
#endif
