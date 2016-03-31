// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Assets.Tests
{
    [TestFixture]
    public class TestMaterialGenerator
    {
        public static readonly ValueParameterKey<Color4> DiffuseValueCustom1 = ParameterKeys.NewValue<Color4>();

        /// <summary>
        /// Test single material (one shading model, no layers)
        /// </summary>
        [Test]
        public void TestSimpleNoLayer()
        {
            // - LayerRoot: SM0 (Shading Model 0)
            var context = new MaterialGeneratorContextExtended();
            var materialDesc = new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Red)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
            };

            var result = MaterialGenerator.Generate(materialDesc, context, "simple_diffuse");
            Assert.False(result.HasErrors);

            var material = result.Material;
            Assert.Null(material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders));
            Assert.Null(material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders));

            // Check that the color is correctly store in the shader parameters
            Assert.AreEqual(new Color4(Color.Red), material.Parameters.Get(MaterialKeys.DiffuseValue));

            var pixelShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);

            var expected = @"!ShaderMixinSource
Mixins:
    -   ClassName: MaterialSurfaceArray
Compositions:
    layers: !ShaderArraySource
        Values:"  
// This is part coming from MaterialDiffuseMapFeature 
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceDiffuse
                Compositions:
                    diffuseMap: !ShaderClassSource
                        ClassName: ComputeColorConstantColorLink
                        GenericArguments: [Material.DiffuseValue]"
// This is part coming from the shading model (MaterialDiffuseLambertModelFeature)
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceLightingAndShading
                Compositions:
                    surfaces: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialSurfaceShadingDiffuseLambert
                                GenericArguments: [false]";

            AssertShaderSourceEqual(expected, pixelShaders);
        }

        /// <summary>
        /// Test material with one shading model and one layer with same single material
        /// </summary>
        [Test]
        public void TestOneLayerSameShadingModel()
        {
            // - LayerRoot: SM0
            //   - Layer1: SM0
            var context = new MaterialGeneratorContextExtended();
            var materialDesc = new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Red)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                },

                Layers =
                {
                    new MaterialBlendLayer()
                    {
                        BlendMap = new ComputeFloat(0.5f),
                        Material = context.MapTo(new Material(), new MaterialDescriptor()
                        {
                            Attributes =
                            {
                                Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Green)
                                {
                                    Key = DiffuseValueCustom1 // Use a custom key in order to detect it in the output
                                }),
                                DiffuseModel = new MaterialDiffuseLambertModelFeature()
                            },
                        })
                    }

                }
            };

            var result = MaterialGenerator.Generate(materialDesc, context, "mix_diffuse");
            Assert.False(result.HasErrors);

            var material = result.Material;
            Assert.Null(material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders));
            Assert.Null(material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders));

            // Check that the color is correctly store in the shader parameters
            Assert.AreEqual(new Color4(Color.Red), material.Parameters.Get(MaterialKeys.DiffuseValue));

            var pixelShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);

            var expected = @"!ShaderMixinSource
Mixins:
    -   ClassName: MaterialSurfaceArray
Compositions:
    layers: !ShaderArraySource
        Values:"
// This is the part coming from the 1st MaterialDiffuseMapFeature 
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceDiffuse
                Compositions:
                    diffuseMap: !ShaderClassSource
                        ClassName: ComputeColorConstantColorLink
                        GenericArguments: [Material.DiffuseValue]"
// This is the part coming from MaterialBlendLayer 
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceStreamsBlend
                Compositions:"
// These streams will be used to blend attributes (in the shader, after "layer" has been processed
+ @"
                    blends: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialStreamLinearBlend
                                GenericArguments: [matDiffuse]
                            - !ShaderClassSource
                                ClassName: MaterialStreamLinearBlend
                                GenericArguments: [matColorBase]"
// Compute the sub-layer
+ @"
                    layer: !ShaderMixinSource
                        Mixins:
                            -   ClassName: MaterialSurfaceArray
                        Compositions:
                            layers: !ShaderArraySource
                                Values:
                                    - !ShaderMixinSource
                                        Mixins:
                                            -   ClassName: MaterialSurfaceDiffuse
                                        Compositions:
                                            diffuseMap: !ShaderClassSource
                                                ClassName: ComputeColorConstantColorLink
                                                GenericArguments: [TestMaterialGenerator.DiffuseValueCustom1]"
// This is the regular code to setup the matBlend attributes (that will be used by "blends")
+ @"
                                    - !ShaderMixinSource
                                        Mixins:
                                            -   ClassName: MaterialSurfaceSetStreamFromComputeColor
                                                GenericArguments: [matBlend, r]
                                        Compositions:
                                            computeColorSource: !ShaderClassSource
                                                ClassName: ComputeColorConstantFloatLink
                                                GenericArguments: [Material.BlendValue]"
// Because we have a single shading model, we expect to have only a shading at the end
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceLightingAndShading
                Compositions:
                    surfaces: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialSurfaceShadingDiffuseLambert
                                GenericArguments: [false]";

            AssertShaderSourceEqual(expected, pixelShaders);
        }

        /// <summary>
        /// Test material with two shading models and one layer
        /// </summary>
        [Test]
        public void TestOneLayer2ShadingModels()
        {
            // - LayerRoot: SM0
            //   - Layer1: SM1
            var context = new MaterialGeneratorContextExtended();
            var materialDesc = new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Red)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                },

                Layers =
                {
                    new MaterialBlendLayer()
                    {
                        BlendMap = new ComputeFloat(0.5f),
                        Material = context.MapTo(new Material(), new MaterialDescriptor()
                        {
                            Attributes =
                            {
                                Specular= new MaterialMetalnessMapFeature(new ComputeFloat(1.0f)),
                                SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                            },
                        })
                    }

                }
            };

            var result = MaterialGenerator.Generate(materialDesc, context, "diffuse_and_specular");
            Assert.False(result.HasErrors);

            var material = result.Material;
            Assert.Null(material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders));
            Assert.Null(material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders));

            // Check that the color is correctly store in the shader parameters
            Assert.AreEqual(new Color4(Color.Red), material.Parameters.Get(MaterialKeys.DiffuseValue));

            var pixelShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);

            var expected = @"!ShaderMixinSource
Mixins:
    -   ClassName: MaterialSurfaceArray
Compositions:
    layers: !ShaderArraySource
        Values:"
// The top-layer MaterialDiffuseMapFeature
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceDiffuse
                Compositions:
                    diffuseMap: !ShaderClassSource
                        ClassName: ComputeColorConstantColorLink
                        GenericArguments: [Material.DiffuseValue]"
// We are shading it immediately, as the layer is switching its ShadingModel
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceLightingAndShading
                Compositions:
                    surfaces: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialSurfaceShadingDiffuseLambert
                                GenericArguments: [false]"
// Next we setup the blending ShadingModel (as the layer is changing the shading model), it will be used by the following MaterialSurfaceShadingBlend
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceSetStreamFromComputeColor
                        GenericArguments: [matBlend, r]
                Compositions:
                    computeColorSource: !ShaderClassSource
                        ClassName: ComputeColorConstantFloatLink
                        GenericArguments: [Material.BlendValue]"
// Perform the shading and blending of the next layer
+ @"
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceShadingBlend
                Compositions:
                    layers: !ShaderArraySource
                        Values:"
// This part is coming from the MaterialMetalnessMapFeature
+ @"
                            - !ShaderMixinSource
                                Mixins:
                                    -   ClassName: MaterialSurfaceMetalness
                                Compositions:
                                    metalnessMap: !ShaderClassSource
                                        ClassName: ComputeColorConstantFloatLink
                                        GenericArguments: [Material.MetalnessValue]"
// Performs the actual shading for this layer
+ @"
                            - !ShaderMixinSource
                                Mixins:
                                    -   ClassName: MaterialSurfaceLightingAndShading
                                Compositions:
                                    surfaces: !ShaderArraySource
                                        Values:
                                            - !ShaderMixinSource
                                                Mixins:
                                                    -   ClassName: MaterialSurfaceShadingSpecularMicrofacet
                                                Compositions:
                                                    fresnelFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetFresnelSchlick
                                                    geometricShadowingFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetVisibilitySmithSchlickGGX
                                                    normalDistributionFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetNormalDistributionGGX";

            AssertShaderSourceEqual(expected, pixelShaders);
        }

        /// <summary>
        /// Test material with 2 shading models and 2 layers (without a shading model on the top layer)
        /// </summary>
        [Test]
        public void Test2Layers2ShadingModels()
        {
            // - LayerRoot:
            //   - Layer1: SM0
            //   - Layer2: SM1
            var context = new MaterialGeneratorContextExtended();
            var materialDesc = new MaterialDescriptor
            {
                Layers =
                {
                    new MaterialBlendLayer()
                    {
                        BlendMap = new ComputeFloat(1.0f),
                        Material = context.MapTo(new Material(), new MaterialDescriptor()
                        {
                            Attributes =
                            {
                                Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(Color.Red)),
                                DiffuseModel = new MaterialDiffuseLambertModelFeature()
                            },
                        })
                    },
                    new MaterialBlendLayer()
                    {
                        BlendMap = new ComputeFloat(0.5f),
                        Material = context.MapTo(new Material(), new MaterialDescriptor()
                        {
                            Attributes =
                            {
                                Specular= new MaterialMetalnessMapFeature(new ComputeFloat(1.0f)),
                                SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                            },
                        })
                    }

                }
            };

            var result = MaterialGenerator.Generate(materialDesc, context, "diffuse_and_specular");
            Assert.False(result.HasErrors);

            var material = result.Material;
            Assert.Null(material.Parameters.Get(MaterialKeys.VertexStageSurfaceShaders));
            Assert.Null(material.Parameters.Get(MaterialKeys.DomainStageSurfaceShaders));

            // Check that the color is correctly store in the shader parameters
            Assert.AreEqual(new Color4(Color.Red), material.Parameters.Get(MaterialKeys.DiffuseValue));

            var pixelShaders = material.Parameters.Get(MaterialKeys.PixelStageSurfaceShaders);

            var expected = @"!ShaderMixinSource
Mixins:
    -   ClassName: MaterialSurfaceArray
Compositions:
    layers: !ShaderArraySource
        Values:
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceStreamsBlend
                Compositions:
                    blends: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialStreamLinearBlend
                                GenericArguments: [matDiffuse]
                            - !ShaderClassSource
                                ClassName: MaterialStreamLinearBlend
                                GenericArguments: [matColorBase]
                    layer: !ShaderMixinSource
                        Mixins:
                            -   ClassName: MaterialSurfaceArray
                        Compositions:
                            layers: !ShaderArraySource
                                Values:
                                    - !ShaderMixinSource
                                        Mixins:
                                            -   ClassName: MaterialSurfaceDiffuse
                                        Compositions:
                                            diffuseMap: !ShaderClassSource
                                                ClassName: ComputeColorConstantColorLink
                                                GenericArguments: [Material.DiffuseValue]
                                    - !ShaderMixinSource
                                        Mixins:
                                            -   ClassName: MaterialSurfaceSetStreamFromComputeColor
                                                GenericArguments: [matBlend, r]
                                        Compositions:
                                            computeColorSource: !ShaderClassSource
                                                ClassName: ComputeColorConstantFloatLink
                                                GenericArguments: [Material.BlendValue]
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceLightingAndShading
                Compositions:
                    surfaces: !ShaderArraySource
                        Values:
                            - !ShaderClassSource
                                ClassName: MaterialSurfaceShadingDiffuseLambert
                                GenericArguments: [false]
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceSetStreamFromComputeColor
                        GenericArguments: [matBlend, r]
                Compositions:
                    computeColorSource: !ShaderClassSource
                        ClassName: ComputeColorConstantFloatLink
                        GenericArguments: [Material.BlendValue.i1]
            - !ShaderMixinSource
                Mixins:
                    -   ClassName: MaterialSurfaceShadingBlend
                Compositions:
                    layers: !ShaderArraySource
                        Values:
                            - !ShaderMixinSource
                                Mixins:
                                    -   ClassName: MaterialSurfaceMetalness
                                Compositions:
                                    metalnessMap: !ShaderClassSource
                                        ClassName: ComputeColorConstantFloatLink
                                        GenericArguments: [Material.MetalnessValue]
                            - !ShaderMixinSource
                                Mixins:
                                    -   ClassName: MaterialSurfaceLightingAndShading
                                Compositions:
                                    surfaces: !ShaderArraySource
                                        Values:
                                            - !ShaderMixinSource
                                                Mixins:
                                                    -   ClassName: MaterialSurfaceShadingSpecularMicrofacet
                                                Compositions:
                                                    fresnelFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetFresnelSchlick
                                                    geometricShadowingFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetVisibilitySmithSchlickGGX
                                                    normalDistributionFunction: !ShaderClassSource
                                                        ClassName: MaterialSpecularMicrofacetNormalDistributionGGX";

            AssertShaderSourceEqual(expected, pixelShaders);
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

        private void AssertShaderSourceEqual(string expected, ShaderSource shaderSource)
        {
            expected = expected.Replace("\r\n", "\n").Trim();
            var textResult = YamlSerializer.Serialize(shaderSource, false).Replace("\r\n", "\n").Trim();
            Console.WriteLine("************************************");
            Console.WriteLine("Result");
            Console.WriteLine("====================================");
            Console.WriteLine(textResult);
            Console.WriteLine("************************************");
            Console.WriteLine("Expected");
            Console.WriteLine("====================================");
            Console.WriteLine(expected);
            Console.Out.Flush();
            Assert.AreEqual(expected, textResult);
        }
    }
}