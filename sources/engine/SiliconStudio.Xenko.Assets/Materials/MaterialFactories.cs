// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
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

    public class CarPaintMaterialFactory : AssetFactory<MaterialAsset>
    {
        public static MaterialAsset Create()
        {
            // Mazda Soul Red Paint (Approximation)
            Color4 defaultCarPaintColor = new Color4(0.274509817f, 0.003921569f, 0.0470588244f, 1.0f);
            Color4 defaultMetalFlakesColor = new Color4(defaultCarPaintColor.R * 2.0f, defaultCarPaintColor.G * 2.0f, defaultCarPaintColor.B * 2.0f, 1.0f);

            var material = new MaterialAsset
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapCarPaintFeature()
                    {
                        DiffuseMap = new ComputeColor(defaultCarPaintColor),
                        MetalFlakesDiffuseMap =  new ComputeColor(defaultMetalFlakesColor)
                    },
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                    Specular = new MaterialMetalnessCarPaintFeature(),
                    MicroSurface = new MaterialGlossinessCarPaintFeature(),
                    Surface = new MaterialNormalMapCarPaint()
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
