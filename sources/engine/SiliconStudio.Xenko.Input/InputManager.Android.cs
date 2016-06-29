// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using Android.Views;
using Android.Content;
using Android.Hardware;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Input
{
    internal partial class InputManagerAndroid : InputManager<AndroidXenkoGameView>
    {
        private const int SensorDesiredUpdateDelay = (int)(1 / DesiredSensorUpdateRate * 1000f);

        private SensorManager sensorManager;
        private Sensor androidAccelerometer;
        private Sensor androidGyroscope;
        private Sensor androidUserAcceleration;
        private Sensor androidGravity;
        private Sensor androidRotationVector;
        private readonly AndroidSensorListener3 accelerometerListener = new AndroidSensorListener3();
        private readonly AndroidSensorListener3 gyroscopeListener = new AndroidSensorListener3();
        private readonly AndroidSensorListener3 userAccelerationListener = new AndroidSensorListener3();
        private readonly AndroidSensorListener3 gravityListener = new AndroidSensorListener3();
        private readonly AndroidSensorListener3 rotationVectorListener = new AndroidSensorListener3();
        private readonly float[] yawPitchRollArray = new float[3];
        private readonly float[] quaternionArray = new float[4];
        private readonly float[] rotationMatrixArray = new float[9];
        private bool androidRotationVectorEnabled;

        public InputManagerAndroid(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = false;
            HasPointer = true;
        }

        public override void Initialize(GameContext<AndroidXenkoGameView> gameContext)
        {
            var viewListener = new ViewListener(this);
            UiControl = gameContext.Control;
            UiControl.SetOnTouchListener(viewListener);
            UiControl.SetOnKeyListener(viewListener);
            UiControl.Resize += GameViewOnResize;

            GameViewOnResize(null, EventArgs.Empty);

            // Get the android sensors
            sensorManager = (SensorManager)PlatformAndroid.Context.GetSystemService(Context.SensorService);
            androidAccelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            androidGyroscope = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
            androidUserAcceleration = sensorManager.GetDefaultSensor(SensorType.LinearAcceleration);
            androidGravity = sensorManager.GetDefaultSensor(SensorType.Gravity);
            androidRotationVector = sensorManager.GetDefaultSensor(SensorType.RotationVector);

            // Determine which sensor is available on the device
            Accelerometer.IsSupported = androidAccelerometer != null;
            Compass.IsSupported = androidRotationVector != null;
            Gyroscope.IsSupported = androidGyroscope != null;
            UserAcceleration.IsSupported = androidUserAcceleration != null;
            Gravity.IsSupported = androidGravity != null;
            Orientation.IsSupported = androidRotationVector != null;
        }

        internal override void CheckAndEnableSensors()
        {
            base.CheckAndEnableSensors();

            if (Accelerometer.ShouldBeEnabled)
                EnableAndroidAccelerometerSensor();

            if (Gyroscope.ShouldBeEnabled)
                EnableAndroidGyroscopeSensor();

            if (UserAcceleration.ShouldBeEnabled)
                EnableAndroidUserAccelerationSensor();

            if (Gravity.ShouldBeEnabled)
                EnableAndroidGravitySensor();

            if ((Compass.ShouldBeEnabled || Orientation.ShouldBeEnabled) && !androidRotationVectorEnabled)
                EnableAndroidRotationVectorSensor();
        }

        internal override void CheckAndDisableSensors()
        {
            base.CheckAndDisableSensors();

            if (Accelerometer.ShouldBeDisabled)
                sensorManager.UnregisterListener(accelerometerListener);

            if (Gyroscope.ShouldBeDisabled)
                sensorManager.UnregisterListener(gyroscopeListener);

            if (UserAcceleration.ShouldBeDisabled)
                sensorManager.UnregisterListener(userAccelerationListener);

            if (Gravity.ShouldBeDisabled)
                sensorManager.UnregisterListener(gravityListener);

            if ((Orientation.ShouldBeDisabled || Compass.ShouldBeDisabled) && !(Orientation.IsEnabled || Compass.IsEnabled))
            {
                androidRotationVectorEnabled = false;
                sensorManager.UnregisterListener(rotationVectorListener);
            }
        }

        internal override void UpdateEnabledSensorsData()
        {
            base.UpdateEnabledSensorsData();

            if (Accelerometer.IsEnabled)
                Accelerometer.Acceleration = accelerometerListener.GetCurrentValuesAsVector();

            if (Gyroscope.IsEnabled)
                Gyroscope.RotationRate = -gyroscopeListener.GetCurrentValuesAsVector();

            if (UserAcceleration.IsEnabled)
                UserAcceleration.Acceleration = userAccelerationListener.GetCurrentValuesAsVector();

            if (Gravity.IsEnabled)
                Gravity.Vector = gravityListener.GetCurrentValuesAsVector();

            if (Orientation.IsEnabled || Compass.IsEnabled)
            {
                var rotationVector = rotationVectorListener.GetCurrentValues();
                SensorManager.GetQuaternionFromVector(quaternionArray, rotationVector);
                SensorManager.GetRotationMatrixFromVector(rotationMatrixArray, rotationVector);
                SensorManager.GetOrientation(rotationMatrixArray, yawPitchRollArray);

                if (Orientation.IsEnabled)
                {
                    var quaternion = Quaternion.Identity;
                    quaternion.W = +quaternionArray[0];
                    quaternion.X = +quaternionArray[1];
                    quaternion.Y = +quaternionArray[3];
                    quaternion.Z = -quaternionArray[2];
                    Orientation.FromQuaternion(Quaternion.RotationY(MathUtil.Pi) * quaternion); // aligh the orientation with north.
                }

                if (Compass.IsEnabled)
                {
                    Compass.Heading = yawPitchRollArray[0] + MathUtil.Pi;
                }
            }
        }

        public override void OnApplicationPaused(object sender, EventArgs e)
        {
            base.OnApplicationPaused(sender, e);

            if (Accelerometer.IsEnabled)
                sensorManager.UnregisterListener(accelerometerListener);

            if (Gyroscope.IsEnabled)
                sensorManager.UnregisterListener(gyroscopeListener);

            if (UserAcceleration.IsEnabled)
                sensorManager.UnregisterListener(userAccelerationListener);

            if (Gravity.IsEnabled)
                sensorManager.UnregisterListener(gravityListener);

            if (Orientation.IsEnabled || Compass.IsEnabled)
            {
                androidRotationVectorEnabled = false;
                sensorManager.UnregisterListener(rotationVectorListener);
            }
        }

        public override void OnApplicationResumed(object sender, EventArgs e)
        {
            base.OnApplicationResumed(sender, e);

            if (Accelerometer.IsEnabled)
                EnableAndroidAccelerometerSensor();

            if (Gyroscope.IsEnabled)
                EnableAndroidGyroscopeSensor();

            if (UserAcceleration.IsEnabled)
                EnableAndroidUserAccelerationSensor();

            if (Gravity.IsEnabled)
                EnableAndroidGravitySensor();

            if (Compass.IsEnabled || Orientation.IsEnabled)
                EnableAndroidRotationVectorSensor();
        }

        private void EnableAndroidAccelerometerSensor()
        {
            if (!sensorManager.RegisterListener(accelerometerListener, androidAccelerometer, (SensorDelay)SensorDesiredUpdateDelay))
            {
                Accelerometer.IsEnabled = false;
                Logger.Error("Failed to start the android accelerometer sensor. Accelerometer will not work for current Game session.");
            }
        }

        private void EnableAndroidRotationVectorSensor()
        {
            if (!sensorManager.RegisterListener(rotationVectorListener, androidRotationVector, (SensorDelay)SensorDesiredUpdateDelay))
            {
                Compass.IsEnabled = false;
                Orientation.IsEnabled = false;
                Logger.Error("Failed to start the android rotation vector sensor. Compass and Orientation will not work for current Game session.");
            }
            else
            {
                androidRotationVectorEnabled = true;
            }
        }

        private void EnableAndroidGravitySensor()
        {
            if (!sensorManager.RegisterListener(gravityListener, androidGravity, (SensorDelay)SensorDesiredUpdateDelay))
            {
                Gravity.IsEnabled = false;
                Logger.Error("Failed to start the android gravity sensor. Gravity will not work for current Game session.");
            }
        }

        private void EnableAndroidUserAccelerationSensor()
        {
            if (!sensorManager.RegisterListener(userAccelerationListener, androidUserAcceleration, (SensorDelay)SensorDesiredUpdateDelay))
            {
                UserAcceleration.IsEnabled = false;
                Logger.Error("Failed to start the android linear acceleration sensor. UserAcceleration will not work for current Game session.");
            }
        }

        private void EnableAndroidGyroscopeSensor()
        {
            if (!sensorManager.RegisterListener(gyroscopeListener, androidGyroscope, (SensorDelay)SensorDesiredUpdateDelay))
            {
                Gyroscope.IsEnabled = false;
                Logger.Error("Failed to start the android gyroscope sensor. Gyroscope will not work for current Game session.");
            }
        }

        private void GameViewOnResize(object sender, EventArgs eventArgs)
        {
            ControlWidth = UiControl.Size.Width;
            ControlHeight = UiControl.Size.Height;
        }

        private bool OnTouch(MotionEvent e)
        {
            PointerState state;
            switch (e.ActionMasked)
            {
                case MotionEventActions.Cancel:
                    state = PointerState.Cancel;
                    break;
                case MotionEventActions.Move:
                    state = PointerState.Move;
                    break;
                case MotionEventActions.Outside:
                    state = PointerState.Out;
                    break;
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    state = PointerState.Down;
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    state = PointerState.Up;
                    break;
                default:
                    // Not handled
                    return false;
            }

            var startIndex = 0;
            var endIndex = e.PointerCount;

            if (state == PointerState.Down || state == PointerState.Up || state == PointerState.Out)
            {
                startIndex = e.ActionIndex;
                endIndex = startIndex + 1;
            }

            for (var i = startIndex; i < endIndex; ++i)
            {
                var pointerId = e.GetPointerId(i);
                var pixelPosition = new Vector2(e.GetX(i), e.GetY(i));

                if(MultiTouchEnabled || pointerId == 0) // manually drop multi-touch events when disabled
                    HandlePointerEvents(pointerId, NormalizeScreenPosition(pixelPosition), state);
            }

            return true;
        }

        private bool OnKey(Keycode keyCode, Android.Views.KeyEvent e)
        {
            lock (KeyboardInputEvents)
            {
                KeyboardInputEvents.Add(new KeyboardInputEvent
                    {
                        Key = ConvertKeyFromAndroid(keyCode),
                        Type = e.Action == KeyEventActions.Down ? InputEventType.Down : InputEventType.Up,
                    });
            }
            return true;
        }

        private Keys ConvertKeyFromAndroid(Keycode key)
        {
            switch (key)
            {
                case Keycode.Num0: return Keys.D0;
                case Keycode.Num1: return Keys.D1;
                case Keycode.Num2: return Keys.D2;
                case Keycode.Num3: return Keys.D3;
                case Keycode.Num4: return Keys.D4;
                case Keycode.Num5: return Keys.D5;
                case Keycode.Num6: return Keys.D6;
                case Keycode.Num7: return Keys.D7;
                case Keycode.Num8: return Keys.D8;
                case Keycode.Num9: return Keys.D9;
                case Keycode.A: return Keys.A;
                case Keycode.B: return Keys.B;
                case Keycode.C: return Keys.C;
                case Keycode.D: return Keys.D;
                case Keycode.E: return Keys.E;
                case Keycode.F: return Keys.F;
                case Keycode.G: return Keys.G;
                case Keycode.H: return Keys.H;
                case Keycode.I: return Keys.I;
                case Keycode.J: return Keys.J;
                case Keycode.K: return Keys.K;
                case Keycode.L: return Keys.L;
                case Keycode.M: return Keys.M;
                case Keycode.N: return Keys.N;
                case Keycode.O: return Keys.O;
                case Keycode.P: return Keys.P;
                case Keycode.Q: return Keys.Q;
                case Keycode.R: return Keys.R;
                case Keycode.S: return Keys.S;
                case Keycode.T: return Keys.T;
                case Keycode.U: return Keys.U;
                case Keycode.V: return Keys.V;
                case Keycode.W: return Keys.W;
                case Keycode.X: return Keys.X;
                case Keycode.Y: return Keys.Y;
                case Keycode.Z: return Keys.Z;
                case Keycode.AltLeft: return Keys.LeftAlt;
                case Keycode.AltRight: return Keys.RightAlt;
                case Keycode.ShiftLeft: return Keys.LeftShift;
                case Keycode.ShiftRight: return Keys.RightShift;
                case Keycode.Enter: return Keys.Enter;
                case Keycode.Back: return Keys.Back;
                case Keycode.Tab: return Keys.Tab;
                case Keycode.Del: return Keys.Delete;
                case Keycode.PageUp: return Keys.PageUp;
                case Keycode.PageDown: return Keys.PageDown;
                case Keycode.DpadUp: return Keys.Up;
                case Keycode.DpadDown: return Keys.Down;
                case Keycode.DpadLeft: return Keys.Right;
                case Keycode.DpadRight: return Keys.Right;
                default:
                    return (Keys)(-1);
            }
        }

        class ViewListener : Java.Lang.Object, View.IOnTouchListener, View.IOnKeyListener
        {
            private readonly InputManagerAndroid inputManager;

            public ViewListener(InputManagerAndroid inputManager)
            {
                this.inputManager = inputManager;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                return inputManager.OnTouch(e);
            }

            public bool OnKey(View v, Keycode keyCode, Android.Views.KeyEvent e)
            {
                return inputManager.OnKey(keyCode, e);
            }
        }

        // No easy way to enable/disable multi-touch on android so we drop them manually in OnTouch function
        public override bool MultiTouchEnabled { get; set; }

        private class AndroidSensorListener : Java.Lang.Object, ISensorEventListener
        {
            private readonly float[] lastValues;
            private readonly float[] lastQueriedValues;
            private readonly object lastValuesLock = new object();

            public AndroidSensorListener(int arraySize)
            {
                lastValues = new float[arraySize];
                lastQueriedValues = new float[arraySize];
            }

            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
            }

            public void OnSensorChanged(SensorEvent e)
            {
                for (int i = 0; i < lastValues.Length; i++)
                    lastValues[i] = e.Values[i];
            }

            public float[] GetCurrentValues()
            {
                lock (lastValuesLock)
                {
                    for (int i = 0; i < lastValues.Length; i++)
                        lastQueriedValues[i] = lastValues[i];
                }
                return lastQueriedValues;
            }
        }

        private class AndroidSensorListener3 : AndroidSensorListener
        {
            public AndroidSensorListener3() : base(3)
            {
            }

            public Vector3 GetCurrentValuesAsVector()
            {
                var values = GetCurrentValues();

                return new Vector3(-values[0], -values[2], values[1]);
            }
        }
    }
}
#endif
