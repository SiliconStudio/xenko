// Copyright (c) 2014-2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Manages collecting input from connected input device in the form of <see cref="IInputDevice"/> objects. Also provides some convenience functions for most commonly used devices
    /// </summary>
    public partial class InputManager : GameSystemBase
    {
        //this is used in some mobile platform for accelerometer stuff
        internal const float G = 9.81f;
        internal const float DesiredSensorUpdateRate = 60;

        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;

        /// <summary>
        /// The deadzone amount applied to all game controller axes
        /// </summary>
        public static float GameControllerAxisDeadZone = 0.05f;

        internal static Logger Logger = GlobalLogger.GetLogger("Input");

        private readonly List<IInputSource> inputSources = new List<IInputSource>();
        private readonly List<IInputDevice> inputDevices = new List<IInputDevice>();

        // Mapping of device guid to device
        private readonly Dictionary<Guid, IInputDevice> inputDevicesById = new Dictionary<Guid, IInputDevice>();

        // List mapping GamePad index to the guid of the device
        private readonly List<List<IGamePadDevice>> gamePadRequestedIndex = new List<List<IGamePadDevice>>();
        private readonly List<IGamePadDevice> gamePadsByIndex = new List<IGamePadDevice>();

        private readonly List<InputEvent> inputEvents = new List<InputEvent>();

        private readonly List<IKeyboardDevice> keyboardDevices = new List<IKeyboardDevice>();
        private readonly List<IPointerDevice> pointerDevices = new List<IPointerDevice>();
        private readonly List<IGameControllerDevice> gameControllerDevices = new List<IGameControllerDevice>();
        private readonly List<IGamePadDevice> gamePadDevices = new List<IGamePadDevice>();
        private readonly List<ISensorDevice> sensorDevices = new List<ISensorDevice>();

        private readonly Dictionary<Type, IInputEventRouter> eventRouters = new Dictionary<Type, IInputEventRouter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InputManager"/> class.
        /// </summary>
        internal InputManager(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;

            Gestures = new TrackingCollection<IInputGesture>();
            Gestures.CollectionChanged += Gestures_CollectionChanged;

            Services.AddService(typeof(InputManager), this);
        }

        /// <summary>
        /// List of the gestures to recognize.
        /// </summary>
        public TrackingCollection<IInputGesture> Gestures { get; }

        /// <summary>
        /// List of the gestures to recognize.
        /// </summary>
        [Obsolete("Use InputManager.Gestures instead")]
        public TrackingCollection<IInputGesture> ActivatedGestures => Gestures;

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
        public bool IsMousePositionLocked => HasMouse && Mouse.IsPositionLocked;

        /// <summary>
        /// All input events that happened since the last frame
        /// </summary>
        public IReadOnlyList<InputEvent> InputEvents => inputEvents;

        /// <summary>
        /// Gets a value indicating whether pointer device is available.
        /// </summary>
        /// <value><c>true</c> if pointer devices are available; otherwise, <c>false</c>.</value>
        public bool HasPointer { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the mouse is available.
        /// </summary>
        /// <value><c>true</c> if the mouse is available; otherwise, <c>false</c>.</value>
        public bool HasMouse { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the keyboard is available.
        /// </summary>
        /// <value><c>true</c> if the keyboard is available; otherwise, <c>false</c>.</value>
        public bool HasKeyboard { get; private set; }

        /// <summary>
        /// Gets a value indicating whether game controllers are available.
        /// </summary>
        /// <value><c>true</c> if game controllers are available; otherwise, <c>false</c>.</value>
        public bool HasGameController { get; private set; }

        /// <summary>
        /// Gets a value indicating whether gamepads are available.
        /// </summary>
        /// <value><c>true</c> if gamepads are available; otherwise, <c>false</c>.</value>
        public bool HasGamePad { get; private set; }

        /// <summary>
        /// Gets the number of game controllers connected.
        /// </summary>
        /// <value>The number of game controllers connected.</value>
        public int GameControllerCount { get; private set; }

        /// <summary>
        /// Gets the number of gamepads connected.
        /// </summary>
        /// <value>The number of gamepads connected.</value>
        public int GamePadCount { get; private set; }

        /// <summary>
        /// Gets the first pointer device, or null if there is none
        /// </summary>
        public IPointerDevice Pointer { get; private set; }

        /// <summary>
        /// Gets the first mouse pointer device, or null if there is none
        /// </summary>
        public IMouseDevice Mouse { get; private set; }

        /// <summary>
        /// Gets the first keyboard device, or null if there is none
        /// </summary>
        public IKeyboardDevice Keyboard { get; private set; }

        /// <summary>
        /// First device that supports text input, or null if there is none
        /// </summary>
        public ITextInputDevice TextInput { get; private set; }

        /// <summary>
        /// Gets the first gamepad that was added to the device
        /// </summary>
        public IGamePadDevice DefaultGamePad { get; private set; }

        /// <summary>
        /// Gets the collection of connected game controllers
        /// </summary>
        public IReadOnlyList<IGameControllerDevice> GameControllers => gameControllerDevices;

        /// <summary>
        /// Gets the collection of connected gamepads
        /// </summary>
        public IReadOnlyList<IGamePadDevice> GamePads => gamePadDevices;

        /// <summary>
        /// Gets the collection of connected pointing devices (mouses, touchpads, etc)
        /// </summary>
        public IReadOnlyList<IPointerDevice> Pointers => pointerDevices;

        /// <summary>
        /// Gets the collection of connected keyboard inputs
        /// </summary>
        public IReadOnlyList<IKeyboardDevice> Keyboards => keyboardDevices;

        /// <summary>
        /// Gets the collection of connected sensor devices
        /// </summary>
        public IReadOnlyList<ISensorDevice> Sensors => sensorDevices;

        /// <summary>
        /// Gets or sets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MousePosition
        {
            get { return mousePosition; }
            set { SetMousePosition(value); }
        }

        /// <summary>
        /// Raised before new input is sent to their respective event listeners
        /// </summary>
        public event EventHandler<InputPreUpdateEventArgs> PreUpdateInput;

        /// <summary>
        /// Raised when a device was removed from the system
        /// </summary>
        public event EventHandler<DeviceChangedEventArgs> DeviceRemoved;

        /// <summary>
        /// Raised when a device was added to the system
        /// </summary>
        public event EventHandler<DeviceChangedEventArgs> DeviceAdded;

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

            InitializeSources();

            // Register event types
            RegisterEventType<KeyEvent>();
            RegisterEventType<TextInputEvent>();
            RegisterEventType<MouseButtonEvent>();
            RegisterEventType<MouseWheelEvent>();
            RegisterEventType<PointerEvent>();
            RegisterEventType<GameControllerButtonEvent>();
            RegisterEventType<GameControllerAxisEvent>();
            RegisterEventType<PovControllerEvent>();
            RegisterEventType<GamePadButtonEvent>();
            RegisterEventType<GamePadAxisEvent>();

            // Add global input state to listen for input events
            AddListener(this);
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
                Mouse.LockPosition(forceCenter);
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
                Mouse.UnlockPosition();
            }
        }

        /// <summary>
        /// Gets the gamepad with a specific index
        /// </summary>
        /// <param name="gamePadIndex">The index of the gamepad</param>
        /// <returns>The gamepad, or null if no gamepad has this index</returns>
        /// <exception cref="IndexOutOfRangeException">When <paramref name="gamePadIndex"/> is less than 0</exception>
        public IGamePadDevice GetGamePad(int gamePadIndex)
        {
            if (gamePadIndex < 0) throw new IndexOutOfRangeException(nameof(gamePadIndex));
            if (gamePadIndex >= gamePadsByIndex.Count) return null;
            return gamePadsByIndex[gamePadIndex];
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
            ResetGlobalInputState();

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
            foreach (var inputDevice in inputDevices)
            {
                inputDevice.Update(inputEvents);
            }

            // Notify PreUpdateInput
            PreUpdateInput?.Invoke(this, new InputPreUpdateEventArgs { GameTime = gameTime });

            // Pre Update on gestures
            foreach (var gesture in Gestures)
            {
                gesture.PreUpdate(gameTime.Elapsed);
            }

            // Send events to input listeners
            foreach (var evt in inputEvents)
            {
                IInputEventRouter router;
                if (!eventRouters.TryGetValue(evt.GetType(), out router))
                    throw new InvalidOperationException($"The event type {evt.GetType()} was not registered with the input mapper and cannot be processed");

                router.RouteEvent(evt);
            }

            // Update gestures
            foreach (var gesture in Gestures)
            {
                gesture.Update(gameTime.Elapsed);
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

        /// <summary>
        /// Inserts any supported pointer event back into it's respective <see cref="InputEventPool"/>. This should normally not be used
        /// </summary>
        /// <param name="inputEvent">The event to instert into it's event pool</param>
        public void PoolInputEvent(InputEvent inputEvent)
        {
            eventRouters[inputEvent.GetType()].PoolEvent(inputEvent);
        }

        /// <summary>
        /// Reinitializes the input sources, useful if you want to add or remove simulated input
        /// </summary>
        public void ReinitializeSources()
        {
            // Destroy all input sources
            foreach (var source in inputSources)
            {
                source.Dispose();
            }
            inputSources.Clear();

            InitializeSources();
        }

        private void InitializeSources()
        {
            // Don't create any other device when using simulated input
            if (!InputSourceSimulated.Enabled)
            {
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
#if SILICONSTUDIO_PLATFORM_UWP
                    case  AppContextType.UWP:
                        AddInputSource(new InputSourceUWP());
                        break;
#endif
#if SILICONSTUDIO_PLATFORM_WINDOWS && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
                    case AppContextType.Desktop:
                        AddInputSource(new InputSourceWinforms());
                        AddInputSource(new InputSourceWindowsDirectInput());
                        if (InputSourceWindowsXInput.IsSupported())
                            AddInputSource(new InputSourceWindowsXInput());
                        if (UseRawInput)
                            AddInputSource(new InputSourceWindowsRawInput());
                        break;
#endif
                    default:
                        throw new InvalidOperationException("GameContext type is not supported by the InputManager");
                }
            }

            // Simulated input, if enabled
            if (InputSourceSimulated.Enabled)
                AddInputSource(new InputSourceSimulated());
        }

        protected override void Destroy()
        {
            base.Destroy();

            // Unregister all gestures
            Gestures.Clear();

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

        private void Gestures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            // TODO: Rename
            var gesture = trackingCollectionChangedEventArgs.Item as InputGestureBase;
            if (gesture == null) throw new InvalidOperationException("Added gesture does not inherit from InputGestureBase");

            switch (trackingCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    gesture.InputManager = this;
                    gesture.OnAdded();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    gesture.OnRemoved();
                    break;
            }
        }

        private void SetMousePosition(Vector2 normalizedPosition)
        {
            // Set mouse position for first mouse device
            if (HasMouse)
            {
                Mouse.SetPosition(normalizedPosition);
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
            inputDevices.Add(device);
            if (inputDevicesById.ContainsKey(device.Id))
                throw new InvalidOperationException($"Device with Id {device.Id}({device.Name}) already registered to {inputDevicesById[device.Id].Name}");

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
            else if (device is IGameControllerDevice)
            {
                RegisterGameController((IGameControllerDevice)device);
                gameControllerDevices.Sort((l, r) => -l.Priority.CompareTo(r.Priority));
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
            UpdateConnectedDevices();

            DeviceAdded?.Invoke(this, new DeviceChangedEventArgs { Device = device, Source = source, Type = DeviceChangedEventType.Added });
        }

        private void OnInputDeviceRemoved(IInputDevice device)
        {
            if (!inputDevices.Contains(device))
                throw new InvalidOperationException("Input device was not registered");

            var source = device.Source;
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
            else if (device is IGameControllerDevice)
            {
                UnregisterGameController((IGameControllerDevice)device);
            }
            else if (device is IGamePadDevice)
            {
                UnregisterGamePad((IGamePadDevice)device);
            }
            else if (device is ISensorDevice)
            {
                UnregisterSensor((ISensorDevice)device);
            }
            UpdateConnectedDevices();

            DeviceRemoved?.Invoke(this, new DeviceChangedEventArgs { Device = device, Source = source, Type = DeviceChangedEventType.Removed });
        }

        private void UpdateConnectedDevices()
        {
            Keyboard = keyboardDevices.FirstOrDefault();
            HasKeyboard = Keyboard != null;

            TextInput = inputDevices.OfType<ITextInputDevice>().FirstOrDefault();

            Mouse = pointerDevices.OfType<IMouseDevice>().FirstOrDefault();
            HasMouse = Mouse != null;

            Pointer = pointerDevices.FirstOrDefault();
            HasPointer = Pointer != null;

            GameControllerCount = GameControllers.Count;
            HasGameController = GameControllerCount > 0;

            GamePadCount = GamePads.Count;
            HasGamePad = GamePadCount > 0;

            gamePadDevices.Sort((l, r) => l.Index.CompareTo(r.Index));

            DefaultGamePad = gamePadDevices.FirstOrDefault();

            Accelerometer = sensorDevices.OfType<IAccelerometerSensor>().FirstOrDefault();
            Gyroscope = sensorDevices.OfType<IGyroscopeSensor>().FirstOrDefault();
            Compass = sensorDevices.OfType<ICompassSensor>().FirstOrDefault();
            UserAcceleration = sensorDevices.OfType<IUserAccelerationSensor>().FirstOrDefault();
            Orientation = sensorDevices.OfType<IOrientationSensor>().FirstOrDefault();
            Gravity = sensorDevices.OfType<IGravitySensor>().FirstOrDefault();
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

            // Check if the gamepad provides an interface for assigning gamepad index
            var assignable = gamePad as IGamePadIndexAssignable;

            if (assignable != null)
            {
                AssignGamepad(assignable);
            }
            else
            {
                // Add this game controller to the list
                GetOrCreateGamepadRequestedIndexList(gamePad.Index).Add(gamePad);
            }
            ReassignActiveGamepads();

            // Handle later index changed
            gamePad.IndexChanged += GamePadOnIndexChanged;
        }

        private void UnregisterGamePad(IGamePadDevice gamePad)
        {
            // Free the gamepad index in the gamepad list
            // this will allow another gamepad to use this index again
            if (gamePadRequestedIndex.Count <= gamePad.Index || gamePad.Index < 0)
                throw new IndexOutOfRangeException("Gamepad index was out of range");

            gamePadRequestedIndex[gamePad.Index].Remove(gamePad);

            gamePadDevices.Remove(gamePad);
            gamePad.IndexChanged -= GamePadOnIndexChanged;
        }

        private void RegisterGameController(IGameControllerDevice gameController)
        {
            gameControllerDevices.Add(gameController);
        }

        private void UnregisterGameController(IGameControllerDevice gameController)
        {
            gameControllerDevices.Remove(gameController);
        }

        private void GamePadOnIndexChanged(object sender, GamePadIndexChangedEventArgs gamePadIndexChangedEventArgs)
        {
            ReassignActiveGamepads();
        }

        private void RegisterSensor(ISensorDevice sensorDevice)
        {
            sensorDevices.Add(sensorDevice);
        }

        private void UnregisterSensor(ISensorDevice sensorDevice)
        {
            sensorDevices.Remove(sensorDevice);
        }

        private void AssignGamepad(IGamePadIndexAssignable assignable)
        {
            // Find a new index for this game controller
            int targetIndex = 0;
            for (int i = 0; i < gamePadRequestedIndex.Count; i++)
            {
                if (gamePadRequestedIndex[i].Count == 0)
                {
                    targetIndex = i;
                    break;
                }
                targetIndex++;
            }

            GetOrCreateGamepadRequestedIndexList(targetIndex).Add(assignable);
            assignable.Index = targetIndex;
        }

        /// <summary>
        /// Updates the <see cref="gamePadsByIndex"/> collection to 1 gamepad per index, might also try to switch around gamepads so that the
        /// fixed gamepads can have their index exclusively and other gamepads that have assignable index can still be used.
        /// </summary>
        private void ReassignActiveGamepads()
        {
            gamePadsByIndex.Clear();

            // Try to shuffle around gamepad indices until gamepads each have a unique index
            for (int i = 0; i < gamePadRequestedIndex.Count; i++)
            {
                var gamePadList = gamePadRequestedIndex[i];
                if (gamePadList.Count > 1)
                {
                    for (int j = 0; j < gamePadList.Count;)
                    {
                        var assignable = gamePadList[j] as IGamePadIndexAssignable;
                        if (assignable != null)
                        {
                            // Reassign this gamepad
                            gamePadList.RemoveAt(j);
                            AssignGamepad(assignable);
                        }
                        else
                        {
                            j++;
                        }

                        if (gamePadList.Count == 1)
                            break; // Now there is only 1 gamepad with this index
                    }
                }
            }

            for (int i = 0; i < gamePadRequestedIndex.Count; i++)
            {
                var gamePad = gamePadRequestedIndex[i].FirstOrDefault();
                gamePadsByIndex.Add(gamePad);
            }
        }

        /// <summary>
        /// Gets the list that contains the gamepads mapped to this index
        /// </summary>
        /// <param name="gamepadIndex">The index for the id list to get</param>
        /// <returns>A list of ids of gamepads that are mapped to this gamepad index</returns>
        private List<IGamePadDevice> GetOrCreateGamepadRequestedIndexList(int gamepadIndex)
        {
            while (gamepadIndex >= gamePadRequestedIndex.Count)
            {
                gamePadRequestedIndex.Add(new List<IGamePadDevice>());
            }
            return gamePadRequestedIndex[gamepadIndex];
        }

        private interface IInputEventRouter
        {
            HashSet<IInputEventListener> Listeners { get; }

            void PoolEvent(InputEvent evt);

            void RouteEvent(InputEvent evt);

            void TryAddListener(IInputEventListener listener);
        }

        private class InputEventRouter<TEventType> : IInputEventRouter where TEventType : InputEvent, new()
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