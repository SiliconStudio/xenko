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
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Particles.Components;


namespace SiliconStudio.Xenko.Particles.Tests
{
    /// <summary>
    /// Game class for test on the Input sensors.
    /// </summary>
    class GameTest : GameTestBase
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
//        private ModelComponent modelComponent;
//        private ModelComponent modelComponent2;
//        private ModelComponent modelComponent3;
//        private Model teapot;

        public GameTest()
        {
            CurrentVersion = 1;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1, };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            font = Asset.Load<SpriteFont>("Font");
//            teapot = Asset.Load<Model>("Teapot");
            batch = new SpriteBatch(GraphicsDevice);

            foreach (var entity1 in SceneSystem.SceneInstance.Scene.Entities)
            {
                foreach (var component in entity1.Components)
                {
                    if (component is ParticleSystemComponent)
                    {
                        System.Console.WriteLine("PArticle System found!");
                    }
                }
            }

//            spriteComponent = new SpriteComponent { SpriteProvider = new SpriteFromSheet { Sheet = Asset.Load<SpriteSheet>("SpriteSheet") } };
//            modelComponent = new ModelComponent { Model = teapot };
//            modelComponent2 = new ModelComponent { Model = teapot };
//            modelComponent3 = new ModelComponent { Model = teapot };
//            entity = new Entity { spriteComponent, modelComponent };
//            entity2 = new Entity { modelComponent2 };
//            entity3 = new Entity { modelComponent3 };
//            SceneSystem.SceneInstance.Scene.Entities.Add(entity);
//            SceneSystem.SceneInstance.Scene.Entities.Add(entity2);
//            SceneSystem.SceneInstance.Scene.Entities.Add(entity3);
            
        }


        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);            
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

