// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input.Gestures;
using SiliconStudio.Xenko.Input.Mapping;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class ActionMappingTest : GameTestBase
    {
        private const float TextSpaceY = 3;
        private const float TextSubSectionOffsetX = 15;

        private static readonly string[] directionNames = new[] { "Right", "Left", "Up", "Down" };
        private static readonly string[] axisNames = new[] { "Positive", "Negative" };

        private readonly Vector2 textLeftTopCorner = new Vector2(5, 5);
        private readonly Color fontColor;

        private SpriteBatch spriteBatch;
        private float textHeight;
        private SpriteFont spriteFont11;
        private Texture roundTexture;
        private int lineOffset = 0;
        
        // List of actions
        List<InputAction> actions = new List<InputAction>();

        private Queue<string> eventLog = new Queue<string>();
        private Stopwatch checkNewDevicesStopwatch = new Stopwatch();

        private InputActionMapping actionMapping;

        // State of action binder
        private InputAction currentlyBindingAction;
        private ActionBinder actionBinder;
        private bool resetBindingsOnBind;

        public ActionMappingTest()
        {
            CurrentVersion = 1;
            //AutoLoadDefaultSettings = true;

            // create and set the Graphic Device to the service register of the parent Game class
            GraphicsDeviceManager.PreferredBackBufferWidth = 1400;
            GraphicsDeviceManager.PreferredBackBufferHeight = 1000;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };

            fontColor = Color.White;
            checkNewDevicesStopwatch.Start();
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();
            SceneSystem.InitialSceneUrl = Settings.DefaultSceneUrl;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Load the default graphics compositor
            SceneSystem.GraphicsCompositor = Content.Load<GraphicsCompositor>("GraphicsCompositor");

            actionMapping = new InputActionMapping(Input);

            // Load the fonts
            spriteFont11 = Content.Load<SpriteFont>("Arial");

            // load the round texture 
            roundTexture = Content.Load<Texture>("round");

            // create the SpriteBatch used to render them
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Measure typical text height
            textHeight = spriteFont11.MeasureString("Dummy").Y;

            SetupActions();

            BuildUI();

            // Save/Load buttons
            for (int i = 0; i < 12; i++)
            {
                var key = new KeyGesture{Key = Keys.F1 + i};
                int saveIndex = i;
                key.Changed += (sender, args) =>
                {
                    if (args.State == ButtonState.Down)
                    {
                        var kb = (IKeyboardDevice)args.Device;
                        if (kb.IsKeyDown(Keys.LeftCtrl) || kb.IsKeyDown(Keys.RightCtrl))
                            SaveBindings(saveIndex);
                        else
                            LoadBindings(saveIndex);
                    }
                };
                Input.Gestures.Add(key);
            }

            // Load first save by default
            LoadBindings(0);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Log events
            foreach (var evt in Input.InputEvents)
            {
                LogEvent(evt.ToString());
            }

            actionMapping.Update(gameTime.Elapsed);

            UpdateActionBinder();

            if (Input.IsKeyReleased(Keys.Escape))
                Exit();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            // clear the screen
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            spriteBatch.Begin(GraphicsContext);
            lineOffset = 0;

            if (currentlyBindingAction != null)
            {
                List<string> accepts = new List<string>();
                if (actionBinder.AcceptsButtons) accepts.AddRange(new[] { "Button", "Key" });
                if (actionBinder.AcceptsAxes) accepts.Add("Axis");
                if (actionBinder.AcceptsDirections) accepts.Add("Direction");

                string buttonName = "Down";
                if (actionBinder is DirectionActionBinder)
                    buttonName = directionNames[actionBinder.Index];
                else if (actionBinder is AxisActionBinder)
                    buttonName = axisNames[actionBinder.Index];

                WriteLine($"Use any {string.Join("/", accepts)} to bind to {buttonName} ({currentlyBindingAction.MappingName})...");
            }
            else
            {
                WriteLine("Press F1-12 to load bindings (with Ctrl held to save them)");
            }
            lineOffset += 1;

            if (Input.HasGamePad)
            {
                WriteLine("Game Pads:");
                foreach (var gamePad in Input.GamePads)
                {
                    WriteLine($"Name: \"{gamePad.Name}\"", 1);
                    WriteLine($"ID: {gamePad.Id}", 2);
                    WriteLine($"PID: {gamePad.ProductId}", 2);
                    WriteLine($"GamePad Index: {gamePad.Index}", 2);
                    WriteLine(gamePad.State.ToString(), 2);
                }
                lineOffset += 1;
            }

            if (Input.HasGameController)
            {
                WriteLine("Game Controllers:");
                foreach (var gameController in Input.GameControllers)
                {
                    WriteLine($"Name: \"{gameController.Name}\"", 1);
                    WriteLine($"ID: {gameController.Id}", 2);
                    WriteLine($"PID: {gameController.ProductId}", 2);
                    WriteLine(gameController.ToString(), 2);
                }
                lineOffset += 1;
            }

            WriteLine("Actions:");
            for (int i = 0; i < actions.Count; i++)
            {
                WriteLine($"[{i}] {actions[i]}", 1);

                // Print out the tree of gestures, some gestures like TwoWay and FourWay have childrens so expand those using a stack with a gesture/indentation amount pair
                Stack<Tuple<IInputGesture, int>> gestures = new Stack<Tuple<IInputGesture, int>>();
                actions[i].ReadOnlyGestures.ForEach(x => gestures.Push(new Tuple<IInputGesture, int>(x, 2)));

                while (gestures.Count > 0)
                {
                    var tuple = gestures.Pop();
                    var gesture = tuple.Item1;
                    if (gesture == null)
                    {
                        WriteLine("null", tuple.Item2);
                    }
                    else if (gesture.GetType() == typeof(TwoWayGesture))
                    {
                        var twoWayGesture = (TwoWayGesture)gesture;
                        WriteLine($"Two Way Gesture (+/-): {twoWayGesture}", tuple.Item2);
                        gestures.Push(new Tuple<IInputGesture, int>(twoWayGesture.Negative, tuple.Item2 + 1));
                        gestures.Push(new Tuple<IInputGesture, int>(twoWayGesture.Positive, tuple.Item2 + 1));
                    }
                    else if (gesture.GetType() == typeof(FourWayGesture))
                    {
                        var fourWayGesture = (FourWayGesture)gesture;
                        WriteLine($"Four Way Gesture (X/Y): {fourWayGesture}", tuple.Item2);
                        gestures.Push(new Tuple<IInputGesture, int>(fourWayGesture.Y, tuple.Item2 + 1));
                        gestures.Push(new Tuple<IInputGesture, int>(fourWayGesture.X, tuple.Item2 + 1));
                    }
                    else if (gesture.GetType() == typeof(AxisButtonGesture))
                    {
                        var axisButton = (AxisButtonGesture)gesture;
                        WriteLine($"Axis Button Gesture: {axisButton}", tuple.Item2);
                        gestures.Push(new Tuple<IInputGesture, int>(axisButton.Axis, tuple.Item2 + 1));
                    }
                    else if (gesture.GetType() == typeof(CompoundAxisGesture))
                    {
                        var compound = (CompoundAxisGesture)gesture;
                        WriteLine($"Compound Axis Gesture: {compound}", tuple.Item2);
                        foreach(var child in compound.Gestures)
                            gestures.Push(new Tuple<IInputGesture, int>(child, tuple.Item2 + 1));
                    }
                    else
                    {
                        WriteLine($"{gesture.GetType().Name}: " + gesture, tuple.Item2);
                    }
                }
            }

            lineOffset += 1;
            WriteLine("Input Events:");
            foreach (var eventLogLine in eventLog.Reverse())
                WriteLine(eventLogLine, 1);


            if (Input.IsPadButtonPressed(0, GamePadButton.A))
                WriteLine("A Button was pressed", 1);
            if (Input.IsPadButtonReleased(0, GamePadButton.A))
                WriteLine("A Button was released", 1);


            if (Input.IsMouseButtonPressed(MouseButton.Left))
                WriteLine("LMB was pressed", 1);
            if (Input.IsMouseButtonReleased(MouseButton.Left))
                WriteLine("LMB was released", 1);

            if (Input.IsKeyPressed(Keys.E))
                WriteLine("E Key was pressed", 1);
            if (Input.IsKeyReleased(Keys.E))
                WriteLine("E Key was released", 1);

            spriteBatch.End();
        }
        
        private void UpdateActionBinder()
        {
            if (actionBinder != null)
            {
                if (actionBinder.Done)
                {
                    if (resetBindingsOnBind) currentlyBindingAction.Clear();
                    currentlyBindingAction.TryAddGesture(actionBinder.TargetGesture);
                    currentlyBindingAction = null;
                    actionBinder.Dispose();
                    actionBinder = null;
                }
            }
        }

        private void SetupActions()
        {
            actionMapping.ClearBindings();
            actions.Clear();

            AddAction<DirectionAction>("Move");
            AddAction<DirectionAction>("Look");
            AddAction<AxisAction>("Speed");
            AddAction<ButtonAction>("Jump");
            AddAction<ButtonAction>("Fire");
        }

        private void AddAction<TType>(string name) where TType : InputAction, new()
        {
            var action = new TType();
            action.MappingName = name;
            actionMapping.AddAction(action);
            actions.Add(action);
        }

        private void StartBindingAction(InputAction action, bool reset)
        {
            // Cancel old operation
            actionBinder?.Dispose();
            actionBinder = null;

            // Setup binding a button/axis or direction
            if (action is ButtonAction)
                actionBinder = new ButtonActionBinder(Input, actionMapping.BoundGestures);
            else if (action is AxisAction)
                actionBinder = new AxisActionBinder(Input, actionMapping.BoundGestures);
            else if (action is DirectionAction)
                actionBinder = new DirectionActionBinder(Input, actionMapping.BoundGestures);
            else
                return;

            resetBindingsOnBind = reset;
            currentlyBindingAction = action;
        }
        
        private void BuildUI()
        {
            var width = 500;
            var bufferRatio = GraphicsDevice.Presenter.BackBuffer.Width / (float)GraphicsDevice.Presenter.BackBuffer.Height;
            var ui = new UIComponent { Resolution = new Vector3(width, width / bufferRatio, 500) };
            SceneSystem.SceneInstance.RootScene.Entities.Add(new Entity { ui });

            var stackPanel = new StackPanel();
            for (int i = 0; i < actions.Count; i++)
            {
                var text = new TextBlock { Font = spriteFont11, Text = $"Action {actions[i].MappingName}", TextSize = 3.5f };
                var add = new Button { Content = new TextBlock { Font = spriteFont11, Text = "Add Binding", TextSize = 3.5f }, BackgroundColor = Color.Gray};
                var clear = new Button { Content = new TextBlock { Font = spriteFont11, Text = "Clear", TextSize = 3.5f }, BackgroundColor = Color.Gray };

                var i1 = i;
                add.Click += (sender, args) =>
                {
                    StartBindingAction(actions[i1], false);
                };
                clear.Click += (sender, args) =>
                {
                    actions[i1].Clear();
                };

                stackPanel.Children.Add(new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Children =
                    {
                        text,
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children = {add, clear},
                            Margin = new Thickness(0,0,0,4)
                        }
                    }
                });
            }

            var numberField = new EditText() { Font = spriteFont11, Text = $"0", TextSize = 3.5f, BackgroundColor = Color.DarkSlateGray };
            var changeGamePad = new Button { Content = new TextBlock { Font = spriteFont11, Text = "Change GamePad IDs", TextSize = 3.5f }, BackgroundColor = Color.Gray };
            changeGamePad.Click += (sender, args) =>
            {
                int newIndex = 0;
                if (int.TryParse(numberField.Text, out newIndex))
                {
                    var config = GetConfiguration();
                    config.ShiftGamePadIndex(newIndex);
                    SetConfiguration(config);
                }
            };
            stackPanel.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { numberField, changeGamePad },
            });

            ui.Page = new UIPage
            {
                RootElement = new Canvas
                {
                    Children = { stackPanel }
                }
            };
            stackPanel.SetCanvasPinOrigin(new Vector3(1.0f, 0, 0));
            stackPanel.SetCanvasRelativePosition(new Vector3(1.0f, 0.0f, 0.0f));
        }
        
        private void WriteLine(string str, int indent = 0)
        {
            spriteBatch.DrawString(spriteFont11, str,
                textLeftTopCorner + new Vector2(TextSubSectionOffsetX * indent, lineOffset++ * (textHeight + TextSpaceY)), fontColor);
        }

        private void LogEvent(string s)
        {
            if (eventLog.Count >= 20)
                eventLog.Dequeue();
            eventLog.Enqueue(s);
        }

        private string GetSavePath(int index)
        {
            return $"input{index}.bin";
        }

        InputActionConfiguration GetConfiguration()
        {
            return new InputActionConfiguration
            {
                Actions = actionMapping.Actions.Select(x => x.Clone()).ToList()
            };
        }

        void SetConfiguration(InputActionConfiguration config)
        {
            foreach (var defaultAction in config.Actions)
            {
                // For each action, restore the gestures.
                //  this will leave all bindings to actions intact and just restore default gesture bindings
                var action = actionMapping.TryGetAction(defaultAction.MappingName);
                //var defaultGestures = defaultAction.CloneGestures();
                action.Clear();
                foreach(var gesture in defaultAction.ReadOnlyGestures)
                    action.TryAddGesture(gesture);
            }
        }

        private void LoadBindings(int index)
        {
            try
            {
                var stream = VirtualFileSystem.ApplicationLocal.OpenStream(GetSavePath(index), VirtualFileMode.Open, VirtualFileAccess.Read);
                using (stream)
                {
                    BinarySerializationReader reader = new BinarySerializationReader(stream);
                    //inputComponent.RestoreBindings(reader.Read<InputActionConfiguration>());
                    InputActionConfiguration config = reader.Read<InputActionConfiguration>();
                    SetConfiguration(config);
                }
            }
            catch (Exception)
            {
            }
        }

        private void SaveBindings(int index)
        {
            string savePath = GetSavePath(index);
            string backupPath = savePath + "_backup";
            var fileProvider = VirtualFileSystem.ApplicationLocal;

            try
            {
                // Make sure data folder exists
                if (!VirtualFileSystem.DirectoryExists(fileProvider.RootPath))
                    VirtualFileSystem.CreateDirectory(fileProvider.RootPath);

                // Remove old file
                if (fileProvider.FileExists(savePath))
                {
                    if (fileProvider.FileExists(backupPath))
                        fileProvider.FileDelete(backupPath);

                    // Move old save to backup location
                    fileProvider.FileMove(savePath, backupPath);
                }

                // Create new file
                var stream = fileProvider.OpenStream(savePath, VirtualFileMode.CreateNew, VirtualFileAccess.Write);
                using (stream)
                {
                    BinarySerializationWriter writer = new BinarySerializationWriter(stream);
                    var config = GetConfiguration();
                    writer.Write(config);
                }
            }
            catch (IOException)
            {
                // Restore backup
                if (fileProvider.FileExists(backupPath))
                {
                    if (fileProvider.FileExists(savePath))
                        fileProvider.FileDelete(savePath);

                    // Move backup to save location
                    fileProvider.FileMove(backupPath, savePath);
                }
            }
        }
        
        [Test]
        public void RunSampleInputTest()
        {
            RunGameTest(new ActionMappingTest());
        }

        public static void Main(string[] args)
        {
            using (var game = new ActionMappingTest())
                game.Run();
        }
    }
}