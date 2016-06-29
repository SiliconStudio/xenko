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
}
