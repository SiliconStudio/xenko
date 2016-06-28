// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Game class for test on the Input sensors.
    /// </summary>
    class SensorGameTest : GameTestBase
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
        private Dictionary<DebugScenes, Color> sceneToColor = new Dictionary<DebugScenes, Color>
        {
            { DebugScenes.UserAccel, Color.Blue},
            { DebugScenes.Gravity, Color.Green },
            { DebugScenes.RawAccel, Color.Yellow },
            { DebugScenes.Compass, Color.Red}
        };
        private DebugScenes currentScene;

        private TextBlock currentText;
        private Entity entity;
        private Entity entity2;
        private Entity entity3;
        private SpriteComponent spriteComponent;
        private ModelComponent modelComponent;
        private ModelComponent modelComponent2;
        private ModelComponent modelComponent3;
        private Model teapot;

        public SensorGameTest()
        {
            CurrentVersion = 1;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new [] { GraphicsProfile.Level_9_1, };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            font = Content.Load<SpriteFont>("Font");
            teapot = Content.Load<Model>("Teapot");
            batch = new SpriteBatch(GraphicsDevice);

            BuildUI();

            spriteComponent = new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = Content.Load<SpriteSheet>("SpriteSheet") } };
            modelComponent = new ModelComponent { Model = teapot };
            modelComponent2 = new ModelComponent { Model = teapot };
            modelComponent3 = new ModelComponent { Model = teapot };
            entity = new Entity { spriteComponent, modelComponent };
            entity2 = new Entity { modelComponent2 };
            entity3 = new Entity { modelComponent3 };
            SceneSystem.SceneInstance.Scene.Entities.Add(entity);
            SceneSystem.SceneInstance.Scene.Entities.Add(entity2);
            SceneSystem.SceneInstance.Scene.Entities.Add(entity3);

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
            var width = 400;
            var bufferRatio = GraphicsDevice.Presenter.BackBuffer.Width / (float)GraphicsDevice.Presenter.BackBuffer.Height;
            var ui = new UIComponent { Resolution = new Vector3(width, width / bufferRatio, 500) };
            SceneSystem.SceneInstance.Scene.Entities.Add(new Entity { ui });

            currentText = new TextBlock { Font = font, TextColor = Color.White, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center };

            var buttonBack = new Button { Content = new TextBlock { Font = font, Text = "Previous" }, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left };
            var buttonNext = new Button { Content = new TextBlock { Font = font, Text = "Next" }, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right };

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
            modelComponent2.Enabled = false;
            modelComponent3.Enabled = false;
            entity.Transform.Scale = new Vector3(1);
            entity.Transform.Position = new Vector3(0);
            entity.Transform.Rotation = Quaternion.Identity;
            entity.Transform.LocalMatrix = Matrix.Identity;
            entity.Transform.UseTRS = true;
            entity2.Transform.UseTRS = false;

            var provider = spriteComponent.SpriteProvider as SpriteFromSheet;
            switch (currentScene)
            {
                case DebugScenes.Orientation:
                    entity.Transform.Position = new Vector3(0, 0, -0.6f);
                    entity3.Transform.Position = new Vector3(0, 0, 0.6f);
                    entity.Transform.Scale = new Vector3(0.5f);
                    entity3.Transform.Scale = new Vector3(0.5f);
                    modelComponent.Enabled = true;
                    modelComponent2.Enabled = true;
                    modelComponent3.Enabled = true;
                    modelComponent.Model = teapot;
                    break;
                case DebugScenes.UserAccel:
                case DebugScenes.Gravity:
                case DebugScenes.RawAccel:
                    entity.Transform.UseTRS = false;
                    spriteComponent.Enabled = true;
                    spriteComponent.Color = sceneToColor[currentScene];
                    if (provider != null)
                        provider.CurrentFrame = 0;
                    break;
                case DebugScenes.Gyroscope:
                    entity.Transform.Scale = new Vector3(0.5f);
                    modelComponent.Enabled = true;
                    modelComponent.Model = teapot;
                    break;
                case DebugScenes.Compass:
                    spriteComponent.Enabled = true;
                    spriteComponent.Color = Color.Red;
                    if (provider != null)
                        provider.CurrentFrame = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var vector = Vector3.Zero;
            switch (currentScene)
            {
                case DebugScenes.Orientation:
                    break;
                case DebugScenes.UserAccel:
                    vector = Input.UserAcceleration.Acceleration;
                    break;
                case DebugScenes.Gravity:
                    vector = Input.Gravity.Vector;
                    break;
                case DebugScenes.RawAccel:
                    vector = Input.Accelerometer.Acceleration;
                    break;
                case DebugScenes.Gyroscope:
                    vector = Input.Gyroscope.RotationRate;
                    break;
                case DebugScenes.Compass:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (currentScene)
            {
                case DebugScenes.Orientation:
                    entity.Transform.Rotation = Quaternion.Invert(Input.Orientation.Quaternion);
                    entity2.Transform.LocalMatrix = Matrix.Scaling(0.5f) * Matrix.Invert(Input.Orientation.RotationMatrix);
                    entity3.Transform.Rotation = Quaternion.Invert(Quaternion.RotationYawPitchRoll(Input.Orientation.Yaw, Input.Orientation.Pitch, Input.Orientation.Roll));
                    break;
                case DebugScenes.UserAccel:
                case DebugScenes.Gravity:
                case DebugScenes.RawAccel:
                    var vectorStrength = vector.Length();
                    if (vectorStrength > MathUtil.ZeroTolerance)
                    {
                        var newY = -Vector3.Normalize(vector);
                        var newX = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, newY));
                        var newZ = Vector3.Cross(newX, newY);
                        var transformation = Matrix.Identity;
                        transformation.Row1 = (Vector4)newX;
                        transformation.Row2 = -vectorStrength / 15f * (Vector4)newY;
                        transformation.Row3 = (Vector4)newZ;
                        entity.Transform.LocalMatrix = transformation;
                    }
                    break;
                case DebugScenes.Gyroscope:
                    var rotation = Input.Gyroscope.RotationRate * (float)UpdateTime.Elapsed.TotalSeconds;
                    entity.Transform.Rotation *= Quaternion.Invert(Quaternion.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
                    break;
                case DebugScenes.Compass:
                    entity.Transform.RotationEulerXYZ = new Vector3(-MathUtil.PiOverTwo, Input.Compass.Heading, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // update the values only once every x frames in order to be able to read them.
            if ((gameTime.FrameCount % 20) == 0)
            {
                currentAcceleration = Input.Accelerometer.Acceleration;
                currentHeading = Input.Compass.Heading;
                currentRotationRate = Input.Gyroscope.RotationRate;
                currentUserAcceleration = Input.UserAcceleration.Acceleration;
                currentGravity = Input.Gravity.Vector;
                currentYawPitchRoww = new Vector3(Input.Orientation.Yaw, Input.Orientation.Pitch, Input.Orientation.Roll);
            }

            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);

            var targetSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);

            batch.Begin(GraphicsContext);

            var position = new Vector2(0.005f, 0.01f);
            var text = "Acceleration[{0}]=({1:0.00})".ToFormat(Input.Accelerometer.IsEnabled ? "E" : "D", currentAcceleration);
            var size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Compass[{0}]=({1:0.00})".ToFormat(Input.Compass.IsEnabled ? "E" : "D", currentHeading);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Gyro[{0}]=({1:0.00})".ToFormat(Input.Gyroscope.IsEnabled ? "E" : "D", currentRotationRate);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "UserAccel[{0}]=({1:0.00})".ToFormat(Input.UserAcceleration.IsEnabled ? "E" : "D", currentUserAcceleration);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Gravity[{0}]=({1:0.00})".ToFormat(Input.Gravity.IsEnabled ? "E" : "D", currentGravity);
            size = batch.MeasureString(font, text, targetSize);

            batch.DrawString(font, text, position, Color.White);

            position.Y += size.Y;
            text = "Orientation[{0}]=({1:0.00})".ToFormat(Input.Orientation.IsEnabled ? "E" : "D", currentYawPitchRoww);
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
