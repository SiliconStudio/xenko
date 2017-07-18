// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Xenko.Assets.Materials
{
    public class DiffuseMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            var material = new MaterialAsset
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature
                    {
                        DiffuseMap = new ComputeTextureColor
                        {
                            FallbackValue = new ComputeColor(new Color(255, 214, 111))
                        }
                    },
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
            };
            return material;
        }

        public override MaterialAsset New()
        {
            return Create();
        }
    }

    public class SpecularMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            var material = new MaterialAsset
            {
                Attributes =
                {
                    MicroSurface = new MaterialGlossinessMapFeature
                    {
                        GlossinessMap = new ComputeFloat(0.6f)
                    },
                    Diffuse = new MaterialDiffuseMapFeature
                    {
                        DiffuseMap = new ComputeTextureColor
                        {
                            FallbackValue = new ComputeColor(new Color(255, 214, 111))
                        }
                    },
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Specular = new MaterialSpecularMapFeature
                    {
                        SpecularMap = new ComputeColor(Color4.White)
                    },
                    SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                }
            };

            return material;
        }

        public override MaterialAsset New()
        {
            return Create();
        }
    }

    public class MetalnessMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            var material = new MaterialAsset
            {
                Attributes =
                {
                    MicroSurface = new MaterialGlossinessMapFeature
                    {
                        GlossinessMap = new ComputeFloat(0.6f)
                    },
                    Diffuse = new MaterialDiffuseMapFeature
                    {
                        DiffuseMap = new ComputeTextureColor
                        {
                            // This is gold
                            FallbackValue = new ComputeColor(new Color4(1.0f, 0.88565079f, 0.609162496f, 1.0f))
                        }
                    },
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Specular = new MaterialMetalnessMapFeature
                    {
                        MetalnessMap = new ComputeFloat(1.0f)
                    },
                    SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                }
            };

            return material;
        }

        public override MaterialAsset New()
        {
            return Create();
        }
    }

    public class GlassMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            var material = new MaterialAsset
            {
                Attributes =
                {
                    MicroSurface = new MaterialGlossinessMapFeature
                    {
                        GlossinessMap = new ComputeFloat(0.95f)
                    },
                    Diffuse = new MaterialDiffuseMapFeature
                    {
                        DiffuseMap = new ComputeColor(new Color4(0.8f, 0.8f, 0.8f, 1.0f))
                    },
                    DiffuseModel = null,
                    Specular = new MaterialMetalnessMapFeature
                    {
                        MetalnessMap = new ComputeFloat(0.0f)
                    },
                    SpecularModel = new MaterialSpecularThinGlassModelFeature()
                }
            };

            return material;
        }

        public override MaterialAsset New()
        {
            return Create();
        }
    }

    public class ClearCoatMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            // Load default texture assets
            var clearCoatLayerNormalMap = new ComputeTextureColor
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("2f76bcba-ae9f-4954-b98d-f94c2102ff86"), "XenkoCarPaintOrangePeelNM"),
                Scale = new Vector2(8, 8)
            };
            
            var metalFlakesNormalMap = new ComputeTextureColor
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("7e2761d1-ef86-420a-b7a7-a0ed1c16f9bb"), "XenkoCarPaintMetalFlakesNM"),
                Scale = new Vector2(128, 128),
                UseRandomTexCoordinates = true
            };

            var metalFlakesMask = new ComputeTextureScalar
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("7e2761d1-ef86-420a-b7a7-a0ed1c16f9bb"), "XenkoCarPaintMetalFlakesNM"),
                Scale = new Vector2(128, 128),
                UseRandomTexCoordinates = true
            };

            // Red Paint
            // Color4 defaultCarPaintColor = new Color4(0.274509817f, 0.003921569f, 0.0470588244f, 1.0f);
            // Color4 defaultMetalFlakesColor = new Color4(defaultCarPaintColor.R * 2.0f, defaultCarPaintColor.G * 2.0f, defaultCarPaintColor.B * 2.0f, 1.0f);

            // Blue Paint
            Color4 defaultPaintColor = new Color4(0, 0.09411765f, 0.329411775f, 1.0f);
            Color4 defaultMetalFlakesColor = new Color4(0, 0.180392161f, 0.6313726f, 1.0f);

            var material = new MaterialAsset
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(defaultPaintColor)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),

                    SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                    Specular = new MaterialMetalnessMapFeature(new ComputeFloat(1.00f)),

                    MicroSurface = new MaterialGlossinessMapFeature(new ComputeBinaryScalar(new ComputeFloat(2.00f), metalFlakesMask, BinaryOperator.Multiply)),

                    Surface = new MaterialNormalMapFeature(metalFlakesNormalMap),

                    ClearCoat = new MaterialClearCoatFeature
                    {
                        BasePaintGlossinessMap = new ComputeBinaryScalar(new ComputeFloat(0.00f), metalFlakesMask, BinaryOperator.Multiply),

                        ClearCoatGlossinessMap = new ComputeFloat(1.00f),
                        ClearCoatLayerNormalMap = clearCoatLayerNormalMap,
                        ScaleAndBiasOrangePeel = true,
                        ClearCoatMetalnessMap = new ComputeFloat(0.50f),

                        MetalFlakesDiffuseMap = new ComputeColor(defaultMetalFlakesColor),
                    }
                }
            };

            return material;
        }

        public override MaterialAsset New()
        {
            return Create();
        }
    }
}
