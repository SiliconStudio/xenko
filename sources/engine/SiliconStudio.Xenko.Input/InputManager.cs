// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Manages collecting input from connected input device in the form of <see cref="IInputDevice"/> objects. Also provides some convenience functions for most commonly used devices
    /// </summary>
    public class InputManager : GameSystemBase
    {
        //this is used in some mobile platform for accelerometer stuff
        internal const float G = 9.81f;
        internal const float DesiredSensorUpdateRate = 60;

        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;

        /// <summary>
        /// The deadzone amount applied to all gamepad axes
        /// </summary>
        public static float GamePadAxisDeadZone = 0.05f;

        internal static Logger Logger = GlobalLogger.GetLogger("Input");

        // Keeps track of Position/Delta and Up/Down/Released states for multiple devices
        internal GlobalInputState GlobalInputState = new GlobalInputState();

        private readonly List<IInputSource> inputSources = new List<IInputSource>();
        private readonly Dictionary<IInputDevice, IInputSource> inputDevices = new Dictionary<IInputDevice, IInputSource>();

        // Mapping of device guid to device
        private readonly Dictionary<Guid, IInputDevice> inputDevicesById = new Dictionary<Guid, IInputDevice>();

        // List mapping GamePad index to the guid of the device
        private readonly List<Guid> gamepadIds = new List<Guid>();

        private readonly List<GestureEvent> gestureEvents = new List<GestureEvent>();
        private readonly List<InputEvent> inputEvents = new List<InputEvent>();

        private readonly List<IKeyboardDevice> keyboardDevices = new List<IKeyboardDevice>();
        private readonly List<IPointerDevice> pointerDevices = new List<IPointerDevice>();
        private readonly List<IGamePadDevice> gamePadDevices = new List<IGamePadDevice>();
        private readonly List<ISensorDevice> sensorDevices = new List<ISensorDevice>();

        private readonly Dictionary<GestureConfig, GestureRecognizer> gestureConfigToRecognizer = new Dictionary<GestureConfig, GestureRecognizer>();
        private Dictionary<int, GamePadState> lastGamePadStates = new Dictionary<int, GamePadState>();
        private Dictionary<int, GamePadState> currentGamePadStates = new Dictionary<int, GamePadState>();

        private readonly Dictionary<Type, IInputEventRouter> eventRouters = new Dictionary<Type, IInputEventRouter>();

        internal InputManager(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;

            ActivatedGestures = new GestureConfigCollection();
            ActivatedGestures.CollectionChanged += ActivatedGesturesChanged;

            Services.AddService(typeof(InputManager), this);

            ActionMapping = new InputActionMapping(this);
        }

        /// <summary>
        /// Virtual button mapping, maps gestures to input actions
        /// </summary>
        public InputActionMapping ActionMapping { get; }

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
        /// Gets the reference to the accelerometer sensor. The accelerometer measures all the acceleration forces applied on the device.
        /// </summary>
        public IAccelerometerSensor Accelerometer { get; private set; }

        /// <summary>
        /// Gets the reference to the compass sensor. The compass measures the angle between the device top and the north.
        /// </summary>
        public ICompassSensor Compass { get; private set; }

        /// <summary>
        /// Gets the reference to the gyroscope sensor. The gyroscope measures the rotation speed of the device.
        /// </summary>
        public IGyroscopeSensor Gyroscope { get; private set; }

        /// <summary>
        /// Gets the reference to the user acceleration sensor. The user acceleration sensor measures the acceleration produce by the user on the device (no gravity).
        /// </summary>
        public IUserAccelerationSensor UserAcceleration { get; private set; }

        /// <summary>
        /// Gets the reference to the gravity sensor. The gravity sensor measures the gravity vector applied to the device.
        /// </summary>
        public IGravitySensor Gravity { get; private set; }

        /// <summary>
        /// Gets the reference to the orientation sensor. The orientation sensor measures orientation of device in the world.
        /// </summary>
        public IOrientationSensor Orientation { get; private set; }

        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked => HasMouse && Mouse.IsMousePositionLocked;

        /// <summary>
        /// Gets the collection of gesture events since the previous updates.
        /// </summary>
        /// <value>The gesture events.</value>
        public IReadOnlyList<GestureEvent> GestureEvents => gestureEvents;

        /// <summary>
        /// Pointer events that happened since the last frame
        /// </summary>
        public IReadOnlyList<PointerEvent> PointerEvents => GlobalInputState.PointerEvents;

        /// <summary>
        /// Keyboard events that happened since the last frame
        /// </summary>
        public IReadOnlyList<KeyEvent> KeyEvents => GlobalInputState.KeyEvents;

        /// <summary>
        /// All input events that happened since the last frame
        /// </summary>
        public IReadOnlyList<InputEvent> InputEvents => inputEvents;

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
        /// Gets the collection of connected pointing devices (mouses, touchpads, etc)
        /// </summary>
        public IReadOnlyCollection<IPointerDevice> Pointers => pointerDevices;

        /// <summary>
        /// Gets the collection of connected keyboard inputs
        /// </summary>
        public IReadOnlyCollection<IKeyboardDevice> Keyboards => keyboardDevices;

        /// <summary>
        /// Gets the collection of connected sensor devices
        /// </summary>
        public IReadOnlyCollection<ISensorDevice> Sensors => sensorDevices;

        /// <summary>
        /// Gets a list of keys being pressed down.
        /// </summary>
        /// <value>A list of keys that are pressed</value>
        public List<Keys> KeyDown => GlobalInputState.DownKeysSet.ToList();

        /// <summary>
        /// Gets or sets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MousePosition
        {
            get { return GlobalInputState.MousePosition; }
            set { SetMousePosition(value); }
        }

        /// <summary>
        /// Gets the mouse delta.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MouseDelta => GlobalInputState.MouseDelta;

        /// <summary>
        /// Gets the delta value of the mouse wheel button since last frame.
        /// </summary>
        public float MouseWheelDelta => GlobalInputState.MouseWheelDelta;

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

        public override void Initialize()
        {
            base.Initialize();

            Game.Activated += OnApplicationResumed;
            Game.Deactivated += OnApplicationPaused;

            // Create input sources
            switch (Game.Context.ContextType)
            {
#if SILICONSTUDIO_XENKO_UI_SDL
                case AppContextType.DesktopSDL:
                    AddInputSource(new InputSourceSDL());
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_ANDROID
                case AppContextType.Android:
                    AddInputSource(new InputSourceAndroid());
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_IOS
                case AppContextType.iOS:
                    AddInputSource(new InputSourceiOS());
                    break;
#endif
#if SILICONSTUDIO_UI_OPENTK
                case AppContextType.DesktopOpenTK:
                    AddInputSource(new InputSourceOpenTK());
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_UWP
                case  AppContextType.UWP:
                    AddInputSource(new InputSourceUWP());
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_WINDOWS && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
                case AppContextType.Desktop:
                    AddInputSource(new InputSourceWinforms());
                    AddInputSource(new InputSourceWindowsDirectInput());
                    AddInputSource(new InputSourceWindowsXInput());
                    if (UseRawInput) AddInputSource(new InputSourceWindowsRawInput());
                    break;
#endif
                default:
                    throw new InvalidOperationException("Unsupported InputManager-GameContext combination");
            }

            // Simulated input, if enabled
            if (InputSourceSimulated.Enabled)
                AddInputSource(new InputSourceSimulated());

            // Register event types
            RegisterEventType<KeyEvent>();
            RegisterEventType<TextInputEvent>();
            RegisterEventType<MouseButtonEvent>();
            RegisterEventType<MouseWheelEvent>();
            RegisterEventType<PointerEvent>();
            RegisterEventType<GamePadButtonEvent>();
            RegisterEventType<GamePadAxisEvent>();
            RegisterEventType<GamePadPovControllerEvent>();

            // Add global input state to listen for input events
            AddListener(GlobalInputState);
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

#region Convenience Functions

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            return GlobalInputState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines whether the specified key is pressed since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
        public bool IsKeyPressed(Keys key)
        {
            return GlobalInputState.IsKeyPressed(key);
        }

        /// <summary>
        /// Determines whether the specified key is released since the previous update.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
        public bool IsKeyReleased(Keys key)
        {
            return GlobalInputState.IsKeyReleased(key);
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are down
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are down; otherwise, <c>false</c>.</returns>
        public bool HasDownMouseButtons()
        {
            return GlobalInputState.HasDownMouseButtons();
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons()
        {
            return GlobalInputState.HasReleasedMouseButtons();
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons()
        {
            return GlobalInputState.HasPressedMouseButtons();
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return GlobalInputState.IsMouseButtonDown(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return GlobalInputState.IsMouseButtonPressed(mouseButton);
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return GlobalInputState.IsMouseButtonReleased(mouseButton);
        }

        /// <summary>
        /// Gets the state of a gamepad with a given index
        /// </summary>
        /// <param name="gamePadIndex">The index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The state of the gamepad</returns>
        public GamePadState GetGamePadState(int gamePadIndex)
        {
            if (gamePadIndex == -1 && gamePadDevices.Count > 0)
                gamePadIndex = gamePadDevices[0].Index;

            GamePadState state;
            currentGamePadStates.TryGetValue(gamePadIndex, out state);
            return state;
        }

        /// <summary>
        /// Gets the previous state of a gamepad with a given index
        /// </summary>
        /// <param name="gamePadIndex">The index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The state of the gamepad</returns>
        public GamePadState GetLastGamePadState(int gamePadIndex)
        {
            if (gamePadIndex == -1 && gamePadDevices.Count > 0)
                gamePadIndex = gamePadDevices[0].Index;

            GamePadState state;
            lastGamePadStates.TryGetValue(gamePadIndex, out state);
            return state;
        }

        /// <summary>
        /// Determines whether the specified game pad button is being pressed down.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonDown(int gamePadIndex, GamePadButton button)
        {
            return (GetGamePadState(gamePadIndex).Buttons & button) != 0;
        }

        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonPressed(int gamePadIndex, GamePadButton button)
        {
            return (GetGamePadState(gamePadIndex).Buttons & button) != 0 && (GetLastGamePadState(gamePadIndex).Buttons & button) == 0;
        }

        /// <summary>
        /// Determines whether the specified game pad button is released since the previous update.
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonReleased(int gamePadIndex, GamePadButton button)
        {
            return (GetGamePadState(gamePadIndex).Buttons & button) == 0 && (GetLastGamePadState(gamePadIndex).Buttons & button) != 0;
        }

#endregion

        /// <summary>
        /// Sets the vibration state of the gamepad
        /// </summary>
        /// <param name="gamePadIndex">Index of the gamepad. -1 to use the first connected gamepad</param>
        /// <param name="leftMotor">A value from 0.0 to 1.0 where 0.0 is no vibration and 1.0 is full vibration power; applies to the left motor.</param>
        /// <param name="rightMotor">A value from 0.0 to 1.0 where 0.0 is no vibration and 1.0 is full vibration power; applies to the right motor.</param>
        public void SetGamePadVibration(int gamePadIndex, float leftMotor, float rightMotor)
        {
            var pad = GetGamePad(gamePadIndex);
            if (pad == null)
                return;
            var padVibration = pad as IGamePadVibration;
            padVibration?.SetVibration(leftMotor, rightMotor);
        }

        /// <summary>
        /// Rescans all input devices in order to query new device connected. See remarks.
        /// </summary>
        /// <remarks>
        /// This method could take several milliseconds and should be used at specific time in a game where performance is not crucial (pause, configuration screen...etc.)
        /// </remarks>
        public void Scan()
        {
            foreach (var source in inputSources)
            {
                source.Scan();
            }
        }

        public override void Update(GameTime gameTime)
        {
            GlobalInputState.Reset();

            // Recycle input event to reduce garbage generation
            foreach (var evt in inputEvents)
            {
                // The router takes care of putting the event back in its respective InputEventPool since it already has the type information
                eventRouters[evt.GetType()].PoolEvent(evt);
            }
            inputEvents.Clear();

            // Update all input sources so they can route events to input devices and possible register new devices
            foreach (var source in inputSources)
            {
                source.Update();
            }

            // Update all input sources so they can send events and update their state
            foreach (var pair in inputDevices)
            {
                pair.Key.Update(inputEvents);
            }

            // Reset action mapping state
            ActionMapping.Reset();

            // Send events to input listeners
            foreach (var evt in inputEvents)
            {
                IInputEventRouter router;
                if (!eventRouters.TryGetValue(evt.GetType(), out router))
                    throw new InvalidOperationException($"The event type {evt.GetType()} was not registered with the input mapper and cannot be processed");
                router.RouteEvent(evt);
            }

            // Update action mappings
            ActionMapping.Update(gameTime.Elapsed);

            // Update gestures
            // TODO: Merge with input actions
            UpdateGestureEvents(gameTime.Elapsed);

            // Update GamePadState for every gamepad
            Utilities.Swap(ref currentGamePadStates, ref lastGamePadStates);
            foreach (var gamepad in gamePadDevices)
            {
                var state = new GamePadState();
                gamepad.GetGamePadState(ref state);
                currentGamePadStates[gamepad.Index] = state;
            }
        }

        /// <summary>
        /// Registers an object that listens for certain types of events using the specialized versions of <see cref="IInputEventListener&lt;"/>
        /// </summary>
        /// <param name="listener">The listener to register</param>
        public void AddListener(IInputEventListener listener)
        {
            foreach (var router in eventRouters)
            {
                router.Value.TryAddListener(listener);
            }
        }

        /// <summary>
        /// Removes a previously registered event listener
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public void RemoveListener(IInputEventListener listener)
        {
            foreach (var pair in eventRouters)
            {
                pair.Value.Listeners.Remove(listener);
            }
        }

        /// <summary>
        /// Gets or sets the value indicating if simultaneous multiple finger touches are enabled or not.
        /// If not enabled only the events of one finger at a time are triggered.
        /// </summary>
        public bool MultiTouchEnabled { get; set; } = false;

        public void OnApplicationPaused(object sender, EventArgs e)
        {
            // Pause sources
            foreach (var source in inputSources)
            {
                source.Pause();
            }
        }

        public void OnApplicationResumed(object sender, EventArgs e)
        {
            // Resume sources
            foreach (var source in inputSources)
            {
                source.Resume();
            }
        }

        /// <summary>
        /// Adds a new input source to be used by the input manager
        /// </summary>
        /// <param name="source">The input source to add</param>
        public void AddInputSource(IInputSource source)
        {
            if (inputSources.Contains(source)) throw new InvalidOperationException("Input Source already added");

            inputSources.Add(source);
            source.InputDevices.CollectionChanged += (sender, args) => InputDevicesOnCollectionChanged(source, args);
            source.Initialize(this);
        }

        /// <summary>
        /// Registers an input event type to process
        /// </summary>
        /// <typeparam name="TEventType">The event type to process</typeparam>
        public void RegisterEventType<TEventType>() where TEventType : InputEvent, new()
        {
            var type = typeof(TEventType);
            eventRouters.Add(type, new InputEventRouter<TEventType>());
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
            if (!HasPointer)
                throw new InvalidOperationException("Need a pointer to use gestures");
            // TODO: Allow gestures for multiple pointer devices?
            gestureConfigToRecognizer.Add(config, config.CreateRecognizer(Pointer.SurfaceAspectRatio));
        }

        private void StopGestureRecognition(GestureConfig config)
        {
            if (!HasPointer)
                throw new InvalidOperationException("Need a pointer to use gestures");
            gestureConfigToRecognizer.Remove(config);
        }

        private void SetMousePosition(Vector2 normalizedPosition)
        {
            // Set mouse position for first pointer device
            if (HasMouse)
            {
                Mouse.SetMousePosition(normalizedPosition);
            }
        }

        private void UpdateGestureEvents(TimeSpan elapsedGameTime)
        {
            gestureEvents.Clear();

            // Only pick out events that lie between Up/Down or are Up/Down events
            var filteredPointerEvents = GlobalInputState.PointerEvents.Where(x => x.IsDown || x.State != PointerState.Move).ToList();

            foreach (var gestureRecognizer in gestureConfigToRecognizer.Values)
            {
                gestureEvents.AddRange(gestureRecognizer.ProcessPointerEvents(elapsedGameTime, filteredPointerEvents));
            }
        }

        private void InputDevicesOnCollectionChanged(IInputSource source, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnInputDeviceAdded(source, (IInputDevice)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnInputDeviceRemoved((IInputDevice)e.Item);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported collection operation");
            }
        }

        private void OnInputDeviceAdded(IInputSource source, IInputDevice device)
        {
            inputDevices.Add(device, source);
            if (inputDevicesById.ContainsKey(device.Id))
                throw new InvalidOperationException($"Device with Id {device.Id}({device.DeviceName}) already registered to {inputDevicesById[device.Id].DeviceName}");
            inputDevicesById.Add(device.Id, device);

            if (device is IKeyboardDevice)
            {
                RegisterKeyboard((IKeyboardDevice)device);
                keyboardDevices.Sort((l, r) => -l.Priority.CompareTo(r.Priority));
            }
            else if (device is IPointerDevice)
            {
                RegisterPointer((IPointerDevice)device);
                pointerDevices.Sort((l, r) => -l.Priority.CompareTo(r.Priority));
            }
            else if (device is IGamePadDevice)
            {
                RegisterGamePad((IGamePadDevice)device);
                gamePadDevices.Sort((l, r) => -l.Priority.CompareTo(r.Priority));
            }
            else if (device is ISensorDevice)
            {
                RegisterSensor((ISensorDevice)device);
            }
        }

        private void OnInputDeviceRemoved(IInputDevice device)
        {
            if (!inputDevices.ContainsKey(device))
                throw new InvalidOperationException("Input device was not registered");
            inputDevices.Remove(device);
            inputDevicesById.Remove(device.Id);

            if (device is IKeyboardDevice)
            {
                UnregisterKeyboard((IKeyboardDevice)device);
            }
            else if (device is IPointerDevice)
            {
                UnregisterPointer((IPointerDevice)device);
            }
            else if (device is IGamePadDevice)
            {
                UnregisterGamePad((IGamePadDevice)device);
            }
            else if (device is ISensorDevice)
            {
                UnregisterSensor((ISensorDevice)device);
            }
        }

        private void RegisterPointer(IPointerDevice pointer)
        {
            pointerDevices.Add(pointer);
        }

        private void UnregisterPointer(IPointerDevice pointer)
        {
            pointerDevices.Remove(pointer);
        }

        private void RegisterKeyboard(IKeyboardDevice keyboard)
        {
            keyboardDevices.Add(keyboard);
        }

        private void UnregisterKeyboard(IKeyboardDevice keyboard)
        {
            keyboardDevices.Remove(keyboard);
        }

        private void RegisterGamePad(IGamePadDevice gamePad)
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
            if (gamepadBase == null)
                throw new InvalidOperationException("Cannot register GamePad id, because it does not inherit from GamePadDeviceBase");
            gamepadBase.IndexInternal = targetIndex;
        }

        private void UnregisterGamePad(IGamePadDevice gamePad)
        {
            // Free the gamepad index in the gamepad list
            // this will allow another gamepad to use this index again
            if (gamepadIds.Count <= gamePad.Index || gamePad.Index < 0)
                throw new IndexOutOfRangeException("Gamepad index was out of range");
            gamepadIds[gamePad.Index] = Guid.Empty;

            gamePadDevices.Remove(gamePad);
        }

        private void UpdateDefaultSensors()
        {
            Accelerometer = (IAccelerometerSensor)sensorDevices.FirstOrDefault(x => x is IAccelerometerSensor);
            Gyroscope = (IGyroscopeSensor)sensorDevices.FirstOrDefault(x => x is IGyroscopeSensor);
            Compass = (ICompassSensor)sensorDevices.FirstOrDefault(x => x is ICompassSensor);
            UserAcceleration = (IUserAccelerationSensor)sensorDevices.FirstOrDefault(x => x is IUserAccelerationSensor);
            Orientation = (IOrientationSensor)sensorDevices.FirstOrDefault(x => x is IOrientationSensor);
            Gravity = (IGravitySensor)sensorDevices.FirstOrDefault(x => x is IGravitySensor);
        }

        private void RegisterSensor(ISensorDevice sensorDevice)
        {
            sensorDevices.Add(sensorDevice);
            UpdateDefaultSensors();
        }

        private void UnregisterSensor(ISensorDevice sensorDevice)
        {
            sensorDevices.Remove(sensorDevice);
            UpdateDefaultSensors();
        }

        protected interface IInputEventRouter
        {
            HashSet<IInputEventListener> Listeners { get; }
            void PoolEvent(InputEvent evt);
            void RouteEvent(InputEvent evt);
            void TryAddListener(IInputEventListener listener);
        }

        protected class InputEventRouter<TEventType> : IInputEventRouter where TEventType : InputEvent, new()
        {
            public HashSet<IInputEventListener> Listeners { get; } = new HashSet<IInputEventListener>(ReferenceEqualityComparer<IInputEventListener>.Default);

            public void RouteEvent(InputEvent evt)
            {
                var listeners = Listeners.ToArray();
                foreach (var gesture in listeners)
                {
                    ((IInputEventListener<TEventType>)gesture).ProcessEvent((TEventType)evt);
                }
            }
            public void TryAddListener(IInputEventListener listener)
            {
                var specific = listener as IInputEventListener<TEventType>;
                if (specific != null)
                {
                    Listeners.Add(specific);
                }
            }
            public void PoolEvent(InputEvent evt)
            {
                InputEventPool<TEventType>.Enqueue((TEventType)evt);
            }
        }
    }
}