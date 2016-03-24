// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Engine.Tests
{
    /// <summary>
    /// Test <see cref="Material"/>.
    /// </summary>
    public class MaterialTests : EngineTestBase
    {
        private string[] materialsToTests;

        private Entity cube;
        private Entity sphere;
        private TestCamera camera;

        public MaterialTests() : this(new[] { "Features/MaterialMetalness" })
        {
            
        }

        public MaterialTests(string[] materialsToTests)
        {
            this.materialsToTests = materialsToTests;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Load both cube and sphere procedural models
            var cubeModel = Content.Load<Model>("MaterialTests/Cube");
            var sphereModel = Content.Load<Model>("MaterialTests/Sphere");

            cube = new Entity { new ModelComponent { Model = cubeModel } };
            sphere = new Entity { new ModelComponent { Model = sphereModel } };

            cube.Transform.Position.X = -0.8f;
            sphere.Transform.Position.X = 0.8f;

            Scene.Entities.Add(cube);
            Scene.Entities.Add(sphere);

            camera = new TestCamera();
            CameraComponent = camera.Camera;
            Script.Add(camera);

            // Place camera to see both cube and sphere at an interesting angle
            camera.Yaw = (float)(Math.PI * 0.10f);
            camera.Pitch = -(float)(Math.PI * 0.15f);
            camera.Position = new Vector3(0.6f, 1.0f, 2.0f);

            // Make ambient light lower intensity
            AmbientLight.Intensity = 0.2f;

            // Add two directional lights
            var directionalLight1 = new Entity { new LightComponent { Type = new LightDirectional { Color = new ColorRgbProvider(Color.White) }, Intensity = 0.4f } };
            directionalLight1.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(30.0f), MathUtil.DegreesToRadians(-70.0f), 0.0f);
            Scene.Entities.Add(directionalLight1);

            var directionalLight2 = new Entity { new LightComponent { Type = new LightDirectional { Color = new ColorRgbProvider(Color.White) }, Intensity = 0.4f } };
            directionalLight2.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(220.0f), MathUtil.DegreesToRadians(-20.0f), 0.0f);
            Scene.Entities.Add(directionalLight2);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            foreach (var materialUrl in materialsToTests)
            {
                // Generate a more interesting name
                var materialShortName = materialUrl.Substring(materialUrl.IndexOf('/') + 1);
                var testName = typeof(MaterialTests).FullName + "." + materialShortName;

                FrameGameSystem.Draw(() =>
                {
                    // Load material
                    var material = Content.Load<Material>("MaterialTests/" + materialUrl);

                    // Apply it on both cube and sphere
                    cube.Get<ModelComponent>().Model.Materials[0] = material;
                    sphere.Get<ModelComponent>().Model.Materials[0] = material;
                }).TakeScreenshot(null, testName);
            }
        }

        [Test]
        public void TestMaterials()
        {
            RunGameTest(new MaterialTests(new[]
            {
                // Color/Float4 diffuse
                "Base/MaterialDiffuseColor",
                "Base/MaterialDiffuseFloat4",

                // Texture diffuse (with various ComputeTextureColor parameters)
                "Base/MaterialDiffuseTexture",
                "Base/MaterialDiffuseTextureFallback",
                "Base/MaterialDiffuseTextureOffset",
                "Base/MaterialDiffuseTextureScaled",
                "Base/MaterialDiffuseTextureCoord1",
                "Base/MaterialDiffuseTextureClampMirror",
            
                // Binary operators
                // TODO: Auto-generate those programatically? (there is many of them)
                // If we do so, we probably want to do that on diffuse full screen quad to check results against image manipulation software implementations
                "BinaryOperators/MaterialBinaryOperatorMultiply",
                "BinaryOperators/MaterialBinaryOperatorAdd",

                // ComputeColor
                "ComputeColors/MaterialDiffuseComputeColorFixed",

                // Feature maps
                "Features/MaterialMetalness",
                "Features/MaterialSpecular",
                "Features/MaterialNormalMap",
                "Features/MaterialNormalMapCompressed",
                "Features/MaterialEmissive",
                "Features/MaterialCavity",

                // Layers (A, B and C are shading models; first character is root parent, and next characters are its child)
                "Layers/MaterialLayerAAA",
                "Layers/MaterialLayerABB",
                "Layers/MaterialLayerABA",
                "Layers/MaterialLayerABC",
                "Layers/MaterialLayerBAA",
                "Layers/MaterialLayerBBB",
            }));
        }

        [Test]
        public void TestMaterialsTransparent()
        {
            // Note: for now, we separate transparent test since we don't reevalute RenderStage dynamically
            RunGameTest(new MaterialTests(new[]
            {
                "Features/MaterialTransparentBlend",
            }));
        }

        public static void Main()
        {
            using (var game = new MaterialTests())
            {
                game.Run();
            }
        }
    }
}
