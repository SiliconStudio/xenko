// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS

using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.CoreMotion;
using OpenTK.Platform.iPhoneOS;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private UIWindow window;
        private iPhoneOSGameView view;
        private CMMotionManager motionManager;
        private CLLocationManager locationManager;

        public InputManager(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = false;
            HasPointer = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            view = Game.Context.GameView;
            window = Game.Context.MainWindow;

            var gameController = Game.Context.GameViewController;

            window.UserInteractionEnabled = true;
            window.MultipleTouchEnabled = true;
            gameController.TouchesBeganDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesMovedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesEndedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesCancelledDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            view.Resize += OnResize;

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

            if(Compass.ShouldBeEnabled)
                locationManager.StartUpdatingHeading();

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

            if (Compass.ShouldBeDisabled)
                locationManager.StopUpdatingHeading();

            if (Gyroscope.ShouldBeDisabled)
                motionManager.StopGyroUpdates();

            if ((UserAcceleration.ShouldBeDisabled || Gravity.ShouldBeDisabled || Orientation.ShouldBeDisabled) && !UserAcceleration.IsEnabled && !Gravity.IsEnabled && !Orientation.IsEnabled)
                motionManager.StopDeviceMotionUpdates();
        }

        private static Vector3 CmAccelerationToVector3(CMAcceleration acceleration)
        {
            return new Vector3((float)acceleration.X, (float)acceleration.Y, (float)acceleration.Z);
        }

        internal override void UpdateEnabledSensorsData()
        {
            base.UpdateEnabledSensorsData();

            if (Accelerometer.IsEnabled)
                Accelerometer.Acceleration = CmAccelerationToVector3(motionManager.AccelerometerData.Acceleration);

            if (Compass.IsEnabled)
                Compass.Heading = (float)locationManager.Heading.MagneticHeading;

            if (Gyroscope.IsEnabled)
            {
                var rate = motionManager.GyroData.RotationRate;
                Gyroscope.RotationRate = new Vector3((float)rate.x, (float)rate.y, (float)rate.z);
            }

            if(UserAcceleration.IsEnabled)
                UserAcceleration.Acceleration = CmAccelerationToVector3(motionManager.DeviceMotion.UserAcceleration);

            if (Gravity.IsEnabled)
                Gravity.Vector = CmAccelerationToVector3(motionManager.DeviceMotion.Gravity);

            if (Orientation.IsEnabled)
            {
                var attitude = motionManager.DeviceMotion.Attitude;

                Orientation.Yaw = (float)attitude.Yaw;
                Orientation.Pitch = (float)attitude.Pitch;
                Orientation.Roll = (float)attitude.Roll;

                var quaternion = attitude.Quaternion;
                Orientation.Quaternion = new Quaternion((float)quaternion.x, (float)quaternion.y, (float)quaternion.z, (float)quaternion.w);

                var matrix = attitude.RotationMatrix;
                var rotationMatrix = Matrix.Identity;
                rotationMatrix.M11 = (float)matrix.m11;
                rotationMatrix.M21 = (float)matrix.m12;
                rotationMatrix.M31 = (float)matrix.m13;
                rotationMatrix.M12 = (float)matrix.m21;
                rotationMatrix.M22 = (float)matrix.m22;
                rotationMatrix.M32 = (float)matrix.m23;
                rotationMatrix.M13 = (float)matrix.m31;
                rotationMatrix.M23 = (float)matrix.m32;
                rotationMatrix.M33 = (float)matrix.m33;
                Orientation.RotationMatrix = rotationMatrix;
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
            ControlHeight = view.Frame.Height;
            ControlWidth = view.Frame.Width;
        }

        private void HandleTouches(NSSet touchesSet)
        {
            var touches = touchesSet.ToArray<UITouch>();

            if (touches != null)
            {
                foreach (var uitouch in touches)
                {
                    var id = uitouch.Handle.ToInt32();
                    var position = NormalizeScreenPosition(PointFToVector2(uitouch.LocationInView(view)));

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

        private Vector2 PointFToVector2(PointF point)
        {
            return new Vector2(point.X, point.Y);
        }

        public override bool MultiTouchEnabled
        {
            get { return Game.Context.GameView.MultipleTouchEnabled; } 
            set { Game.Context.GameView.MultipleTouchEnabled = value; }
        }
    }
}
#endif