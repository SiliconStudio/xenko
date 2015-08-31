// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Sprites;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.Input.Tests
{
    /// <summary>
    /// Game class for test on the Input sensors.
    /// </summary>
    class SensorGameTest : GraphicsTestBase
    {
        private SpriteFont font;

        private SpriteBatch batch;
        private Vector3 currentAcceleration;
        private float currentHeading;
        private Vector3 currentRotationRate;
        private Vector3 currentUserAcceleration;
        private Vector3 currentGravity;
        private Vector3 currentYawPitchRoww;

        private enum DebugScenes
        {
            Orientation,
            UserAccel,
            Gravity,
            RawAccel,
            Gyroscope,
            Compass
        }
        private DebugScenes currentScene;

        private TextBlock currentText;
        private Entity entity;
        private SpriteComponent spriteComponent;
        private ModelComponent modelComponent;
        private Model teapot;

        public SensorGameTest()
        {
            CurrentVersion = 1;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new [] { GraphicsProfile.Level_11_0, };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            font = Asset.Load<SpriteFont>("Font");
            teapot = Asset.Load<Model>("Teapot");
            batch = new SpriteBatch(GraphicsDevice);

            BuildUI();

            spriteComponent = new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = Asset.Load<SpriteSheet>("SpriteSheet")} };
            modelComponent = new ModelComponent();
            entity = new Entity { spriteComponent, modelComponent };
            SceneSystem.SceneInstance.Scene.Entities.Add(entity);

            if (Input.Accelerometer.IsSupported)
                Input.Accelerometer.IsEnabled = true;

            if (Input.Compass.IsSupported)
                Input.Compass.IsEnabled = true;

            if (Input.Gyroscope.IsSupported)
                Input.Gyroscope.IsEnabled = true;

            if (Input.UserAcceleration.IsSupported)
                Input.UserAcceleration.IsEnabled = true;

            if (Input.Gravity.IsSupported)
                Input.Gravity.IsEnabled = true;

            if (Input.Orientation.IsSupported)
                Input.Orientation.IsEnabled = true;

            ChangeScene(0);
        }

        private void BuildUI()
        {
            var bufferSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            var ui = new UIComponent { VirtualResolution = new Vector3(bufferSize, 500) };
            SceneSystem.SceneInstance.Scene.Entities.Add(new Entity { ui });
            
            currentText = new TextBlock { Font = font, TextColor = Color.White, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center};

            var buttonBack = new Button { Content = new TextBlock { Font = font, Text = "Previous" }, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left};
            var buttonNext = new Button { Content = new TextBlock { Font = font, Text = "Next" }, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right};

            currentText.SetGridColumn(1);
            buttonNext.SetGridColumn(2);

            buttonBack.Click += (o, _) => ChangeScene(-1);
            buttonNext.Click += (o, _) => ChangeScene(+1);

            ui.RootElement = new UniformGrid { Columns = 3, Children = { buttonBack, currentText, buttonNext } };
        }

        private void ChangeScene(int i)
        {
            var sceneCount = Enum.GetNames(typeof(DebugScenes)).Length;
            currentScene = (DebugScenes)((i + (int)currentScene + sceneCount) % sceneCount);

            currentText.Text = currentScene.ToString();

            spriteComponent.Enabled = false;
            modelComponent.Enabled = false;

            switch (currentScene)
            {
                case DebugScenes.Orientation:
                    modelComponent.Enabled = true;
                    modelComponent.Model = teapot;
                    break;
                case DebugScenes.UserAccel:
                    break;
                case DebugScenes.Gravity:
                    break;
                case DebugScenes.RawAccel:
                    break;
                case DebugScenes.Gyroscope:
                    break;
                case DebugScenes.Compass:
                    spriteComponent.Enabled = true;
                    spriteComponent.Color = Color.Red;
                    spriteComponent.CurrentFrame = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (currentScene)
            {
                case DebugScenes.Orientation:
                    entity.Transform.Rotation = -Input.Orientation.Quaternion;
                    break;
                case DebugScenes.UserAccel:
                    break;
                case DebugScenes.Gravity:
                    break;
                case DebugScenes.RawAccel:
                    break;
                case DebugScenes.Gyroscope:
                    break;
                case DebugScenes.Compass:
                    entity.Transform.RotationEulerXYZ = new Vector3(0, 0, Input.Compass.Heading);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // update the values only once every x frames in order to be able to read them.
            if ((gameTime.FrameCount%20) == 0)
            {
                currentAcceleration = Input.Accelerometer.Acceleration;
                currentHeading = Input.Compass.Heading;
                currentRotationRate = Input.Gyroscope.RotationRate;
                currentUserAcceleration = Input.UserAcceleration.Acceleration;
                currentGravity = Input.Gravity.Vector;
                currentYawPitchRoww = new Vector3(Input.Orientation.Yaw, Input.Orientation.Pitch, Input.Orientation.Roll);
            }

            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);

            var targetSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

            batch.Begin();

            var position = new Vector2(0.005f, 0.01f);
            var text = "Acceleration [{0}] = ({1})".ToFormat(Input.Accelerometer.IsEnabled ? "Enabled" : "Disabled", currentAcceleration);
            var size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Compass [{0}] = ({1})".ToFormat(Input.Compass.IsEnabled ? "Enabled" : "Disabled", currentHeading);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Gyroscope [{0}] = ({1})".ToFormat(Input.Gyroscope.IsEnabled ? "Enabled" : "Disabled", currentRotationRate);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "UserAcceleration [{0}] = ({1})".ToFormat(Input.UserAcceleration.IsEnabled ? "Enabled" : "Disabled", currentUserAcceleration);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Gravity [{0}] = ({1})".ToFormat(Input.Gravity.IsEnabled ? "Enabled" : "Disabled", currentGravity);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Orientation [{0}] = ({1})".ToFormat(Input.Orientation.IsEnabled ? "Enabled" : "Disabled", currentYawPitchRoww);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);
            position.Y += size.Y;

            batch.End();
        }

        public static void Main()
        {
            using (var game = new SensorGameTest())
            {
                game.Run();
            }
        }

        [Test]
        public void RunSensorTest()
        {
            RunGameTest(new SensorGameTest());
        }
    }
}
