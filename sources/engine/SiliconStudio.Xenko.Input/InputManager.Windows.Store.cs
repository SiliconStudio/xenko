// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.Collections.Generic;
using Windows.Devices.Input;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Point = Windows.Foundation.Point;
using WinRTPointerDeviceType = Windows.Devices.Input.PointerDeviceType;
using WinRTPointerPoint = Windows.UI.Input.PointerPoint;
using WindowsAccelerometer = Windows.Devices.Sensors.Accelerometer;
using WindowsGyroscope = Windows.Devices.Sensors.Gyrometer;
using WindowsOrientation = Windows.Devices.Sensors.OrientationSensor;
using WindowsCompass = Windows.Devices.Sensors.Compass;
using Matrix = SiliconStudio.Core.Mathematics.Matrix;

namespace SiliconStudio.Xenko.Input
{
    internal partial class InputManagerWindowsRuntime : InputManager
    {

        private const uint DesiredSensorUpdateIntervalMs = (uint)(1f/DesiredSensorUpdateRate*1000f);

        // mapping between WinRT keys and toolkit keys
        private static readonly Dictionary<VirtualKey, Keys> mapKeys;
        private static readonly MouseCapabilities mouseCapabilities = new MouseCapabilities();

        private WindowsAccelerometer windowsAccelerometer;
        private WindowsCompass windowsCompass;
        private WindowsGyroscope windowsGyroscope;
        private WindowsOrientation windowsOrientation;
        // TODO: Support for MultiTouchEnabled on Windows Runtime
        public override bool MultiTouchEnabled { get { return true; } set { } }

        static InputManagerWindowsRuntime()
        {
            mapKeys = new Dictionary<VirtualKey, Keys>();
            // this dictionary was built from Desktop version (VirtualKey are compatible with WinForms keys)
            AddKeys(WinFormsKeys.None, Keys.None);
            AddKeys(WinFormsKeys.Cancel, Keys.Cancel);
            AddKeys(WinFormsKeys.Back, Keys.Back);
            AddKeys(WinFormsKeys.Tab, Keys.Tab);
            AddKeys(WinFormsKeys.LineFeed, Keys.LineFeed);
            AddKeys(WinFormsKeys.Clear, Keys.Clear);
            AddKeys(WinFormsKeys.Enter, Keys.Enter);
            AddKeys(WinFormsKeys.Return, Keys.Return);
            AddKeys(WinFormsKeys.Pause, Keys.Pause);
            AddKeys(WinFormsKeys.Capital, Keys.Capital);
            AddKeys(WinFormsKeys.CapsLock, Keys.CapsLock);
            AddKeys(WinFormsKeys.HangulMode, Keys.HangulMode);
            AddKeys(WinFormsKeys.KanaMode, Keys.KanaMode);
            AddKeys(WinFormsKeys.JunjaMode, Keys.JunjaMode);
            AddKeys(WinFormsKeys.FinalMode, Keys.FinalMode);
            AddKeys(WinFormsKeys.HanjaMode, Keys.HanjaMode);
            AddKeys(WinFormsKeys.KanjiMode, Keys.KanjiMode);
            AddKeys(WinFormsKeys.Escape, Keys.Escape);
            AddKeys(WinFormsKeys.IMEConvert, Keys.ImeConvert);
            AddKeys(WinFormsKeys.IMENonconvert, Keys.ImeNonConvert);
            AddKeys(WinFormsKeys.IMEAccept, Keys.ImeAccept);
            AddKeys(WinFormsKeys.IMEModeChange, Keys.ImeModeChange);
            AddKeys(WinFormsKeys.Space, Keys.Space);
            AddKeys(WinFormsKeys.PageUp, Keys.PageUp);
            AddKeys(WinFormsKeys.Prior, Keys.Prior);
            AddKeys(WinFormsKeys.Next, Keys.Next);
            AddKeys(WinFormsKeys.PageDown, Keys.PageDown);
            AddKeys(WinFormsKeys.End, Keys.End);
            AddKeys(WinFormsKeys.Home, Keys.Home);
            AddKeys(WinFormsKeys.Left, Keys.Left);
            AddKeys(WinFormsKeys.Up, Keys.Up);
            AddKeys(WinFormsKeys.Right, Keys.Right);
            AddKeys(WinFormsKeys.Down, Keys.Down);
            AddKeys(WinFormsKeys.Select, Keys.Select);
            AddKeys(WinFormsKeys.Print, Keys.Print);
            AddKeys(WinFormsKeys.Execute, Keys.Execute);
            AddKeys(WinFormsKeys.PrintScreen, Keys.PrintScreen);
            AddKeys(WinFormsKeys.Snapshot, Keys.Snapshot);
            AddKeys(WinFormsKeys.Insert, Keys.Insert);
            AddKeys(WinFormsKeys.Delete, Keys.Delete);
            AddKeys(WinFormsKeys.Help, Keys.Help);
            AddKeys(WinFormsKeys.D0, Keys.D0);
            AddKeys(WinFormsKeys.D1, Keys.D1);
            AddKeys(WinFormsKeys.D2, Keys.D2);
            AddKeys(WinFormsKeys.D3, Keys.D3);
            AddKeys(WinFormsKeys.D4, Keys.D4);
            AddKeys(WinFormsKeys.D5, Keys.D5);
            AddKeys(WinFormsKeys.D6, Keys.D6);
            AddKeys(WinFormsKeys.D7, Keys.D7);
            AddKeys(WinFormsKeys.D8, Keys.D8);
            AddKeys(WinFormsKeys.D9, Keys.D9);
            AddKeys(WinFormsKeys.A, Keys.A);
            AddKeys(WinFormsKeys.B, Keys.B);
            AddKeys(WinFormsKeys.C, Keys.C);
            AddKeys(WinFormsKeys.D, Keys.D);
            AddKeys(WinFormsKeys.E, Keys.E);
            AddKeys(WinFormsKeys.F, Keys.F);
            AddKeys(WinFormsKeys.G, Keys.G);
            AddKeys(WinFormsKeys.H, Keys.H);
            AddKeys(WinFormsKeys.I, Keys.I);
            AddKeys(WinFormsKeys.J, Keys.J);
            AddKeys(WinFormsKeys.K, Keys.K);
            AddKeys(WinFormsKeys.L, Keys.L);
            AddKeys(WinFormsKeys.M, Keys.M);
            AddKeys(WinFormsKeys.N, Keys.N);
            AddKeys(WinFormsKeys.O, Keys.O);
            AddKeys(WinFormsKeys.P, Keys.P);
            AddKeys(WinFormsKeys.Q, Keys.Q);
            AddKeys(WinFormsKeys.R, Keys.R);
            AddKeys(WinFormsKeys.S, Keys.S);
            AddKeys(WinFormsKeys.T, Keys.T);
            AddKeys(WinFormsKeys.U, Keys.U);
            AddKeys(WinFormsKeys.V, Keys.V);
            AddKeys(WinFormsKeys.W, Keys.W);
            AddKeys(WinFormsKeys.X, Keys.X);
            AddKeys(WinFormsKeys.Y, Keys.Y);
            AddKeys(WinFormsKeys.Z, Keys.Z);
            AddKeys(WinFormsKeys.LWin, Keys.LeftWin);
            AddKeys(WinFormsKeys.RWin, Keys.RightWin);
            AddKeys(WinFormsKeys.Apps, Keys.Apps);
            AddKeys(WinFormsKeys.Sleep, Keys.Sleep);
            AddKeys(WinFormsKeys.NumPad0, Keys.NumPad0);
            AddKeys(WinFormsKeys.NumPad1, Keys.NumPad1);
            AddKeys(WinFormsKeys.NumPad2, Keys.NumPad2);
            AddKeys(WinFormsKeys.NumPad3, Keys.NumPad3);
            AddKeys(WinFormsKeys.NumPad4, Keys.NumPad4);
            AddKeys(WinFormsKeys.NumPad5, Keys.NumPad5);
            AddKeys(WinFormsKeys.NumPad6, Keys.NumPad6);
            AddKeys(WinFormsKeys.NumPad7, Keys.NumPad7);
            AddKeys(WinFormsKeys.NumPad8, Keys.NumPad8);
            AddKeys(WinFormsKeys.NumPad9, Keys.NumPad9);
            AddKeys(WinFormsKeys.Multiply, Keys.Multiply);
            AddKeys(WinFormsKeys.Add, Keys.Add);
            AddKeys(WinFormsKeys.Separator, Keys.Separator);
            AddKeys(WinFormsKeys.Subtract, Keys.Subtract);
            AddKeys(WinFormsKeys.Decimal, Keys.Decimal);
            AddKeys(WinFormsKeys.Divide, Keys.Divide);
            AddKeys(WinFormsKeys.F1, Keys.F1);
            AddKeys(WinFormsKeys.F2, Keys.F2);
            AddKeys(WinFormsKeys.F3, Keys.F3);
            AddKeys(WinFormsKeys.F4, Keys.F4);
            AddKeys(WinFormsKeys.F5, Keys.F5);
            AddKeys(WinFormsKeys.F6, Keys.F6);
            AddKeys(WinFormsKeys.F7, Keys.F7);
            AddKeys(WinFormsKeys.F8, Keys.F8);
            AddKeys(WinFormsKeys.F9, Keys.F9);
            AddKeys(WinFormsKeys.F10, Keys.F10);
            AddKeys(WinFormsKeys.F11, Keys.F11);
            AddKeys(WinFormsKeys.F12, Keys.F12);
            AddKeys(WinFormsKeys.F13, Keys.F13);
            AddKeys(WinFormsKeys.F14, Keys.F14);
            AddKeys(WinFormsKeys.F15, Keys.F15);
            AddKeys(WinFormsKeys.F16, Keys.F16);
            AddKeys(WinFormsKeys.F17, Keys.F17);
            AddKeys(WinFormsKeys.F18, Keys.F18);
            AddKeys(WinFormsKeys.F19, Keys.F19);
            AddKeys(WinFormsKeys.F20, Keys.F20);
            AddKeys(WinFormsKeys.F21, Keys.F21);
            AddKeys(WinFormsKeys.F22, Keys.F22);
            AddKeys(WinFormsKeys.F23, Keys.F23);
            AddKeys(WinFormsKeys.F24, Keys.F24);
            AddKeys(WinFormsKeys.NumLock, Keys.NumLock);
            AddKeys(WinFormsKeys.Scroll, Keys.Scroll);
            AddKeys(WinFormsKeys.LShiftKey, Keys.LeftShift);
            AddKeys(WinFormsKeys.RShiftKey, Keys.RightShift);
            AddKeys(WinFormsKeys.LControlKey, Keys.LeftCtrl);
            AddKeys(WinFormsKeys.RControlKey, Keys.RightCtrl);
            AddKeys(WinFormsKeys.LMenu, Keys.LeftAlt);
            AddKeys(WinFormsKeys.RMenu, Keys.RightAlt);
            AddKeys(WinFormsKeys.BrowserBack, Keys.BrowserBack);
            AddKeys(WinFormsKeys.BrowserForward, Keys.BrowserForward);
            AddKeys(WinFormsKeys.BrowserRefresh, Keys.BrowserRefresh);
            AddKeys(WinFormsKeys.BrowserStop, Keys.BrowserStop);
            AddKeys(WinFormsKeys.BrowserSearch, Keys.BrowserSearch);
            AddKeys(WinFormsKeys.BrowserFavorites, Keys.BrowserFavorites);
            AddKeys(WinFormsKeys.BrowserHome, Keys.BrowserHome);
            AddKeys(WinFormsKeys.VolumeMute, Keys.VolumeMute);
            AddKeys(WinFormsKeys.VolumeDown, Keys.VolumeDown);
            AddKeys(WinFormsKeys.VolumeUp, Keys.VolumeUp);
            AddKeys(WinFormsKeys.MediaNextTrack, Keys.MediaNextTrack);
            AddKeys(WinFormsKeys.MediaPreviousTrack, Keys.MediaPreviousTrack);
            AddKeys(WinFormsKeys.MediaStop, Keys.MediaStop);
            AddKeys(WinFormsKeys.MediaPlayPause, Keys.MediaPlayPause);
            AddKeys(WinFormsKeys.LaunchMail, Keys.LaunchMail);
            AddKeys(WinFormsKeys.SelectMedia, Keys.SelectMedia);
            AddKeys(WinFormsKeys.LaunchApplication1, Keys.LaunchApplication1);
            AddKeys(WinFormsKeys.LaunchApplication2, Keys.LaunchApplication2);
            AddKeys(WinFormsKeys.Oem1, Keys.Oem1);
            AddKeys(WinFormsKeys.OemSemicolon, Keys.OemSemicolon);
            AddKeys(WinFormsKeys.Oemplus, Keys.OemPlus);
            AddKeys(WinFormsKeys.Oemcomma, Keys.OemComma);
            AddKeys(WinFormsKeys.OemMinus, Keys.OemMinus);
            AddKeys(WinFormsKeys.OemPeriod, Keys.OemPeriod);
            AddKeys(WinFormsKeys.Oem2, Keys.Oem2);
            AddKeys(WinFormsKeys.OemQuestion, Keys.OemQuestion);
            AddKeys(WinFormsKeys.Oem3, Keys.Oem3);
            AddKeys(WinFormsKeys.Oemtilde, Keys.OemTilde);
            AddKeys(WinFormsKeys.Oem4, Keys.Oem4);
            AddKeys(WinFormsKeys.OemOpenBrackets, Keys.OemOpenBrackets);
            AddKeys(WinFormsKeys.Oem5, Keys.Oem5);
            AddKeys(WinFormsKeys.OemPipe, Keys.OemPipe);
            AddKeys(WinFormsKeys.Oem6, Keys.Oem6);
            AddKeys(WinFormsKeys.OemCloseBrackets, Keys.OemCloseBrackets);
            AddKeys(WinFormsKeys.Oem7, Keys.Oem7);
            AddKeys(WinFormsKeys.OemQuotes, Keys.OemQuotes);
            AddKeys(WinFormsKeys.Oem8, Keys.Oem8);
            AddKeys(WinFormsKeys.Oem102, Keys.Oem102);
            AddKeys(WinFormsKeys.OemBackslash, Keys.OemBackslash);
            AddKeys(WinFormsKeys.Attn, Keys.Attn);
            AddKeys(WinFormsKeys.Crsel, Keys.CrSel);
            AddKeys(WinFormsKeys.Exsel, Keys.ExSel);
            AddKeys(WinFormsKeys.EraseEof, Keys.EraseEof);
            AddKeys(WinFormsKeys.Play, Keys.Play);
            AddKeys(WinFormsKeys.Zoom, Keys.Zoom);
            AddKeys(WinFormsKeys.NoName, Keys.NoName);
            AddKeys(WinFormsKeys.Pa1, Keys.Pa1);
            AddKeys(WinFormsKeys.OemClear, Keys.OemClear);
        }

        public InputManagerWindowsRuntime(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasPointer = true;

#if SILICONSTUDIO_PLATFORM_WINDOWS_STORE
            GamePadFactories.Add(new XInputGamePadFactory());
#endif
            HasMouse = new Windows.Devices.Input.MouseCapabilities().MousePresent > 0;
        }

        public override void Initialize()
        {
            base.Initialize();

            var windowHandle = Game.Window.NativeWindow;
            switch (windowHandle.Context)
            {
                case AppContextType.WindowsRuntime:
                    InitializeFromFrameworkElement((FrameworkElement)windowHandle.NativeWindow);
                    break;
                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
            
            // get sensor default instances
            windowsAccelerometer = WindowsAccelerometer.GetDefault();
            windowsCompass = WindowsCompass.GetDefault();
            windowsGyroscope = WindowsGyroscope.GetDefault();
            windowsOrientation = WindowsOrientation.GetDefault();

            // determine which sensors are available
            Accelerometer.IsSupported = windowsAccelerometer != null;
            Compass.IsSupported = windowsCompass != null;
            Gyroscope.IsSupported = windowsGyroscope != null;
            Orientation.IsSupported = windowsOrientation != null;
            Gravity.IsSupported = Orientation.IsSupported && Accelerometer.IsSupported;
            UserAcceleration.IsSupported = Gravity.IsSupported;

            if (mouseCapabilities.MousePresent > 0)
                MouseDevice.GetForCurrentView().MouseMoved += (_,y) => HandleRelativeOnMouseMoved(y);
        }

        private void HandleRelativeOnMouseMoved(MouseEventArgs args)
        {
            CurrentMouseDelta = NormalizeScreenPosition(new Vector2(args.MouseDelta.X, args.MouseDelta.Y));
        }

        internal override void CheckAndEnableSensors()
        {
            base.CheckAndEnableSensors();

            if (Accelerometer.ShouldBeEnabled || Gravity.ShouldBeEnabled || UserAcceleration.ShouldBeEnabled)
                windowsAccelerometer.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsAccelerometer.MinimumReportInterval);

            if (Compass.ShouldBeEnabled)
                windowsCompass.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsCompass.MinimumReportInterval);

            if (Gyroscope.ShouldBeEnabled)
                windowsGyroscope.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsGyroscope.MinimumReportInterval);

            if (Orientation.ShouldBeEnabled || Gravity.ShouldBeEnabled || UserAcceleration.ShouldBeEnabled)
                windowsOrientation.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsOrientation.MinimumReportInterval);
        }

        private static Vector3 GetAcceleration(WindowsAccelerometer accelerometer)
        {
            var currentReading = accelerometer.GetCurrentReading();
            if(currentReading == null)
                return Vector3.Zero;

            return G * new Vector3((float)currentReading.AccelerationX, (float)currentReading.AccelerationZ, -(float)currentReading.AccelerationY);
        }

        private static Quaternion GetOrientation(WindowsOrientation orientation)
        {
            var reading = orientation.GetCurrentReading();
            if (reading == null)
                return Quaternion.Identity;

            var q = reading.Quaternion;
            return new Quaternion(q.X, q.Z, -q.Y, q.W);
        }

        private static float GetNorth(WindowsCompass compass)
        {
            var currentReading = compass.GetCurrentReading();
            if (currentReading == null)
                return 0f;
            
            return MathUtil.DegreesToRadians((float)(currentReading.HeadingTrueNorth ?? currentReading.HeadingMagneticNorth));
        }

        internal override void UpdateEnabledSensorsData()
        {
            base.UpdateEnabledSensorsData();

            if (Accelerometer.IsEnabled)
                Accelerometer.Acceleration = GetAcceleration(windowsAccelerometer);

            if (Compass.IsEnabled)
                Compass.Heading = GetNorth(windowsCompass);

            if (Gyroscope.IsEnabled)
            {
                var reading = windowsGyroscope.GetCurrentReading();
                Gyroscope.RotationRate = reading != null? new Vector3((float)reading.AngularVelocityX, (float)reading.AngularVelocityZ, -(float)reading.AngularVelocityY): Vector3.Zero;
            }

            if (Orientation.IsEnabled || UserAcceleration.IsEnabled || Gravity.IsEnabled)
            {
                var quaternion = GetOrientation(windowsOrientation);

                if (Orientation.IsEnabled)
                {
                    Orientation.FromQuaternion(quaternion);
                }
                if (UserAcceleration.IsEnabled || Gravity.IsEnabled)
                {
                    // calculate the gravity direction
                    var acceleration = GetAcceleration(windowsAccelerometer);
                    var gravityDirection = Vector3.Transform(-Vector3.UnitY, Quaternion.Invert(quaternion));
                    var gravity = G * gravityDirection;
                    
                    if (Gravity.IsEnabled)
                        Gravity.Vector = gravity;

                    if (UserAcceleration.IsEnabled)
                        UserAcceleration.Acceleration = acceleration - gravity;
                }
            }
        }

        internal override void CheckAndDisableSensors()
        {
            base.CheckAndDisableSensors();

            if (!(Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled) && (Accelerometer.ShouldBeDisabled || Gravity.ShouldBeDisabled || UserAcceleration.ShouldBeDisabled))
                windowsAccelerometer.ReportInterval = 0;

            if (Compass.ShouldBeDisabled)
                windowsCompass.ReportInterval = 0;

            if (Gyroscope.ShouldBeDisabled)
                windowsGyroscope.ReportInterval = 0;

            if (!(Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled) && (Orientation.ShouldBeDisabled || Gravity.ShouldBeDisabled || UserAcceleration.ShouldBeDisabled))
                windowsOrientation.ReportInterval = 0;
        }

        public override void OnApplicationPaused(object sender, EventArgs e)
        {
            base.OnApplicationPaused(sender, e);

            // revert sensor sampling rate to reduce battery consumption

            if (Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsAccelerometer.ReportInterval = 0;

            if (Compass.IsEnabled)
                windowsCompass.ReportInterval = 0;

            if (Gyroscope.IsEnabled)
                windowsGyroscope.ReportInterval = 0;

            if (Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsOrientation.ReportInterval = 0;
        }

        public override void OnApplicationResumed(object sender, EventArgs e)
        {
            base.OnApplicationResumed(sender, e);

            // reset the xenko sampling rate to activated sensors

            if (Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsAccelerometer.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsAccelerometer.MinimumReportInterval);

            if (Compass.IsEnabled)
                windowsCompass.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsCompass.MinimumReportInterval);

            if (Gyroscope.IsEnabled)
                windowsGyroscope.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsGyroscope.MinimumReportInterval);

            if (Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsOrientation.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsOrientation.MinimumReportInterval);
        }

        private void InitializeFromFrameworkElement(FrameworkElement uiElement)
        {
            if (!(uiElement is Control))
            {
                uiElement.Loaded += uiElement_Loaded;
                uiElement.Unloaded += uiElement_Unloaded;

                uiElement_Loaded(uiElement, null); //todo verify, this fixes WIN10 issues but it's not so clear
                //uiElement_Loaded never triggers because uiElement is already loaded but there is no way to check that unless by event...
            }
            else
            {
                uiElement.KeyDown += (_, e) => HandleKeyFrameworkElement(e, InputEventType.Down);
                uiElement.KeyUp += (_, e) => HandleKeyFrameworkElement(e, InputEventType.Up);
            }

            ControlWidth = (float)uiElement.ActualWidth;
            ControlHeight = (float)uiElement.ActualHeight;

            uiElement.SizeChanged += (_, e) => HandleSizeChangedEvent(e.NewSize);
            uiElement.PointerPressed += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Down);
            uiElement.PointerReleased += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Up);
            uiElement.PointerWheelChanged += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Move);
            uiElement.PointerMoved += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Move);
            uiElement.PointerExited += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Out);
            uiElement.PointerCanceled += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Cancel);
            uiElement.PointerCaptureLost += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Cancel);
        }

        void uiElement_Loaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (DependencyObject)sender;

            while (uiElement != null)
            {
                var control = uiElement as Control;
                if (control != null && control.Focus(FocusState.Programmatic))
                {
                    // Get keyboard focus, and bind to this event
                    control.KeyDown += (_, e2) => HandleKeyFrameworkElement(e2, InputEventType.Down);
                    control.KeyUp += (_, e2) => HandleKeyFrameworkElement(e2, InputEventType.Up);
                    break;
                }

                uiElement = VisualTreeHelper.GetParent(uiElement);
            }
        }

        void uiElement_Unloaded(object sender, RoutedEventArgs e)
        {
            // TODO: Unregister event
        }
        
        private void InitializeFromCoreWindow(CoreWindow coreWindow)
        {
            ControlWidth = (float)coreWindow.Bounds.Width;
            ControlHeight = (float)coreWindow.Bounds.Height;

            coreWindow.SizeChanged += (_, args) => { HandleSizeChangedEvent(args.Size); args.Handled = true; };
            coreWindow.PointerPressed += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Down);
            coreWindow.PointerReleased += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Up);
            coreWindow.PointerWheelChanged += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Move);
            coreWindow.PointerMoved += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Move);
            coreWindow.PointerExited += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Out);
            coreWindow.PointerCaptureLost += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Cancel);

            coreWindow.KeyDown += (_, args) => HandleKeyCoreWindow(args, InputEventType.Down);
            coreWindow.KeyUp += (_, args) => HandleKeyCoreWindow(args, InputEventType.Up);
        }

        private void HandleSizeChangedEvent(Size size)
        {
            ControlWidth = (float)size.Width;
            ControlHeight = (float)size.Height;
        }

        private void HandleKeyFrameworkElement(KeyRoutedEventArgs args, InputEventType inputEventType)
        {
            if (HandleKey(args.Key, args.KeyStatus, inputEventType))
                args.Handled = true;
        }

        private void HandleKeyCoreWindow(KeyEventArgs args, InputEventType inputEventType)
        {
            if (HandleKey(args.VirtualKey, args.KeyStatus, inputEventType))
                args.Handled = true;
        }

        private bool HandleKey(VirtualKey virtualKey, CorePhysicalKeyStatus keyStatus, InputEventType type)
        {
            // If our EditText TextBox is active, let's ignore all key events
            if (Game.Context is GameContextWindowsRuntime && ((GameContextWindowsRuntime)Game.Context).EditTextBox.Parent != null)
            {
                return false;
            }

            // Remap certain keys
            switch (virtualKey)
            {
                case VirtualKey.Shift:
                    // Only way to differentiate left and right shift is through the scan code
                    virtualKey = keyStatus.ScanCode == 54 ? VirtualKey.RightShift : VirtualKey.LeftShift;
                    break;
                case VirtualKey.Control:
                    virtualKey = keyStatus.IsExtendedKey ? VirtualKey.RightControl : VirtualKey.LeftControl;
                    break;
                case VirtualKey.Menu:
                    virtualKey = keyStatus.IsExtendedKey ? VirtualKey.RightMenu : VirtualKey.LeftMenu;
                    break;
            }

            // Let Alt + F4 go through
            if (virtualKey == VirtualKey.F4 && IsKeyDownNow(Keys.LeftAlt))
                return false;

            Keys key;
            if (!mapKeys.TryGetValue(virtualKey, out key))
                key = Keys.None;

            lock (KeyboardInputEvents)
            {
                KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = type });
            }

            return true;
        }

        private bool IsKeyDownNow(Keys key)
        {
            // Check unprocessed up/down events that happened during this frame (in case key has just been pressed)
            lock (KeyboardInputEvents)
            {
                for (int index = KeyboardInputEvents.Count - 1; index >= 0; --index)
                {
                    var keyboardInputEvent = KeyboardInputEvents[index];
                    if (keyboardInputEvent.Key == key)
                    {
                        if (keyboardInputEvent.Type == InputEventType.Down)
                            return true;
                        if (keyboardInputEvent.Type == InputEventType.Up)
                            return false;
                    }
                }
            }

            // If nothing was done this frame, check if Alt was already considered down in previous frames
            if (IsKeyDown(key))
                return true;

            return false;
        }

        private void HandlePointerEventFrameworkElement(FrameworkElement uiElement, PointerRoutedEventArgs pointerRoutedEventArgs, PointerState pointerState)
        {
            HandlePointerEvent(pointerRoutedEventArgs.GetCurrentPoint(uiElement), pointerState);

            pointerRoutedEventArgs.Handled = true;
        }

        private void HandlePointerEventCoreWindow(PointerEventArgs args, PointerState pointerState)
        {
            HandlePointerEvent(args.CurrentPoint, pointerState);

            args.Handled = true;
        }

        void HandlePointerEvent(WinRTPointerPoint p, PointerState ptrState)
        {
            var pointerType = ConvertPointerDeviceType(p.PointerDevice.PointerDeviceType);
            var isMouse = pointerType == PointerType.Mouse;
            var position = NormalizeScreenPosition(PointToVector2(p.Position));

            if (isMouse)
            {
                if (ptrState == PointerState.Cancel || ptrState == PointerState.Out)
                {
                    // invalidate mouse and current pointers
                    LostFocus = true;

                    for (int i = 0; i < MouseButtonCurrentlyDown.Length; i++)
                    {
                        if (MouseButtonCurrentlyDown[i])
                        {
                            HandlePointerEvents(i, position, PointerState.Out, pointerType);
                            MouseButtonCurrentlyDown[i] = false;
                        }
                    }
                }
                else // down/up/move
                {
                    // Note: The problem here is that the PointerPressed event is not triggered twice when two button are pressed together.
                    // That is why we are forced to continuously keep the state of all buttons of the mouse.

                    MouseInputEvent mouseEvent;

                    // Trigger mouse button and pointer Down events for newly pressed buttons.
                    foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
                    {
                        var buttonId = (int)button;
                        if (!MouseButtonCurrentlyDown[buttonId] && MouseButtonIsPressed(p.Properties, button))
                        {
                            lock (MouseInputEvents)
                            {
                                mouseEvent = new MouseInputEvent { Type = InputEventType.Down, MouseButton = button };
                                MouseInputEvents.Add(mouseEvent);
                            }

                            HandlePointerEvents(buttonId, position, PointerState.Down, pointerType);

                            MouseButtonCurrentlyDown[buttonId] = true;
                        }
                    }

                    // Trigger Move events to pointer that have changed position
                    if (CurrentMousePosition != position)
                    {
                        foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
                        {
                            var buttonId = (int)button;
                            if (MouseButtonCurrentlyDown[buttonId])
                                HandlePointerEvents(buttonId, position, PointerState.Move, pointerType);
                        } 
                    }

                    // Trigger mouse button and pointer Up events for newly released buttons.
                    foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
                    {
                        var buttonId = (int)button;
                        if (MouseButtonCurrentlyDown[buttonId] && !MouseButtonIsPressed(p.Properties, button))
                        {
                            lock (MouseInputEvents)
                            {
                                mouseEvent = new MouseInputEvent { Type = InputEventType.Up, MouseButton = button };
                                MouseInputEvents.Add(mouseEvent);
                            }

                            HandlePointerEvents(buttonId, position, PointerState.Up, pointerType);

                            MouseButtonCurrentlyDown[buttonId] = false;
                        }
                    }

                    // Trigger mouse wheel events
                    if (Math.Abs(p.Properties.MouseWheelDelta) > MathUtil.ZeroTolerance)
                    {
                        lock (MouseInputEvents)
                        {
                            mouseEvent = new MouseInputEvent { Type = InputEventType.Wheel, MouseButton = MouseButton.Middle, Value = p.Properties.MouseWheelDelta };
                            MouseInputEvents.Add(mouseEvent);
                        }
                    }
                }

                // Update mouse cursor position
                CurrentMousePosition = position;
            }
            else
            {
                HandlePointerEvents((int)p.PointerId, position, ptrState, pointerType);
            }
        }

        private bool MouseButtonIsPressed(PointerPointProperties mouseProperties, MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return mouseProperties.IsLeftButtonPressed;
                case MouseButton.Middle:
                    return mouseProperties.IsMiddleButtonPressed;
                case MouseButton.Right:
                    return mouseProperties.IsRightButtonPressed;
                case MouseButton.Extended1:
                    return mouseProperties.IsXButton1Pressed;
                case MouseButton.Extended2:
                    return mouseProperties.IsXButton2Pressed;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
        }

        private PointerType ConvertPointerDeviceType(WinRTPointerDeviceType deviceType)
        {
            switch (deviceType)
            {
                case WinRTPointerDeviceType.Mouse:
                    return PointerType.Mouse;
                case WinRTPointerDeviceType.Pen:
                    throw new NotSupportedException("Pen device input is not supported.");
                case WinRTPointerDeviceType.Touch:
                    return PointerType.Touch;
            }
            return PointerType.Unknown;
        }

        private Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        private static void AddKeys(WinFormsKeys fromKey, Keys toKey)
        {
            if (!mapKeys.ContainsKey((VirtualKey)fromKey))
            {
                mapKeys.Add((VirtualKey)fromKey, toKey);
            }
        }
    }
}
#endif
