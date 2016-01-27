// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Particles.Components;


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

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // TODO Load the respective scenes here
            var assetManager = Services.GetSafeServiceAs<AssetManager>();


//            SceneSystem.SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>("Scene01"));

            SceneSystem.SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>("MainScene"));

            //// Preload the scene if it exists
            //if (SceneSystem.InitialSceneUrl != null && assetManager.Exists(SceneSystem.InitialSceneUrl))
            //{
            //    SceneSystem.SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>(SceneSystem.InitialSceneUrl));
            //}

            // TODO My custom code here
            //            font = Asset.Load<SpriteFont>("Font");
            //            teapot = Asset.Load<Model>("Teapot");
            //            batch = new SpriteBatch(GraphicsDevice);

        }


        protected override void Update(GameTime gameTime)
        {
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

          //  GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            
        }

        public static void Main()
        {
            using (var game = new GameTest())
            {
                game.Run();
            }
        }

        [Test]
        public void RunSensorTest()
        {
            RunGameTest(new GameTest());
        }
    }
}

