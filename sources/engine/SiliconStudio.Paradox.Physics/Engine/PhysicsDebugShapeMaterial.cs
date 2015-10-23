using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;

namespace SiliconStudio.Paradox.Physics
{
    public static class PhysicsDebugShapeMaterial
    {
        public static Material Create(GraphicsDevice device, Color color, float intensity)
        {
            var material = Material.New(device, new MaterialDescriptor
            {
                Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor()),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Emissive = new MaterialEmissiveMapFeature(new ComputeColor())
                }
            });

            // set the color to the material
            material.Parameters.Set(MaterialKeys.DiffuseValue, new Color4(color).ToColorSpace(device.ColorSpace));

            material.Parameters.Set(MaterialKeys.EmissiveIntensity, intensity);
            material.Parameters.Set(MaterialKeys.EmissiveValue, new Color4(color).ToColorSpace(device.ColorSpace));

            return material;
        }
    }
}