// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Manages collecting input from connected input device in the form of <see cref="IInputDevice"/> objects. Also provides some convenience functions for most commonly used devices
    /// </summary>
    public class InputManager : GameSystemBase
    {
        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;
        
        public static Logger Logger = GlobalLogger.GetLogger("Input");

        //this is used in some mobile platform for accelerometer stuff
        internal const float G = 9.81f;
        internal const float DesiredSensorUpdateRate = 60;
        internal const float GamePadAxisDeadZone = 0.05f;

        private readonly TypeBasedRegistry<IInputSource> inputSourceRegistry = new TypeBasedRegistry<IInputSource>();
        private readonly List<IInputSource> inputSources = new List<IInputSource>();
        private readonly Dictionary<IInputDevice, IInputSource> inputDevices = new Dictionary<IInputDevice, IInputSource>();

        // Mapping of device guid to device
        private readonly Dictionary<Guid, IInputDevice> inputDevicesById = new Dictionary<Guid, IInputDevice>();

        // List mapping GamePad index to the guid of the device
        private readonly List<Guid> gamepadIds = new List<Guid>();

        private readonly HashSet<Keys> downKeysSet = new HashSet<Keys>();
        private readonly HashSet<Keys> pressedKeysSet = new HashSet<Keys>();
        private readonly HashSet<Keys> releasedKeysSet = new HashSet<Keys>();

        private readonly HashSet<MouseButton> downButtonSet = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> pressedButtonsSet = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> releasedButtonsSet = new HashSet<MouseButton>();
        
        public readonly List<PointerEvent> PointerEvents = new List<PointerEvent>();
        public readonly List<KeyEvent> KeyEvents = new List<KeyEvent>();

        private readonly List<IKeyboardDevice> keyboardDevices = new List<IKeyboardDevice>();
        private readonly List<IPointerDevice> pointerDevices = new List<IPointerDevice>();
        private readonly List<IGamePadDevice> gamePadDevices = new List<IGamePadDevice>();

        private readonly List<SensorBase> sensors = new List<SensorBase>();
        private readonly List<GestureEvent> currentGestureEvents = new List<GestureEvent>();
        private readonly Dictionary<GestureConfig, GestureRecognizer> gestureConfigToRecognizer = new Dictionary<GestureConfig, GestureRecognizer>();

        // Backing field of MousePosition
        private Vector2 mousePosition;

        /// <summary>
        /// List of the gestures to recognize.
        /// </summary>
        /// <remarks>To detect a new gesture add its configuration to the list. 
        /// To stop detecting a gesture remove its configuration from the list. 
        /// To all gestures detection clear the list.
        /// Note that once added to the list the <see cref="GestureConfig"/>s are frozen by the system and cannot be modified anymore.</remarks>
        /// <seealso cref="GestureConfig"/>
        public GestureConfigCollection ActivatedGestures { get; private set; }
        
        /// <summary>
        /// Gets the delta value of the mouse wheel button since last frame.
        /// </summary>
        public float MouseWheelDelta { get; private set; }

        /// <summary>
        /// Gets the reference to the accelerometer sensor. The accelerometer measures all the acceleration forces applied on the device.
        /// </summary>
        public AccelerometerSensor Accelerometer { get; private set; }

        /// <summary>
        /// Gets the reference to the compass sensor. The compass measures the angle between the device top and the north.
        /// </summary>
        public CompassSensor Compass { get; private set; }

        /// <summary>
        /// Gets the reference to the gyroscope sensor. The gyroscope measures the rotation speed of the device.
        /// </summary>
        public GyroscopeSensor Gyroscope { get; private set; }

        /// <summary>
        /// Gets the reference to the user acceleration sensor. The user acceleration sensor measures the acceleration produce by the user on the device (no gravity).
        /// </summary>
        public UserAccelerationSensor UserAcceleration { get; private set; }

        /// <summary>
        /// Gets the reference to the gravity sensor. The gravity sensor measures the gravity vector applied to the device.
        /// </summary>
        public GravitySensor Gravity { get; private set; }

        /// <summary>
        /// Gets the reference to the orientation sensor. The orientation sensor measures orientation of device in the world.
        /// </summary>
        public OrientationSensor Orientation { get; private set; }

        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked => HasMouse && Mouse.IsMousePositionLocked;

        /// <summary>
        /// Gets or sets the configuration for virtual buttons.
        /// </summary>
        /// <value>The current binding.</value>
        public VirtualButtonConfigSet VirtualButtonConfigSet { get; set; }

        /// <summary>
        /// Gets the collection of gesture events since the previous updates.
        /// </summary>
        /// <value>The gesture events.</value>
        public List<GestureEvent> GestureEvents { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether pointer device is available.
        /// </summary>
        /// <value><c>true</c> if pointer devices are available; otherwise, <c>false</c>.</value>
        public bool HasPointer => pointerDevices.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the mouse is available.
        /// </summary>
        /// <value><c>true</c> if the mouse is available; otherwise, <c>false</c>.</value>
        public bool HasMouse => pointerDevices.Any(x => x.Type == PointerType.Mouse);

        /// <summary>
        /// Gets a value indicating whether the keyboard is available.
        /// </summary>
        /// <value><c>true</c> if the keyboard is available; otherwise, <c>false</c>.</value>
        public bool HasKeyboard => keyboardDevices.Count > 0;

        /// <summary>
        /// Gets a value indicating whether gamepads are available.
        /// </summary>
        /// <value><c>true</c> if gamepads are available; otherwise, <c>false</c>.</value>
        public bool HasGamePad => gamePadDevices.Count > 0;

        /// <summary>
        /// Gets the number of gamepad connected.
        /// </summary>
        /// <value>The number of gamepad connected.</value>
        public int GamePadCount => gamePadDevices.Count;

        /// <summary>
        /// Gets the first pointer device, or null if there is none
        /// </summary>
        public IPointerDevice Pointer => pointerDevices.Count > 0 ? pointerDevices[0] : null;

        /// <summary>
        /// Gets the first mouse pointer device, or null if there is none
        /// </summary>
        public IMouseDevice Mouse => pointerDevices.FirstOrDefault(x => x.Type == PointerType.Mouse) as IMouseDevice;

        /// <summary>
        /// Gets the first keyboard device, or null if there is none
        /// </summary>
        public IKeyboardDevice Keyboard => keyboardDevices.Count > 0 ? keyboardDevices[0] : null;

        /// <summary>
        /// Gets the collection of connected gamepads, in no particular order
        /// </summary>
        public IReadOnlyCollection<IGamePadDevice> GamePads => gamePadDevices;
        
        /// <summary>
        /// Gets the list of keys being pressed down.
        /// </summary>
        /// <value>The key pressed.</value>
        public List<Keys> KeyDown => downKeysSet.ToList();

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MousePosition
        {
            get { return mousePosition; }
            set { SetMousePosition(value); }
        }

        /// <summary>
        /// Gets the mouse delta.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MouseDelta { get; private set; }
        
        internal InputManager(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;

            GestureEvents = currentGestureEvents;

            ActivatedGestures = new GestureConfigCollection();
            //ActivatedGestures.CollectionChanged += ActivatedGesturesChanged;

            Services.AddService(typeof(InputManager), this);

            Accelerometer = new AccelerometerSensor();
            Compass = new CompassSensor();
            Gyroscope = new GyroscopeSensor();
            UserAcceleration = new UserAccelerationSensor();
            Gravity = new GravitySensor();
            Orientation = new OrientationSensor();

            sensors.Add(Accelerometer);
            sensors.Add(Compass);
            sensors.Add(Gyroscope);
            sensors.Add(UserAcceleration);
            sensors.Add(Gravity);
            sensors.Add(Orientation);

            //supportedGamePadButtons.Add(GamePadButton.A);
            //supportedGamePadButtons.Add(GamePadButton.B);
            //supportedGamePadButtons.Add(GamePadButton.X);
            //supportedGamePadButtons.Add(GamePadButton.Y);
            //supportedGamePadButtons.Add(GamePadButton.Start);
            //supportedGamePadButtons.Add(GamePadButton.Back);
            //supportedGamePadButtons.Add(GamePadButton.LeftShoulder);
            //supportedGamePadButtons.Add(GamePadButton.RightShoulder);
            //supportedGamePadButtons.Add(GamePadButton.RightThumb);
            //supportedGamePadButtons.Add(GamePadButton.LeftThumb);
            //supportedGamePadButtons.Add(GamePadButton.PadUp);
            //supportedGamePadButtons.Add(GamePadButton.PadDown);
            //supportedGamePadButtons.Add(GamePadButton.PadLeft);
            //supportedGamePadButtons.Add(GamePadButton.PadRight);
            //
            //for (var i = 0; i < MaximumGamePadCount; i++)
            //{
            //    pressedGamePadButtonsSet[i] = new HashSet<GamePadButton>();
            //    releasedGamePadButtonsSet[i] = new HashSet<GamePadButton>();
            //    currentGamePadButtonsSet[i] = new HashSet<GamePadButton>();
            //    activeGamePadButtonsSet[i] = new HashSet<GamePadButton>();
            //}
        }

        public override void Initialize()
        {
            base.Initialize();

            Game.Activated += OnApplicationResumed;
            Game.Deactivated += OnApplicationPaused;

            // Find all classes that inherit from IInputSource and are enabled for the current game context
            foreach(var inputSource in inputSourceRegistry.CreateAllInstances())
            {
                if (inputSource.IsEnabled(Game.Context))
                {
                    inputSources.Add(inputSource);
                }
            }

            // Initialize sources
            foreach(var source in inputSources)
            {
                source.OnInputDeviceAdded += OnInputDeviceAdded;
                source.OnInputDeviceRemoved += OnInputDeviceRemoved;
                source.Initialize(this);
            }
        }

        protected override void Destroy()
        {
            base.Destroy();

            // Destroy all input sources
            foreach (var source in inputSources)
            {
                source.Dispose();
            }

            Game.Activated -= OnApplicationResumed;
            Game.Deactivated -= OnApplicationPaused;

            // ensure that OnApplicationPaused is called before destruction, when Game.Deactivated event is not triggered.
            OnApplicationPaused(this, EventArgs.Empty);
        }

        private void OnInputDeviceAdded(object sender, IInputDevice device)
        {
            inputDevices.Add(device, (IInputSource)sender);
            inputDevicesById.Add(device.Id, device);
            
            if (device is IKeyboardDevice)
            {
                RegisterKeyboard(device as IKeyboardDevice);
            }
            else if (device is IPointerDevice)
            {
                RegisterPointer(device as IPointerDevice);
            }
            else if (device is IGamePadDevice)
            {
                RegisterGamePad(device as IGamePadDevice);
            }
        }

        private void OnInputDeviceRemoved(object sender, IInputDevice device)
        {
            inputDevices.Remove(device);
            inputDevicesById.Remove(device.Id);

            if (device is IKeyboardDevice)
            {
                UnregisterKeyboard(device as IKeyboardDevice);
            }
            else if (device is IPointerDevice)
            {
                UnregisterPointer(device as IPointerDevice);
            }
            else if (device is IGamePadDevice)
            {
                UnregisterGamePad(device as IGamePadDevice);
            }
        }

        void RegisterPointer(IPointerDevice pointer)
        {
            pointerDevices.Add(pointer);

            // Handle pointer events
            pointer.OnPointer += (sender, evt) =>
            {
                var dev = sender as IPointerDevice;
                Vector2 invSurfaceSize = 1.0f / dev.SurfaceSize;

                // Normalize pointer positions
                evt.Position *= invSurfaceSize;
                evt.DeltaPosition *= invSurfaceSize;

                PointerEvents.Add(evt);
            };

            var mouse = pointer as IMouseDevice;
            if (mouse != null)
            {
                // Handle button events for all mice
                mouse.OnMouseButton += (sender, evt) =>
                {
                    if (evt.State == MouseButtonState.Pressed)
                    {
                        downButtonSet.Add(evt.Button);
                        pressedButtonsSet.Add(evt.Button);
                    }
                    else
                    {
                        downButtonSet.Remove(evt.Button);
                        releasedButtonsSet.Add(evt.Button);
                    }
                };

                mouse.OnMouseWheel += (sender, evt) =>
                {
                    if(Math.Abs(evt.WheelDelta) > Math.Abs(MouseWheelDelta))
                        MouseWheelDelta = evt.WheelDelta;
                };
            }
        }

        void UnregisterPointer(IPointerDevice pointer)
        {
            pointerDevices.Remove(pointer);
            pointer.OnPointer = null;
            var mouse = pointer as IMouseDevice;
            if (mouse != null)
            {
                mouse.OnMouseButton = null;
                mouse.OnMouseWheel = null;
            }
        }

        void RegisterKeyboard(IKeyboardDevice keyboard)
        {
            keyboardDevices.Add(keyboard);

            // Handle key events for all keyboards
            keyboard.OnKey += (sender, evt) =>
            {
                if (evt.Type == KeyEventType.Pressed)
                {
                    downKeysSet.Add(evt.Key);
                    pressedKeysSet.Add(evt.Key);
                }
                else
                {
                    downKeysSet.Remove(evt.Key);
                    releasedKeysSet.Add(evt.Key);
                }
                KeyEvents.Add(evt);
            };
        }

        void UnregisterKeyboard(IKeyboardDevice keyboard)
        {
            keyboardDevices.Remove(keyboard);
            keyboard.OnKey = null;
        }
        
        void RegisterGamePad(IGamePadDevice gamePad)
        {
            gamePadDevices.Add(gamePad);
            
            // Find a new index for this gamepad
            int targetIndex = 0;
            for (int i = 0; i < gamepadIds.Count; i++)
            {
                if (gamepadIds[i] == Guid.Empty)
                {
                    targetIndex = i;
                    break;
                }
                targetIndex++;
            }

            if (targetIndex >= gamepadIds.Count)
            {
                gamepadIds.Add(gamePad.Id);
            }
            else
            {
                gamepadIds[targetIndex] = gamePad.Id;
            }

            var gamepadBase = gamePad as GamePadDeviceBase;
            if(gamepadBase == null)
                throw new InvalidOperationException("Cannot register GamePad id, because it does not inherit from GamePadDeviceBase");
            gamepadBase.IndexInternal = targetIndex;
        }

        void UnregisterGamePad(IGamePadDevice gamePad)
        {
            // Free the gamepad index in the gamepad list
            // this will allow another gamepad to use this index again
            if(gamepadIds.Count <= gamePad.Index || gamePad.Index < 0)
                throw new IndexOutOfRangeException("Gamepad index was out of range");
            gamepadIds[gamePad.Index] = Guid.Empty;

            gamePadDevices.Remove(gamePad);
            gamePad.OnButton = null;
            gamePad.OnAxisChanged = null;
            gamePad.OnPovControllerChanged = null;
        }

        /// <summary>
        /// Lock the mouse's position and hides it until the next call to <see cref="UnlockMousePosition"/>.
        /// </summary>
        /// <param name="forceCenter">If true will make sure that the mouse cursor position moves to the center of the client window</param>
        /// <remarks>This function has no effects on devices that does not have mouse</remarks>
        public void LockMousePosition(bool forceCenter = false)
        {
            // Lock primary mouse
            if (HasMouse)
            {
                Mouse.LockMousePosition(forceCenter);
            }
        }

        /// <summary>
        /// Unlock the mouse's position previously locked by calling <see cref="LockMousePosition"/> and restore the mouse visibility.
        /// </summary>
        /// <remarks>This function has no effects on devices that does not have mouse</remarks>
        public void UnlockMousePosition()
        {
            if (HasMouse)
            {
                Mouse.UnlockMousePosition();
            }
        }
        
        /*
        private void ActivatedGesturesChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            switch (trackingCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    StartGestureRecognition((GestureConfig)trackingCollectionChangedEventArgs.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    StopGestureRecognition((GestureConfig)trackingCollectionChangedEventArgs.Item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("ActivatedGestures collection was modified but the action was not supported by the system.");
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartGestureRecognition(GestureConfig config)
        {
            gestureConfigToRecognizer.Add(config, config.CreateRecognizer(ScreenAspectRatio));
        }

        private void StopGestureRecognition(GestureConfig config)
        {
            gestureConfigToRecognizer.Remove(config);
        }
        */

        protected virtual void SetMousePosition(Vector2 normalizedPosition)
        {
            // Set mouse position for first pointer device
            if(HasMouse)
            {
                Mouse.SetMousePosition(normalizedPosition * pointerDevices[0].SurfaceSize);
            }
        }
        
        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return downKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            return pressedKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public bool IsKeyReleased(Keys key)
        {
            return releasedKeysSet.Contains(key);
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        public bool HasDownMouseButtons()
        {
            return downButtonSet.Count > 0;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons()
        {
            return releasedButtonsSet.Count > 0;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons()
        {
            return pressedButtonsSet.Count > 0;
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return downButtonSet.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return pressedButtonsSet.Contains(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return releasedButtonsSet.Contains(mouseButton);
        }

        /// <summary>
        /// Gets the gamepad with a specific index.
        /// </summary>
        /// <param name="gamePadIndex">The index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The gamepad, or null if no gamepad was found with the given index.</returns>
        public IGamePadDevice GetGamePad(int gamePadIndex)
        {
            if (gamePadDevices.Count == 0)
                return null; // No gamepads connected

            Guid padId;

            if (gamePadIndex < 0)
                padId = gamepadIds.First(x => x != Guid.Empty); // Return the first gamepad
            else if (gamePadIndex >= gamepadIds.Count)
                return null;
            else
                padId = gamepadIds[gamePadIndex];

            return inputDevicesById[padId] as IGamePadDevice;
        }

        /// <summary>
        /// Gets the state of a gamepad with a given index
        /// </summary>
        /// <param name="gamePadIndex">The index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The state of the gamepad</returns>
        public GamePadState GetGamePadState(int gamePadIndex)
        {
            GamePadState state = new GamePadState();
            GetGamePad(gamePadIndex)?.GetGamePadState(ref state);
            return state;
        }

        /// <summary>
        /// Sets the vibration state of the gamepad
        /// </summary>
        /// <param name="gamepadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="leftMotor">A value from 0.0 to 1.0 where 0.0 is no vibration and 1.0 is full vibration power; applies to the left motor.</param>
        /// <param name="rightMotor">A value from 0.0 to 1.0 where 0.0 is no vibration and 1.0 is full vibration power; applies to the right motor.</param>
        public void SetGamePadVibration(int gamePadIndex, float leftMotor, float rightMotor)
        {
        }

        /// <summary>
        /// Determines whether the specified game pad button is being pressed down.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonDown(int gamePadIndex, GamePadButton button)
        {
            return (GetGamePadState(gamePadIndex).Buttons & button) != 0;
        }

        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonPressed(int gamePadIndex, GamePadButton button)
        {
            return false;
        }

        /// <summary>
        /// Determines whether the specified game pad button is released since the previous update.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonReleased(int gamePadIndex, GamePadButton button)
        {
            return false;
        }

        /// <summary>
        /// Rescans all input devices in order to query new device connected. See remarks.
        /// </summary>
        /// <remarks>
        /// This method could take several milliseconds and should be used at specific time in a game where performance is not crucial (pause, configuration screen...etc.)
        /// </remarks>
        public virtual void Scan()
        {
            foreach (var source in inputSources)
            {
                source.Scan();
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Reset convenience states
            pressedKeysSet.Clear();
            releasedKeysSet.Clear();
            pressedButtonsSet.Clear();
            releasedButtonsSet.Clear();
            PointerEvents.Clear();
            KeyEvents.Clear();
            MouseWheelDelta = 0;

            // Update all input sources so they can route events to input devices and possible register new devices
            foreach (var source in inputSources)
            {
                source.Update();
            }

            // Update all input sources so they can send events and update their state
            foreach (var pair in inputDevices)
            {
                pair.Key.Update();
            }

            // Update mouse position from first pointer
            if (HasPointer)
            {
                Vector2 invSurfaceSize = 1.0f/pointerDevices[0].SurfaceSize;
                mousePosition =  pointerDevices[0].AbsolutePosition * invSurfaceSize;
                MouseDelta = pointerDevices[0].Delta * invSurfaceSize;
            }
        }

        /// <summary>
        /// Used internally to simulate pointer events
        /// </summary>
        /// <param name="e"></param>
        internal void InjectPointerEvent(PointerEvent e)
        {
            lock (PointerEvents)
            {
                PointerEvents.Add(e);
            }
        }

        /*
        private void UpdateSensors()
        {
            CheckAndEnableSensors();
            UpdateEnabledSensorsData();
            CheckAndDisableSensors();

            // reset ShouldEnable/Disable and Data values sensor
            foreach (var sensor in sensors)
            {
                // reset the data of disabled sensor (we don't want the sensor data value to be frozen to last enabled value)
                if (!sensor.IsEnabled)
                    sensor.ResetData();

                // reset the ShouldBeDisable/Enable internal state
                sensor.ShouldBeDisabled = false;
                sensor.ShouldBeEnabled = false;
            }
        }



        private void UpdateGestureEvents(TimeSpan elapsedGameTime)
        {
            currentGestureEvents.Clear();

            foreach (var gestureRecognizer in gestureConfigToRecognizer.Values)
                currentGestureEvents.AddRange(gestureRecognizer.ProcessPointerEvents(elapsedGameTime, currentPointerEvents));
        } */

        /// <summary>
        /// Gets or sets the value indicating if simultaneous multiple finger touches are enabled or not.
        /// If not enabled only the events of one finger at a time are triggered.
        /// </summary>
        public bool MultiTouchEnabled { get; set; } = false;

        public virtual void OnApplicationPaused(object sender, EventArgs e)
        {
            // TODO: Disable input updates, or is this disabled automatically?
        }
        public virtual void OnApplicationResumed(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Helper method to transform mouse and pointer event positions to sub rectangles
        /// </summary>
        /// <param name="fromSize">the size of the source rectangle</param>
        /// <param name="destinationRectangle">The destination viewport rectangle</param>
        /// <param name="screenCoordinates">The normalized screen coordinates</param>
        /// <returns></returns>
        public static Vector2 TransformPosition(Size2F fromSize, RectangleF destinationRectangle, Vector2 screenCoordinates)
        {
            return new Vector2((screenCoordinates.X*fromSize.Width - destinationRectangle.X)/destinationRectangle.Width,
                (screenCoordinates.Y*fromSize.Height - destinationRectangle.Y)/destinationRectangle.Height);
        }
    }
}
