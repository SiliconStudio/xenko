// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Common material attributes.
    /// </summary>
    [DataContract("MaterialAttributes")]
    [Display("Material Attributes")]
    [CategoryOrder(5, "Geometry")]
    [CategoryOrder(10, "Shading")]
    [CategoryOrder(15, "Misc")]
    public class MaterialAttributes : MaterialFeature, IMaterialAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAttributes"/> class.
        /// </summary>
        public MaterialAttributes()
        {
            CullMode = CullMode.Back;
            Overrides = new MaterialOverrides();
        }

        /// <summary>
        /// Gets or sets the tessellation.
        /// </summary>
        /// <value>The tessellation.</value>
        /// <userdoc>The tessellation method to use for this material. Tessellation consists into subdividing model poligons in order to increase the realism.</userdoc>
        [Display("Tessellation", "Geometry")]
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialTessellationFeature Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the displacement.
        /// </summary>
        /// <value>The displacement.</value>
        /// <userdoc>The displacement method to use for this material. Displacement consists into atering model original vertex position by adding some offsets.</userdoc>
        [Display("Displacement", "Geometry")]
        [DefaultValue(null)]
        [DataMember(20)]
        public IMaterialDisplacementFeature Displacement { get; set; }

        /// <summary>
        /// Gets or sets the surface.
        /// </summary>
        /// <value>The surface.</value>
        /// <userdoc>The method to use to alter macro-surface aspect of this material. A classic example consists in perturbing the normals of the model.</userdoc>
        [Display("Surface", "Geometry")]
        [DefaultValue(null)]
        [DataMember(30)]
        public IMaterialSurfaceFeature Surface { get; set; }

        /// <summary>
        /// Gets or sets the micro surface.
        /// </summary>
        /// <value>The micro surface.</value>
        /// <userdoc>The method to use to alter micro-surface aspect of this material.</userdoc>
        [Display("MicroSurface", "Geometry", Expand = ExpandRule.Once)]
        [DefaultValue(null)]
        [DataMember(40)]
        public IMaterialMicroSurfaceFeature MicroSurface { get; set; }

        /// <summary>
        /// Gets or sets the diffuse.
        /// </summary>
        /// <value>The diffuse.</value>
        /// <userdoc>The method to use to determine the diffuse color of the material. 
        /// The diffuse color of an object corresponds to the essential (pure) color of the object without any reflections.</userdoc>
        [Display("Diffuse", "Shading", Expand = ExpandRule.Once)]
        [DefaultValue(null)]
        [DataMember(50)]
        public IMaterialDiffuseFeature Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the diffuse model.
        /// </summary>
        /// <value>The diffuse model.</value>
        /// <userdoc>The shading model to use to render the material diffuse color.</userdoc>
        [Display("Diffuse Model", "Shading")]
        [DefaultValue(null)]
        [DataMember(60)]
        public IMaterialDiffuseModelFeature DiffuseModel { get; set; }

        /// <summary>
        /// Gets or sets the specular.
        /// </summary>
        /// <value>The specular.</value>
        /// <userdoc>The method to use to determine the specular color of the material. 
        /// The specular color of an object corresponds to the color produced by the reflection of a white light on the object.</userdoc>
        [Display("Specular", "Shading", Expand = ExpandRule.Once)]
        [DefaultValue(null)]
        [DataMember(70)]
        public IMaterialSpecularFeature Specular { get; set; }

        /// <summary>
        /// Gets or sets the specular model.
        /// </summary>
        /// <value>The specular model.</value>
        /// <userdoc>The shading model to use to render the material specular color.</userdoc>
        [Display("Specular Model", "Shading", Expand = ExpandRule.Once)]
        [DefaultValue(null)]
        [DataMember(80)]
        public IMaterialSpecularModelFeature SpecularModel { get; set; }

        /// <summary>
        /// Gets or sets the occlusion.
        /// </summary>
        /// <value>The occlusion.</value>
        /// <userdoc>The occlusion method to use for this material. 
        /// Occlusions consists in modulating the ambient and direct lighting of the material to simulate shadows or cavity artifacts.</userdoc>
        [Display("Occlusion", "Misc")]
        [DefaultValue(null)]
        [DataMember(90)]
        public IMaterialOcclusionFeature Occlusion { get; set; }

        /// <summary>
        /// Gets or sets the emissive.
        /// </summary>
        /// <value>The emissive.</value>
        /// <userdoc>The method to use to determine the emissive color of the material.
        /// The emissive color of an object is the color emitted by the object.</userdoc>
        [Display("Emissive", "Shading")]
        [DefaultValue(null)]
        [DataMember(100)]
        public IMaterialEmissiveFeature Emissive { get; set; }

        /// <summary>
        /// Gets or sets the transparency.
        /// </summary>
        /// <value>The transparency.</value>
        /// <userdoc>The method to use to determine the transparency of the material.</userdoc>
        [Display("Transparency", "Misc")]
        [DefaultValue(null)]
        [DataMember(110)]
        public IMaterialTransparencyFeature Transparency { get; set; }

        /// <summary>
        /// Gets or sets the overrides.
        /// </summary>
        /// <value>The overrides.</value>
        /// <userdoc>Can be used to override some of the properties of the current material.</userdoc>
        [Display("Overrides", "Misc")]
        [DataMember(120)]
        public MaterialOverrides Overrides { get; private set; }
        
        /// <summary>
        /// Gets or sets the cull mode used for the material.
        /// </summary>
        /// <userdoc>Specifies if some faces of the model should be culled depending on their orientation.</userdoc>
        [Display("Cull Mode", "Misc")]
        [DataMember(130)]
        [DefaultValue(CullMode.Back)]
        public CullMode CullMode{ get; set; }

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            // Push overrides of this attributes
            context.PushOverrides(Overrides);

            // Order is important, as some features are dependent on other
            // (For example, Specular can depend on Diffuse in case of Metalness)
            // We may be able to describe a dependency system here, but for now, assume 
            // that it won't change much so it is hardcoded

            // Diffuse - these 2 features are always used as a pair
            if (Diffuse != null && DiffuseModel != null)
            {
                context.Visit(Diffuse);
                context.Visit(DiffuseModel);
            }

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

            // Specular - these 2 features are always used as a pair
            if (Specular != null && SpecularModel != null)
            {
                context.Visit(Specular);
                context.Visit(SpecularModel);
            }

            // Misc
            context.Visit(Occlusion);
            context.Visit(Emissive);
            context.Visit(Transparency);

            // Pop overrides
            context.PopOverrides();

            // Only set the cullmode to something 
            if (CullMode != CullMode.Back)
            {
                if (context.Material.CullMode == null)
                {
                    context.Material.CullMode = CullMode;
                }
            }
        }
    }
}