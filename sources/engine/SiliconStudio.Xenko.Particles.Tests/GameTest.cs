// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;


namespace SiliconStudio.Xenko.Particles.Tests
{
    /// <summary>
    /// Game class for test on the Input sensors.
    /// </summary>
    class GameTest : GameTestBase
    {

        public GameTest()
        {
            CurrentVersion = 1;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1, };
        }
        
        protected override void Update(GameTime gameTime)
        {
            // TODO Override time so that each frame has the same duration

            base.Update(gameTime);            
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // TODO My custom code here

            // update the values only once every x frames in order to be able to read them.
            if ((gameTime.FrameCount % 20) == 0)
            {
                // TODO Do something based on frame count? Maybe take a screenshot
            }

            // TODO Run the game for X frames then take a screenshot

          //  GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            
        }

        public static void Main()
        {
            using (var game = new GameTest())
            {
                game.Run();
            }
        }        
    }
}

