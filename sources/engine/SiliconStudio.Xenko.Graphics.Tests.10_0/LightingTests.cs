// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class LightingTests : GameTestBase
    {
        public Action<LightingTests> SetupLighting { get; set; }

        public bool AmbientLight { get; set; } = true;

        public bool PointLight { get; set; }

        public bool SpotLight { get; set; }

        public bool SpotLightShadow { get; set; }

        public bool DirectionalLight { get; set; }

        public bool DirectionalLightShadowOneCascade { get; set; }

        public bool DirectionalLightShadowOneCascade2 { get; set; }

        public bool DirectionalLightShadowFourCascades { get; set; }

        public bool DirectionalLightShadowOneCascadePCF { get; set; }

        public LightingTests()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();

            // Override initial scene
            SceneSystem.InitialSceneUrl = "LightingTests/LightingScene";
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Setup camera script
            var camera = SceneSystem.SceneInstance.First(x => x.Name == "Camera");
            if (camera != null)
            {
                var cameraScript = new FpsTestCamera();
                camera.Add(cameraScript);
            }

            SetupLighting?.Invoke(this);

            // Setup lights
            SceneSystem.SceneInstance.First(x => x.Name == nameof(AmbientLight)).Get<LightComponent>().Enabled = AmbientLight;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(PointLight)).Get<LightComponent>().Enabled = PointLight;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(SpotLight)).Get<LightComponent>().Enabled = SpotLight;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(SpotLightShadow)).Get<LightComponent>().Enabled = SpotLightShadow;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(DirectionalLight)).Get<LightComponent>().Enabled = DirectionalLight;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(DirectionalLightShadowOneCascade)).Get<LightComponent>().Enabled = DirectionalLightShadowOneCascade;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(DirectionalLightShadowOneCascade2)).Get<LightComponent>().Enabled = DirectionalLightShadowOneCascade2;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(DirectionalLightShadowFourCascades)).Get<LightComponent>().Enabled = DirectionalLightShadowFourCascades;
            SceneSystem.SceneInstance.First(x => x.Name == nameof(DirectionalLightShadowOneCascadePCF)).Get<LightComponent>().Enabled = DirectionalLightShadowOneCascadePCF;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Take screenshot first frame
            FrameGameSystem.TakeScreenshot();
        }

        [Test]
        public void SceneNoLighting()
        {
            RunGameTest(new LightingTests { AmbientLight = false });
        }

        [Test]
        public void SceneAmbientLight()
        {
            RunGameTest(new LightingTests());
        }

        [Test]
        public void ScenePointLight()
        {
            RunGameTest(new LightingTests { PointLight = true });
        }

        [Test]
        public void SceneSpotLight()
        {
            RunGameTest(new LightingTests { SpotLight = true });
        }

        [Test]
        public void SceneSpotLightShadow()
        {
            RunGameTest(new LightingTests { SpotLightShadow = true });
        }

        [Test]
        public void SceneDirectionalLight()
        {
            RunGameTest(new LightingTests { DirectionalLight = true });
        }

        [Test]
        public void SceneDirectionalLightShadowOneCascade()
        {
            RunGameTest(new LightingTests { DirectionalLightShadowOneCascade = true });
        }

        [Test]
        public void SceneTwoDirectionalLightShadowOneCascade()
        {
            RunGameTest(new LightingTests { DirectionalLightShadowOneCascade = true, DirectionalLightShadowOneCascade2 = true });
        }

        [Test]
        public void SceneDirectionalLightShadowOneFourCascade()
        {
            RunGameTest(new LightingTests { DirectionalLightShadowOneCascade = true, DirectionalLightShadowFourCascades = true });
        }

        [Test]
        public void SceneDirectionalLightShadowOneCascadePCF()
        {
            RunGameTest(new LightingTests { DirectionalLightShadowOneCascadePCF = true });
        }

        [Test]
        public void SceneDirectionalLightShadowFourCascades()
        {
            RunGameTest(new LightingTests { DirectionalLightShadowFourCascades = true });
        }
    }
}