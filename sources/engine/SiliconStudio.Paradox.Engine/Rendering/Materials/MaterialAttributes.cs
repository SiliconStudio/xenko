// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// Common material attributes.
    /// </summary>
    [DataContract("MaterialAttributes")]
    [Display("Material Attributes")]
    [CategoryOrder(5, "Geometry")]
    [CategoryOrder(10, "Shading")]
    [CategoryOrder(15, "Misc")]
    public class MaterialAttributes : IMaterialAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAttributes"/> class.
        /// </summary>
        public MaterialAttributes()
        {
            Overrides = new MaterialOverrides();
        }

        /// <summary>
        /// Gets or sets the tessellation.
        /// </summary>
        /// <value>The tessellation.</value>
        [Display("Tessellation", null, "Geometry")]
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialTessellationFeature Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the displacement.
        /// </summary>
        /// <value>The displacement.</value>
        [Display("Displacement", null, "Geometry")]
        [DefaultValue(null)]
        [DataMember(20)]
        public IMaterialDisplacementFeature Displacement { get; set; }

        /// <summary>
        /// Gets or sets the surface.
        /// </summary>
        /// <value>The surface.</value>
        [Display("Surface", null, "Geometry")]
        [DefaultValue(null)]
        [DataMember(30)]
        public IMaterialSurfaceFeature Surface { get; set; }

        /// <summary>
        /// Gets or sets the micro surface.
        /// </summary>
        /// <value>The micro surface.</value>
        [Display("MicroSurface", null, "Geometry")]
        [DefaultValue(null)]
        [DataMember(40)]
        public IMaterialMicroSurfaceFeature MicroSurface { get; set; }

        /// <summary>
        /// Gets or sets the diffuse.
        /// </summary>
        /// <value>The diffuse.</value>
        [Display("Diffuse", null, "Shading")]
        [DefaultValue(null)]
        [DataMember(50)]
        public IMaterialDiffuseFeature Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the diffuse model.
        /// </summary>
        /// <value>The diffuse model.</value>
        [Display("Diffuse Model", null, "Shading")]
        [DefaultValue(null)]
        [DataMember(60)]
        public IMaterialDiffuseModelFeature DiffuseModel { get; set; }

        /// <summary>
        /// Gets or sets the specular.
        /// </summary>
        /// <value>The specular.</value>
        [Display("Specular", null, "Shading")]
        [DefaultValue(null)]
        [DataMember(70)]
        public IMaterialSpecularFeature Specular { get; set; }

        /// <summary>
        /// Gets or sets the specular model.
        /// </summary>
        /// <value>The specular model.</value>
        [Display("Specular Model", null, "Shading")]
        [DefaultValue(null)]
        [DataMember(80)]
        public IMaterialSpecularModelFeature SpecularModel { get; set; }

        /// <summary>
        /// Gets or sets the occlusion.
        /// </summary>
        /// <value>The occlusion.</value>
        [Display("Occlusion", null, "Misc")]
        [DefaultValue(null)]
        [DataMember(90)]
        public IMaterialOcclusionFeature Occlusion { get; set; }

        /// <summary>
        /// Gets or sets the emissive.
        /// </summary>
        /// <value>The emissive.</value>
        [Display("Emissive", null, "Shading")]
        [DefaultValue(null)]
        [DataMember(100)]
        public IMaterialEmissiveFeature Emissive { get; set; }

        /// <summary>
        /// Gets or sets the transparency.
        /// </summary>
        /// <value>The transparency.</value>
        [Display("Transparency", null, "Misc")]
        [DefaultValue(null)]
        [DataMember(110)]
        public IMaterialTransparencyFeature Transparency { get; set; }

        /// <summary>
        /// Gets or sets the overrides.
        /// </summary>
        /// <value>The overrides.</value>
        [Display("Overrides", null, "Misc")]
        [DataMember(120)]
        public MaterialOverrides Overrides { get; private set; }

        public void Visit(MaterialGeneratorContext context)
        {
            // Push overrides of this attributes
            context.PushOverrides(Overrides);

            // Order is important, as some features are dependent on other
            // (For example, Specular can depend on Diffuse in case of Metalness)
            // We may be able to describe a dependency system here, but for now, assume 
            // that it won't change much so it is hardcoded

            // Diffuse
            context.Visit(Diffuse);
            context.Visit(DiffuseModel);

            // Surface Geometry
            context.Visit(Tessellation);
            context.Visit(Displacement);
            context.Visit(Surface);
            context.Visit(MicroSurface);

            // If Specular has energy conservative, copy this to the diffuse lambertian model
            // TODO: Should we apply it to any Diffuse Model?
            bool isEnergyConservative = (Specular is MaterialSpecularMapFeature && ((MaterialSpecularMapFeature)Specular).IsEnergyConservative);

            var lambert = DiffuseModel as MaterialDiffuseLambertModelFeature;
            if (lambert != null)
            {
                lambert.IsEnergyConservative = isEnergyConservative;
            }

            // Specular 
            context.Visit(Specular);
            context.Visit(SpecularModel);

            // Misc
            context.Visit(Occlusion);
            context.Visit(Emissive);
            context.Visit(Transparency);

            // Pop overrides
            context.PopOverrides();
        }
    }
}