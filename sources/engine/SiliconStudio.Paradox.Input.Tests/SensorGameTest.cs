// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;

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

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            font = Asset.Load<SpriteFont>("Arial");
            batch = new SpriteBatch(GraphicsDevice);

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

            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, new Color4());
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
