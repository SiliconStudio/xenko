// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Engine.Tests
{
    /// <summary>
    /// Test <see cref="Material"/>.
    /// </summary>
    public class MaterialTests : EngineTestBase
    {
        private string testName;
        private Entity cube;
        private Entity sphere;
        private TestCamera camera;
        private Func<MaterialTests, Material> createMaterial;

        public MaterialTests() : this(null)
        {
            
        }

        private MaterialTests(Func<MaterialTests, Material> createMaterial)
        {
            this.createMaterial = createMaterial;
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

            var material = createMaterial(this);

            // Apply it on both cube and sphere
            cube.Get<ModelComponent>().Model.Materials[0] = material;
            sphere.Get<ModelComponent>().Model.Materials[0] = material;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Take screenshot first frame
            FrameGameSystem.TakeScreenshot(null, testName);
        }

        #region Basic tests (diffuse color/float4)
        [Test]
        public void MaterialDiffuseColor()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseColor")));
        }

        [Test]
        public void MaterialDiffuseFloat4()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseFloat4")));
        }
        #endregion

        #region Test Diffuse ComputeTextureColor with various parameters
        [Test]
        public void MaterialDiffuseTexture()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTexture")));
        }

        // Test ComputeTextureColor.Fallback
        [Test]
        public void MaterialDiffuseTextureFallback()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTextureFallback")));
        }

        // Test texcoord offsets
        [Test]
        public void MaterialDiffuseTextureOffset()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTextureOffset")));
        }

        // Test texcoord scaling
        [Test]
        public void MaterialDiffuseTextureScaled()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTextureScaled")));
        }

        // Test texcoord1
        [Test]
        public void MaterialDiffuseTextureCoord1()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTextureCoord1")));
        }

        // Test uv address modes
        [Test]
        public void MaterialDiffuseTextureClampMirror()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Base/MaterialDiffuseTextureClampMirror")));
        }
        #endregion

        #region Test diffuse binary operators
        [Test]
        public void MaterialBinaryOperatorMultiply()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/BinaryOperators/MaterialBinaryOperatorMultiply")));
        }

        [Test]
        public void MaterialBinaryOperatorAdd()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/BinaryOperators/MaterialBinaryOperatorAdd")));
        }
        #endregion

        #region Test diffuse compute color
        [Test]
        public void MaterialDiffuseComputeColorFixed()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/ComputeColors/MaterialDiffuseComputeColorFixed")));
        }
        #endregion

        #region Test material features (specular, metalness, cavity, normal map, emissive)
        [Test]
        public void MaterialMetalness()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialMetalness")));
        }

        [Test]
        public void MaterialSpecular()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialSpecular")));
        }

        [Test]
        public void MaterialNormalMap()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialNormalMap")));
        }

        [Test]
        public void MaterialNormalMapCompressed()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialNormalMapCompressed")));
        }

        [Test]
        public void MaterialEmissive()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialEmissive")));
        }

        [Test]
        public void MaterialCavity()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Features/MaterialCavity")));
        }
        #endregion

        #region Test layers with different shading models
        // Layers (A, B and C are shading models; first character is root parent, and next characters are its child)
        [Test]
        public void MaterialLayerAAA()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerAAA")));
        }

        // Disabled until XK-3123 is fixed (material blending SM flush results in layer masks applied improperly)
        [Test, Ignore]
        public void MaterialLayerABB()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerABB")));
        }

        [Test]
        public void MaterialLayerABA()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerABA")));
        }

        [Test]
        public void MaterialLayerABC()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerABC")));
        }

        // Disabled until XK-3123 is fixed (material blending SM flush results in layer masks applied improperly)
        [Test, Ignore]
        public void MaterialLayerBAA()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerBAA")));
        }

        [Test]
        public void MaterialLayerBBB()
        {
            RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerBBB")));
        }

        // Similar to MaterialLayerABB but using API for easier debugging
        [Test, Ignore]
        public void MaterialLayerABBWithAPI()
        {
            //RunGameTest(new MaterialTests(game => game.Content.Load<Material>("MaterialTests/Layers/MaterialLayerABB")));
            RunGameTest(new MaterialTests(game =>
            {
                // Use same gold as MaterialLayerABB
                game.testName = typeof(MaterialTests).FullName + "." + nameof(MaterialLayerABB);

                var layerMask = game.Content.Load<Texture>("MaterialTests/Layers/LayerMask");
                var layerMask2 = game.Content.Load<Texture>("MaterialTests/Layers/LayerMask2");

                var diffuse = game.Content.Load<Texture>("MaterialTests/stone4_dif");

                var context = new MaterialGeneratorContextExtended();

                // Load material
                var materialDesc = new MaterialDescriptor
                {
                    Attributes =
                        {
                            Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor { Texture = diffuse }),
                            DiffuseModel = new MaterialDiffuseLambertModelFeature()
                        },
                    Layers =
                        {
                            new MaterialBlendLayer()
                            {
                                BlendMap = new ComputeTextureScalar { Texture = layerMask, Filtering = TextureFilter.Point },
                                Material = context.MapTo(new Material(), new MaterialDescriptor() // MaterialB1
                                {
                                    Attributes =
                                    {
                                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Blue)),
                                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                                        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(0.2f)),
                                        SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                                        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(0.4f)),
                                    },
                                }),
                            },
                            new MaterialBlendLayer()
                            {
                                BlendMap = new ComputeTextureScalar { Texture = layerMask2, Filtering = TextureFilter.Point },
                                Material = context.MapTo(new Material(), new MaterialDescriptor() // MaterialB2
                                {
                                    Attributes =
                                    {
                                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Red)),
                                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                                        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(0.8f)),
                                        SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                                        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(0.9f)),
                                    },
                                }),
                            },
                        },
                };

                return CreateMaterial(materialDesc, context);
            }));
        }

        #endregion

        private static Material CreateMaterial(MaterialDescriptor materialDesc, MaterialGeneratorContextExtended context)
        {
            var result = MaterialGenerator.Generate(materialDesc, context, "test_material");

            if (result.HasErrors)
                throw new InvalidOperationException($"Error compiling material:\n{result.ToText()}");

            return result.Material;
        }

        private class MaterialGeneratorContextExtended : MaterialGeneratorContext
        {
            private readonly Dictionary<object, object> assetMap = new Dictionary<object, object>();

            public MaterialGeneratorContextExtended() : base(null)
            {
                FindAsset = asset =>
                {
                    object value;
                    Assert.True(assetMap.TryGetValue(asset, out value), "A material instance has not been associated to a MaterialDescriptor");
                    return value;
                };
            }

            public T MapTo<T>(T runtime, object asset)
            {
                assetMap[runtime] = asset;
                return runtime;
            }
        }
    }
}
