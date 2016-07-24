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
    public abstract partial class InputManager : GameSystemBase
    {
        protected const float G = 9.81f; //this is used in some mobile platform for accelerometer stuff

        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;

        #region Constants and Fields

        private readonly List<SensorBase> sensors = new List<SensorBase>();

        internal const float DesiredSensorUpdateRate = 60;

        public static Logger Logger = GlobalLogger.GetLogger("Input");

        internal const float GamePadAxisDeadZone = 0.01f;

        internal readonly List<GamePadFactory> GamePadFactories = new List<GamePadFactory>();

        private const int MaximumGamePadCount = 8;

        private readonly GamePadState[] gamePadStates;

        private readonly GamePad[] gamePads;

        private int gamePadCount;

        private readonly List<Keys> downKeysList = new List<Keys>();

        private readonly HashSet<Keys> pressedKeysSet = new HashSet<Keys>();

        private readonly HashSet<Keys> releasedKeysSet = new HashSet<Keys>();

        private readonly HashSet<GamePadButton>[] pressedGamePadButtonsSet = new HashSet<GamePadButton>[MaximumGamePadCount];

        private readonly HashSet<GamePadButton>[] releasedGamePadButtonsSet = new HashSet<GamePadButton>[MaximumGamePadCount];

        private readonly HashSet<GamePadButton>[] currentGamePadButtonsSet = new HashSet<GamePadButton>[MaximumGamePadCount];

        private readonly HashSet<GamePadButton>[] activeGamePadButtonsSet = new HashSet<GamePadButton>[MaximumGamePadCount];

        private readonly List<GamePadButton> supportedGamePadButtons = new List<GamePadButton>();

        internal List<KeyboardInputEvent> KeyboardInputEvents = new List<KeyboardInputEvent>();

        internal bool LostFocus;

        internal readonly List<MouseInputEvent> MouseInputEvents = new List<MouseInputEvent>();

        internal Vector2 CurrentMousePosition;

        internal Vector2 CurrentMouseDelta;

        private readonly Dictionary<Keys, bool> activeKeys = new Dictionary<Keys, bool>();

        private const int NumberOfMouseButtons = 5;

        private readonly bool[] mouseButtons = new bool[NumberOfMouseButtons];

        private readonly bool[] mouseButtonsPrevious = new bool[NumberOfMouseButtons];

        /// <summary>
        /// The exact current down/up state of the mouse button (not synchronized with the update cycles).
        /// </summary>
        internal readonly bool[] MouseButtonCurrentlyDown = new bool[NumberOfMouseButtons];

        private readonly List<Dictionary<object, float>> virtualButtonValues = new List<Dictionary<object, float>>();

        private readonly List<PointerEvent> pointerEvents = new List<PointerEvent>();

        private readonly List<PointerEvent> currentPointerEvents = new List<PointerEvent>();

        private readonly List<GestureEvent> currentGestureEvents = new List<GestureEvent>();

        private readonly Dictionary<GestureConfig, GestureRecognizer> gestureConfigToRecognizer = new Dictionary<GestureConfig, GestureRecognizer>();

        /// <summary>
        /// List of the gestures to recognize.
        /// </summary>
        /// <remarks>To detect a new gesture add its configuration to the list. 
        /// To stop detecting a gesture remove its configuration from the list. 
        /// To all gestures detection clear the list.
        /// Note that once added to the list the <see cref="GestureConfig"/>s are frozen by the system and cannot be modified anymore.</remarks>
        /// <seealso cref="GestureConfig"/>
        public GestureConfigCollection ActivatedGestures { get; private set; }

        internal readonly Dictionary<int, PointerInfo> PointerInfos = new Dictionary<int, PointerInfo>();

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
        /// The width in pixel of the control
        /// </summary>
        internal float ControlWidth
        {
            get { return controlWidth; }
            set
            {
                controlWidth = Math.Max(0, value);

                if (controlHeight > 0)
                    ScreenAspectRatio = ControlWidth / ControlHeight;
            }
        }

        private float controlWidth;

        /// <summary>
        /// The height in pixel of the control
        /// </summary>
        internal float ControlHeight
        {
            get { return controlHeight; }
            set
            {
                controlHeight = Math.Max(0, value);

                if (controlHeight > 0)
                    ScreenAspectRatio = ControlWidth / ControlHeight;
            }
        }
        private float controlHeight;

        internal float ScreenAspectRatio
        {
            get { return screenAspectRatio; }
            private set
            {
                screenAspectRatio = value;

                foreach (var recognizer in gestureConfigToRecognizer.Values)
                    recognizer.ScreenRatio = ScreenAspectRatio;
            }
        }

        private float screenAspectRatio;

        #endregion

        internal class PointerInfo
        {
            public readonly Stopwatch PointerClock = new Stopwatch();
            public Vector2 LastPosition;
        }

        internal InputManager(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            gamePads = new GamePad[MaximumGamePadCount];
            gamePadStates = new GamePadState[MaximumGamePadCount];

            KeyDown = downKeysList;
            KeyEvents = new List<KeyEvent>();
            PointerEvents = currentPointerEvents;
            GestureEvents = currentGestureEvents;

            ActivatedGestures = new GestureConfigCollection();
            ActivatedGestures.CollectionChanged += ActivatedGesturesChanged;

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

            supportedGamePadButtons.Add(GamePadButton.A);
            supportedGamePadButtons.Add(GamePadButton.B);
            supportedGamePadButtons.Add(GamePadButton.X);
            supportedGamePadButtons.Add(GamePadButton.Y);
            supportedGamePadButtons.Add(GamePadButton.Start);
            supportedGamePadButtons.Add(GamePadButton.Back);
            supportedGamePadButtons.Add(GamePadButton.LeftShoulder);
            supportedGamePadButtons.Add(GamePadButton.RightShoulder);
            supportedGamePadButtons.Add(GamePadButton.RightThumb);
            supportedGamePadButtons.Add(GamePadButton.LeftThumb);
            supportedGamePadButtons.Add(GamePadButton.PadUp);
            supportedGamePadButtons.Add(GamePadButton.PadDown);
            supportedGamePadButtons.Add(GamePadButton.PadLeft);
            supportedGamePadButtons.Add(GamePadButton.PadRight);

            for (var i = 0; i < MaximumGamePadCount; i++)
            {
                pressedGamePadButtonsSet[i] = new HashSet<GamePadButton>();
                releasedGamePadButtonsSet[i] = new HashSet<GamePadButton>();
                currentGamePadButtonsSet[i] = new HashSet<GamePadButton>();
                activeGamePadButtonsSet[i] = new HashSet<GamePadButton>();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Game.Activated += OnApplicationResumed;
            Game.Deactivated += OnApplicationPaused;
        }

        protected override void Destroy()
        {
            base.Destroy();

            Game.Activated -= OnApplicationResumed;
            Game.Deactivated -= OnApplicationPaused;

            // ensure that OnApplicationPaused is called before destruction, when Game.Deactivated event is not triggered.
            OnApplicationPaused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Lock the mouse's position and hides it until the next call to <see cref="UnlockMousePosition"/>.
        /// </summary>
        /// <param name="forceCenter">If true will make sure that the mouse cursor position moves to the center of the client window</param>
        /// <remarks>This function has no effects on devices that does not have mouse</remarks>
        public virtual void LockMousePosition(bool forceCenter = false)
        {
        }

        /// <summary>
        /// Unlock the mouse's position previously locked by calling <see cref="LockMousePosition"/> and restore the mouse visibility.
        /// </summary>
        /// <remarks>This function has no effects on devices that does not have mouse</remarks>
        public virtual void UnlockMousePosition()
        {
        }

        /// <summary>
        /// Gets the value indicating if the mouse position is currently locked or not.
        /// </summary>
        public bool IsMousePositionLocked { get; protected set; }

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

        internal Vector2 NormalizeScreenPosition(Vector2 pixelPosition)
        {
            return new Vector2(pixelPosition.X / ControlWidth, pixelPosition.Y / ControlHeight);
        }

        internal void HandlePointerEvents(int pointerId, Vector2 newPosition, PointerState pState, PointerType pointerType = PointerType.Touch)
        {
            lock (pointerEvents)
            {
                if (!PointerInfos.ContainsKey(pointerId))
                    PointerInfos[pointerId] = new PointerInfo();

                var pointerInfo = PointerInfos[pointerId];

                if (pState == PointerState.Down)
                {
                    pointerInfo.LastPosition = newPosition;
                    pointerInfo.PointerClock.Restart();
                }

                var pointerEvent = PointerEvent.GetOrCreatePointerEvent();

                pointerEvent.PointerId = pointerId;
                pointerEvent.Position = newPosition;
                pointerEvent.DeltaPosition = newPosition - pointerInfo.LastPosition;
                pointerEvent.DeltaTime = pointerInfo.PointerClock.Elapsed;
                pointerEvent.State = pState;
                pointerEvent.PointerType = pointerType;
                pointerEvent.IsPrimary = pointerId == 0;

                lock (pointerEvents)
                    pointerEvents.Add(pointerEvent);

                pointerInfo.LastPosition = newPosition;
                pointerInfo.PointerClock.Restart();
            }
        }

        internal enum InputEventType
        {
            Up,

            Down,

            Wheel,
        }

        /// <summary>
        /// Gets or sets the configuration for virtual buttons.
        /// </summary>
        /// <value>The current binding.</value>
        public VirtualButtonConfigSet VirtualButtonConfigSet { get; set; }

        /// <summary>
        /// Gets a collection of pointer events since the previous updates.
        /// </summary>
        /// <value>The pointer events.</value>
        public List<PointerEvent> PointerEvents { get; private set; }

        /// <summary>
        /// Gets the collection of gesture events since the previous updates.
        /// </summary>
        /// <value>The gesture events.</value>
        public List<GestureEvent> GestureEvents { get; private set; }

        /// <summary>
        /// Gets a value indicating whether gamepads are available.
        /// </summary>
        /// <value><c>true</c> if gamepads are available; otherwise, <c>false</c>.</value>
        public bool HasGamePad
        {
            get
            {
                return gamePadCount > 0;
            }
        }

        /// <summary>
        /// Gets the number of gamepad connected.
        /// </summary>
        /// <value>The number of gamepad connected.</value>
        public int GamePadCount
        {
            get
            {
                return gamePadCount;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the keyboard is available.
        /// </summary>
        /// <value><c>true</c> if the keyboard is available; otherwise, <c>false</c>.</value>
        public bool HasKeyboard { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the mouse is available.
        /// </summary>
        /// <value><c>true</c> if the mouse is available; otherwise, <c>false</c>.</value>
        public bool HasMouse { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether pointer device is available.
        /// </summary>
        /// <value><c>true</c> if pointer devices are available; otherwise, <c>false</c>.</value>
        public bool HasPointer { get; internal set; }

        /// <summary>
        /// Gets the list of keys being pressed down.
        /// </summary>
        /// <value>The key pressed.</value>
        public List<Keys> KeyDown { get; private set; }

        /// <summary>
        /// Gets the list of key events (pressed or released) since the previous update.
        /// </summary>
        /// <value>The key events.</value>
        public List<KeyEvent> KeyEvents { get; private set; }

        private Vector2 mousePosition;

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MousePosition
        {
            get
            {
                return mousePosition;
            }
            set
            {
                SetMousePosition(value);
            }
        }

        /// <summary>
        /// Gets the mouse delta.
        /// </summary>
        /// <value>The mouse position.</value>
        public Vector2 MouseDelta { get; private set; }

        protected virtual void SetMousePosition(Vector2 normalizedPosition)
        {
        }

        /// <summary>
        /// Gets a binding value for the specified name and the specified config extract from the current <see cref="VirtualButtonConfigSet"/>.
        /// </summary>
        /// <param name="configIndex">An index to a <see cref="VirtualButtonConfig"/> stored in the <see cref="VirtualButtonConfigSet"/></param>
        /// <param name="bindingName">Name of the binding.</param>
        /// <returns>The value of the binding.</returns>
        public virtual float GetVirtualButton(int configIndex, object bindingName)
        {
            if (VirtualButtonConfigSet == null || configIndex < 0 || configIndex >= virtualButtonValues.Count)
            {
                return 0.0f;
            }

            float value;
            virtualButtonValues[configIndex].TryGetValue(bindingName, out value);
            return value;
        }

        /// <summary>
        /// Gets the state of the specified gamepad.
        /// </summary>
        /// <param name="gamepadIndex">Index of the gamepad. -1 to return the first connected gamepad</param>
        /// <returns>The state of the gamepad.</returns>
        public virtual GamePadState GetGamePad(int gamepadIndex)
        {
            // If the game pad index is negative or larger, take the first connected gamepad
            if (gamepadIndex < 0)
            {
                gamepadIndex = 0;
                for (int i = 0; i < gamePadStates.Length; i++)
                {
                    if (gamePadStates[i].IsConnected)
                    {
                        gamepadIndex = i;
                        break;
                    }
                }
            }
            else if (gamepadIndex >= gamePadStates.Length)
            {
                for (gamepadIndex = gamePadStates.Length - 1; gamepadIndex >= 0; gamepadIndex--)
                {
                    if (gamePadStates[gamepadIndex].IsConnected)
                    {
                        break;
                    }
                }
            }

            return gamePadStates[gamepadIndex];
        }

        /// <summary>
        /// Determines whether the specified game pad button is being pressed down.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonDown(int gamepadIndex, GamePadButton button)
        {
            return activeGamePadButtonsSet[gamepadIndex].Contains(button);
        }

        /// <summary>
        /// Determines whether the specified game pad button is pressed since the previous update.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonPressed(int gamepadIndex, GamePadButton button)
        {
            return pressedGamePadButtonsSet[gamepadIndex].Contains(button);
        }

        /// <summary>
        /// Determines whether the specified game pad button is released since the previous update.
        /// </summary>
        /// <param name="gamepadIndex">A valid game pad index</param>
        /// <param name="button">The button to check</param>
        /// <returns></returns>
        public bool IsPadButtonReleased(int gamepadIndex, GamePadButton button)
        {
            return releasedGamePadButtonsSet[gamepadIndex].Contains(button);
        }

        /// <summary>
        /// Determines whether the specified key is being pressed down.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsKeyDown(Keys key)
        {
            bool pressed;
            activeKeys.TryGetValue(key, out pressed);
            return pressed;
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
            for (int i = 0; i < mouseButtons.Length; ++i)
                if (IsMouseButtonDown((MouseButton)i))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are released
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are released; otherwise, <c>false</c>.</returns>
        public bool HasReleasedMouseButtons()
        {
            for (int i = 0; i < mouseButtons.Length; ++i)
                if (IsMouseButtonReleased((MouseButton)i))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether one or more of the mouse buttons are pressed
        /// </summary>
        /// <returns><c>true</c> if one or more of the mouse buttons are pressed; otherwise, <c>false</c>.</returns>
        public bool HasPressedMouseButtons()
        {
            for (int i = 0; i < mouseButtons.Length; ++i)
                if (IsMouseButtonPressed((MouseButton)i))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified mouse button is being pressed down.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is being pressed down; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return mouseButtons[(int)mouseButton];
        }

        /// <summary>
        /// Determines whether the specified mouse button is pressed since the previous update.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is pressed since the previous update; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return !mouseButtonsPrevious[(int)mouseButton] && mouseButtons[(int)mouseButton];
        }

        /// <summary>
        /// Determines whether the specified mouse button is released.
        /// </summary>
        /// <param name="mouseButton">The mouse button.</param>
        /// <returns><c>true</c> if the specified mouse button is released; otherwise, <c>false</c>.</returns>
        public bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return mouseButtonsPrevious[(int)mouseButton] && !mouseButtons[(int)mouseButton];
        }

        /// <summary>
        /// Rescans all input devices in order to query new device connected. See remarks.
        /// </summary>
        /// <remarks>
        /// This method could take several milliseconds and should be used at specific time in a game where performance is not crucial (pause, configuration screen...etc.)
        /// </remarks>
        public virtual void Scan()
        {
            lock (gamePads)
            {
                List<GamePadKey> gamePadKeys = GamePadFactories.SelectMany(gamePadFactory => gamePadFactory.GetConnectedPads()).ToList();

                int nextAvailable = -1;
                for (int i = 0; i < gamePads.Length; i++)
                {
                    GamePad gamePad = gamePads[i];
                    if (gamePad == null)
                    {
                        if (nextAvailable < 0)
                        {
                            nextAvailable = i;
                        }
                        continue;
                    }

                    if (gamePadKeys.Contains(gamePad.Key))
                    {
                        gamePadKeys.Remove(gamePad.Key);
                    }
                    else
                    {
                        gamePad.Dispose();
                        gamePads[i] = null;

                        if (nextAvailable < 0)
                        {
                            nextAvailable = i;
                        }
                    }
                }

                foreach (GamePadKey gamePadKey in gamePadKeys)
                {
                    int gamePadIndex = -1;
                    for (int i = nextAvailable; i < gamePads.Length; i++)
                    {
                        if (gamePads[i] == null)
                        {
                            gamePadIndex = i;
                            break;
                        }
                    }

                    if (gamePadIndex >= 0)
                    {
                        GamePad gamePad = gamePadKey.Factory.GetGamePad(gamePadKey.Guid);
                        gamePads[gamePadIndex] = gamePad;
                        nextAvailable = gamePadIndex + 1;
                    }
                }

                gamePadCount = 0;
                foreach (GamePad internalGamePad in gamePads)
                {
                    if (internalGamePad != null)
                    {
                        gamePadCount++;
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateSensors();
            UpdateKeyboard();
            UpdateMouse();
            UpdateGamePads();
            UpdatePointerEvents();
            UpdateVirtualButtonValues();
            UpdateGestureEvents(gameTime.Elapsed);

            LostFocus = false;
        }

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

        /// <summary>
        /// Checks if new sensors need to be enabled and activate them.
        /// </summary>
        internal virtual void CheckAndEnableSensors()
        {
        }

        /// <summary>
        /// Update the data values of enabled sensors
        /// </summary>
        internal virtual void UpdateEnabledSensorsData()
        {
        }

        /// <summary>
        /// Checks if old sensors need to be disabled and inactivate them.
        /// </summary>
        internal virtual void CheckAndDisableSensors()
        {
        }

        internal void InjectPointerEvent(PointerEvent e)
        {
            lock (pointerEvents)
            {
                pointerEvents.Add(e);
            }
        }

        private void UpdateGestureEvents(TimeSpan elapsedGameTime)
        {
            currentGestureEvents.Clear();

            foreach (var gestureRecognizer in gestureConfigToRecognizer.Values)
                currentGestureEvents.AddRange(gestureRecognizer.ProcessPointerEvents(elapsedGameTime, currentPointerEvents));
        }

        private void UpdatePointerEvents()
        {
            lock (PointerEvent.Pool)
            {
                foreach (var pointerEvent in currentPointerEvents)
                    PointerEvent.Pool.Enqueue(pointerEvent);

                currentPointerEvents.Clear();
            }

            lock (pointerEvents)
            {
                currentPointerEvents.AddRange(pointerEvents);
                pointerEvents.Clear();
            }
        }

        private void UpdateVirtualButtonValues()
        {
            if (VirtualButtonConfigSet != null)
            {
                for (int i = 0; i < VirtualButtonConfigSet.Count; i++)
                {
                    var config = VirtualButtonConfigSet[i];

                    Dictionary<object, float> mapNameToValue;
                    if (i == virtualButtonValues.Count)
                    {
                        mapNameToValue = new Dictionary<object, float>();
                        virtualButtonValues.Add(mapNameToValue);
                    }
                    else
                    {
                        mapNameToValue = virtualButtonValues[i];
                    }

                    mapNameToValue.Clear();

                    if (config != null)
                    {
                        foreach (var name in config.BindingNames)
                        {
                            mapNameToValue[name] = config.GetValue(this, name);
                        }
                    }
                }
            }
        }

        private void UpdateGamePads()
        {
            for (var i = 0; i < MaximumGamePadCount; i++)
            {
                pressedGamePadButtonsSet[i].Clear();
                releasedGamePadButtonsSet[i].Clear();
                currentGamePadButtonsSet[i].Clear();
                gamePadStates[i].IsConnected = false;
            }

            lock (gamePads)
            {
                for (int i = 0, j = gamePadCount; i < gamePads.Length && j > 0; i++, j--)
                {
                    if (gamePads[i] != null)
                    {
                        // Get the state of the gamepad
                        gamePadStates[i] = gamePads[i].GetState();
                    }
                }
            }

            for (var i = 0; i < MaximumGamePadCount; i++)
            {
                if (!gamePadStates[i].IsConnected) continue;

                foreach (var supportedGamePadButton in supportedGamePadButtons)
                {
                    if ((gamePadStates[i].Buttons & supportedGamePadButton) != 0)
                    {
                        if (!activeGamePadButtonsSet[i].Contains(supportedGamePadButton))
                        {
                            pressedGamePadButtonsSet[i].Add(supportedGamePadButton); //newly pressed button
                            activeGamePadButtonsSet[i].Add(supportedGamePadButton);
                        }

                        currentGamePadButtonsSet[i].Add(supportedGamePadButton);
                    }
                }

                foreach (var button in activeGamePadButtonsSet[i])
                {
                    if (!currentGamePadButtonsSet[i].Contains(button))
                    {
                        releasedGamePadButtonsSet[i].Add(button); //newly released button
                    }
                }

                foreach (var gamePadButton in releasedGamePadButtonsSet[i])
                {
                    activeGamePadButtonsSet[i].Remove(gamePadButton);
                }
            }
        }

        private void UpdateMouse()
        {
            MouseWheelDelta = 0;

            for (int i = 0; i < mouseButtons.Length; ++i)
                mouseButtonsPrevious[i] = mouseButtons[i];

            lock (MouseInputEvents)
            {
                foreach (MouseInputEvent mouseInputEvent in MouseInputEvents)
                {
                    var mouseButton = (int)mouseInputEvent.MouseButton;
                    if (mouseButton < 0 || mouseButton >= mouseButtons.Length)
                        continue;

                    switch (mouseInputEvent.Type)
                    {
                        case InputEventType.Down:
                            mouseButtons[mouseButton] = true;
                            break;
                        case InputEventType.Up:
                            mouseButtons[mouseButton] = false;
                            break;
                        case InputEventType.Wheel:
                            if (mouseInputEvent.MouseButton != MouseButton.Middle)
                            {
                                throw new NotImplementedException();
                            }
                            MouseWheelDelta += mouseInputEvent.Value;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                MouseInputEvents.Clear();

                mousePosition = CurrentMousePosition;
                MouseDelta = CurrentMouseDelta;
                CurrentMouseDelta = Vector2.Zero;
            }


            if (LostFocus)
            {
                for (int i = 0; i < mouseButtons.Length; ++i)
                    mouseButtons[i] = false;
            }
        }

        private void UpdateKeyboard()
        {
            pressedKeysSet.Clear();
            releasedKeysSet.Clear();
            KeyEvents.Clear();

            lock (KeyboardInputEvents)
            {
                foreach (KeyboardInputEvent keyboardInputEvent in KeyboardInputEvents)
                {
                    var key = keyboardInputEvent.Key;

                    if (key == Keys.None)
                        continue;

                    switch (keyboardInputEvent.Type)
                    {
                        case InputEventType.Down:
                            if (!IsKeyDown(key)) // prevent from several inconsistent pressed key due to OS repeat key  
                            {
                                activeKeys[key] = true;
                                if (!keyboardInputEvent.OutOfFocus)
                                {
                                    pressedKeysSet.Add(key);
                                    KeyEvents.Add(new KeyEvent(key, KeyEventType.Pressed));
                                }
                                downKeysList.Add(key);
                            }
                            break;
                        case InputEventType.Up:
                            activeKeys[key] = false;
                            releasedKeysSet.Add(key);
                            KeyEvents.Add(new KeyEvent(key, KeyEventType.Released));
                            downKeysList.Remove(key);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                KeyboardInputEvents.Clear();
            }

            if (LostFocus)
            {
                activeKeys.Clear();
                downKeysList.Clear();
            }
        }

        internal static float ClampDeadZone(float value, float deadZone)
        {
            if (value > 0.0f)
            {
                value -= deadZone;
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
            }
            else
            {
                value += deadZone;
                if (value > 0.0f)
                {
                    value = 0.0f;
                }
            }

            // Renormalize the value according to the dead zone
            value = value / (1.0f - deadZone);
            return value < -1.0f ? -1.0f : value > 1.0f ? 1.0f : value;
        }

        internal struct GamePadKey : IEquatable<GamePadKey>
        {
            #region Constants and Fields

            public readonly GamePadFactory Factory;

            public readonly Guid Guid;

            #endregion

            public GamePadKey(Guid guid, GamePadFactory factory)
            {
                Guid = guid;
                Factory = factory;
            }

            public bool Equals(GamePadKey other)
            {
                return Guid.Equals(other.Guid) && Factory.Equals(other.Factory);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is GamePadKey && Equals((GamePadKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Guid.GetHashCode() * 397) ^ Factory.GetHashCode();
                }
            }

            public static bool operator ==(GamePadKey left, GamePadKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GamePadKey left, GamePadKey right)
            {
                return !left.Equals(right);
            }
        }

        internal struct KeyboardInputEvent
        {
            #region Constants and Fields

            public Keys Key;

            public InputEventType Type;

            public bool OutOfFocus;

            #endregion

        }

        internal struct MouseInputEvent
        {
            #region Constants and Fields

            public MouseButton MouseButton;

            public InputEventType Type;

            public float Value;

            #endregion
        }

        /// <summary>
        /// Gets or sets the value indicating if simultaneous multiple finger touches are enabled or not.
        /// If not enabled only the events of one finger at a time are triggered.
        /// </summary>
        public abstract bool MultiTouchEnabled { get; set; }

        public virtual void OnApplicationPaused(object sender, EventArgs e)
        {
        }

        public virtual void OnApplicationResumed(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Base class used to track the state of a gamepad (XInput or DirectInput).
        /// </summary>
        internal abstract class GamePad : IDisposable
        {
            #region Constants and Fields

            /// <summary>
            /// Unique identifier of the gamepad.
            /// </summary>
            public GamePadKey Key;

            #endregion

            protected GamePad(GamePadKey key)
            {
                Key = key;
            }

            public virtual void Dispose()
            {
            }

            public abstract GamePadState GetState();
        }

        /// <summary>
        /// Base class used to track the state of a gamepad (XInput or DirectInput).
        /// </summary>
        internal abstract class GamePadFactory
        {
            public abstract IEnumerable<GamePadKey> GetConnectedPads();

            public abstract GamePad GetGamePad(Guid guid);
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
            return new Vector2((screenCoordinates.X * fromSize.Width - destinationRectangle.X) / destinationRectangle.Width, (screenCoordinates.Y * fromSize.Height - destinationRectangle.Y) / destinationRectangle.Height);
        }
    }

    internal abstract class InputManager<TK> : InputManager
    {
        protected InputManager(IServiceRegistry registry) : base(registry)
        {
        }

        protected TK UiControl;

        public sealed override void Initialize()
        {
            base.Initialize();
            var context = Game.Context as GameContext<TK>;
            if (context != null)
            {
                Initialize(context);
            }
            else
            {
                throw new InvalidOperationException("Incompatible Context and InputManager.");
            }
        }

        /// <summary>
        /// Type safe version of Initialize.
        /// </summary>
        /// <param name="context">Matching context type.</param>
        public abstract void Initialize(GameContext<TK> context);
    }
}
