// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SharpDX;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Common material attributes.
    /// </summary>
    [DataContract("MaterialAttributes")]
    [Display("Material Attributes")]
    public class MaterialAttributes : IMaterialAttributes
    {
        /// <summary>
        /// Gets or sets the tessellation.
        /// </summary>
        /// <value>The tessellation.</value>
        [Display("Tessellation", "Geometry")]
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialTessellationFeature Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the displacement.
        /// </summary>
        /// <value>The displacement.</value>
        [Display("Displacement", "Geometry")]
        [DefaultValue(null)]
        [DataMember(20)]
        public IMaterialDisplacementFeature Displacement { get; set; }

        /// <summary>
        /// Gets or sets the surface.
        /// </summary>
        /// <value>The surface.</value>
        [Display("Surface", "Geometry")]
        [DefaultValue(null)]
        [DataMember(30)]
        public IMaterialSurfaceFeature Surface { get; set; }

        /// <summary>
        /// Gets or sets the micro surface.
        /// </summary>
        /// <value>The micro surface.</value>
        [DefaultValue(null)]
        [DataMember(40)]
        public IMaterialMicroSurfaceFeature MicroSurface { get; set; }

        /// <summary>
        /// Gets or sets the diffuse.
        /// </summary>
        /// <value>The diffuse.</value>
        [DefaultValue(null)]
        [DataMember(50)]
        public IMaterialDiffuseFeature Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the diffuse model.
        /// </summary>
        /// <value>The diffuse model.</value>
        [Display("Diffuse Model")]
        [DefaultValue(null)]
        [DataMember(60)]
        public IMaterialDiffuseModelFeature DiffuseModel { get; set; }

        /// <summary>
        /// Gets or sets the specular.
        /// </summary>
        /// <value>The specular.</value>
        [DefaultValue(null)]
        [DataMember(70)]
        public IMaterialSpecularFeature Specular { get; set; }

        /// <summary>
        /// Gets or sets the specular model.
        /// </summary>
        /// <value>The specular model.</value>
        [Display("Specular Model")]
        [DefaultValue(null)]
        [DataMember(80)]
        public IMaterialSpecularModelFeature SpecularModel { get; set; }

        /// <summary>
        /// Gets or sets the occlusion.
        /// </summary>
        /// <value>The occlusion.</value>
        [DefaultValue(null)]
        [DataMember(90)]
        public IMaterialOcclusionFeature Occlusion { get; set; }

        /// <summary>
        /// Gets or sets the emissive.
        /// </summary>
        /// <value>The emissive.</value>
        [DefaultValue(null)]
        [DataMember(100)]
        public IMaterialEmissiveFeature Emissive { get; set; }

        /// <summary>
        /// Gets or sets the transparency.
        /// </summary>
        /// <value>The transparency.</value>
        [DefaultValue(null)]
        [DataMember(110)]
        public IMaterialTransparencyFeature Transparency { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            // Order is important, as some features are dependent on other
            // (For example, Specular can depend on Diffuse in case of Metalness)
            // We may be able to describe a dependency system here, but for now, assume 
            // that it won't change much so it is hardcoded

            // Surface Geometry
            context.Visit(Tessellation);
            context.Visit(Displacement);
            context.Visit(Surface);
            context.Visit(MicroSurface);

            // If Specular has energy conservative, copy this to the diffuse lambertian model
            // TODO: Should we apply it to any Diffuse Model?
            bool isEnergyConservative = (Specular is MaterialSpecularMapFeature && ((MaterialSpecularMapFeature)Specular).IsEnergyConservative);

            var lambert = DiffuseModel as MaterialDiffuseLambertianModelFeature;
            if (lambert != null)
            {
                lambert.IsEnergyConservative = isEnergyConservative;
            }

            // Diffuse
            context.Visit(Diffuse);
            context.Visit(DiffuseModel);

            // Specular 
            context.Visit(Specular);
            context.Visit(SpecularModel);

            // Misc
            context.Visit(Occlusion);
            context.Visit(Emissive);
            context.Visit(Transparency);
        }
    }
}