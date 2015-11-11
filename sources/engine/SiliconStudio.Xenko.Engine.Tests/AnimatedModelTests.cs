// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.ProceduralModels;
using SiliconStudio.Xenko.Rendering.Tessellation;

namespace SiliconStudio.Xenko.Engine.Tests
{
    public class AnimatedModelTests : EngineTestBase
    {
        private Entity knight;
        private TestCamera camera;

        public AnimatedModelTests()
        {
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            knight = new Entity { new ModelComponent { Model = Asset.Load<Model>("knight Model") } };
            knight.Transform.RotationEulerXYZ = new Vector3(-MathUtil.Pi / 2, MathUtil.Pi / 4, 0);
            knight.Transform.Position = new Vector3(0, -50f, 20f);
            knight.Transform.Scale = new Vector3(60.0f);
            var animationComponent = knight.GetOrCreate<AnimationComponent>();
            animationComponent.Animations.Add("Run", Asset.Load<AnimationClip>("knight Run"));
            animationComponent.Animations.Add("Idle", Asset.Load<AnimationClip>("knight Idle"));

            Scene.Entities.Add(knight);

            camera = new TestCamera();
            CameraComponent = camera.Camera;
            Script.Add(camera);

            LightingKeys.EnableFixedAmbientLight(GraphicsDevice.Parameters, true);
            GraphicsDevice.Parameters.Set(EnvironmentLightKeys.GetParameterKey(LightSimpleAmbientKeys.AmbientLight, 0), (Color3)Color.White);

            camera.Position = new Vector3(25, 45, 80);
            camera.SetTarget(knight, true);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Initial frame (no anim
            FrameGameSystem.Draw(() => { }).TakeScreenshot();

            FrameGameSystem.Draw(() =>
            {
                // T = 0
                var playingAnimation = knight.Get<AnimationComponent>().Play("Run");
                playingAnimation.Enabled = false;
            }).TakeScreenshot();

            FrameGameSystem.Draw(() =>
            {
                // T = 0.5sec
                var playingAnimation = knight.Get<AnimationComponent>().PlayingAnimations.First();
                playingAnimation.CurrentTime = TimeSpan.FromSeconds(0.5f);
            }).TakeScreenshot();

            FrameGameSystem.Draw(() =>
            {
                // Blend with Idle (both weighted 1.0f)
                var playingAnimation = knight.Get<AnimationComponent>().Blend("Idle", 1.0f, TimeSpan.Zero);
                playingAnimation.Enabled = false;
            }).TakeScreenshot();
        }

        [Test]
        public void RunTestGame()
        {
            RunGameTest(new AnimatedModelTests());
        }

        static public void Main()
        {
            using (var game = new AnimatedModelTests())
            {
                game.Run();
            }
        }
    }
}