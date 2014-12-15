// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Materials
{
    // TODO: Move all these interfaces/classes to single file once the design is stabilized

    /// <summary>
    /// Base interface for a material attribute.
    /// </summary>
    public interface IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for a tessellation material attribute.
    /// </summary>
    public interface IMaterialTessellationAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for a displacement material attribute.
    /// </summary>
    public interface IMaterialDisplacementAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the surface material attribute (normals...etc.)
    /// </summary>
    public interface IMaterialSurfaceAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for a diffuse material attribute.
    /// </summary>
    public interface IMaterialDiffuseAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for a specular material attribute.
    /// </summary>
    public interface IMaterialSpecularAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for a micro-surface material attribute.
    /// </summary>
    public interface IMaterialMicroSurfaceAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the diffuse model material attribute.
    /// </summary>
    public interface IMaterialDiffuseModelAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the specular model material attribute.
    /// </summary>
    public interface IMaterialSpecularModelAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the occlusion material attribute.
    /// </summary>
    public interface IMaterialOcclusionAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the emissive material attribute.
    /// </summary>
    public interface IMaterialEmissiveAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// Base interface for the transparency material attribute.
    /// </summary>
    public interface IMaterialTransparencyAttribute : IMaterialAttribute
    {
    }

    /// <summary>
    /// A material composition.
    /// </summary>
    public interface IMaterialComposition
    {
    }

    /// <summary>
    /// A smoothness map for the micro-surface material attribute.
    /// </summary>
    [DataContract("MaterialSmoothnessMapAttribute")]
    [Display("Smoothness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSmoothnessMapAttribute : IMaterialMicroSurfaceAttribute
    {
        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Smoothness Map")]
        [DefaultValue(null)]
        public IMaterialNode SmoothnessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSmoothnessMapAttribute() { SmoothnessMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A Diffuse map for the diffuse material attribute.
    /// </summary>
    [DataContract("MaterialDiffuseMapAttribute")]
    [Display("Diffuse Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialDiffuseMapAttribute : IMaterialDiffuseAttribute
    {
        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Diffuse Map")]
        [DefaultValue(null)]
        public IMaterialNode DiffuseMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialDiffuseMapAttribute() { DiffuseMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A Specular map for the specular material attribute.
    /// </summary>
    [DataContract("MaterialSpecularMapAttribute")]
    [Display("Specular Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSpecularMapAttribute : IMaterialSpecularAttribute
    {
        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [Display("Specular Map")]
        [DefaultValue(null)]
        public IMaterialNode SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(null)]
        public IMaterialNode Intensity { get; set; }

        /// <summary>
        /// Gets or sets the fresnel.
        /// </summary>
        /// <value>The fresnel.</value>
        [DefaultValue(null)]
        public IMaterialNode Fresnel { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSpecularMapAttribute()
                {
                    SpecularMap = new MaterialTextureNode(),
                    Intensity = new MaterialFloatNode(1.0f),
                    Fresnel = new MaterialFloatNode(1.0f),
                };
            }
        }
    }

    /// <summary>
    /// A Metalness map for the specular material attribute.
    /// </summary>
    [DataContract("MaterialMetalnessMapAttribute")]
    [Display("Metalness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialMetalnessMapAttribute : IMaterialSpecularAttribute
    {
        /// <summary>
        /// Gets or sets the metalness map.
        /// </summary>
        /// <value>The metalness map.</value>
        [Display("Metalness Map")]
        [DefaultValue(null)]
        public IMaterialNode MetalnessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialMetalnessMapAttribute() { MetalnessMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// An occlusion map for the occlusion material attribute.
    /// </summary>
    [DataContract("MaterialOcclusionMapAttribute")]
    [Display("Occlusion Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialOcclusionMapAttribute : IMaterialOcclusionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialOcclusionMapAttribute"/> class.
        /// </summary>
        public MaterialOcclusionMapAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialNode OcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DefaultValue(null)]
        [DataMember(20)]
        public IMaterialNode CavityMap { get; set; }

        /// <summary>
        /// Gets or sets the diffuse cavity influence.
        /// </summary>
        /// <value>The diffuse cavity.</value>
        [Display("Diffuse Cavity")]
        [DefaultValue(null)]
        [DataMember(30)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public IMaterialNode DiffuseCavity { get; set; }

        /// <summary>
        /// Gets or sets the specular cavity.
        /// </summary>
        /// <value>The specular cavity.</value>
        [Display("Specular Cavity")]
        [DefaultValue(null)]
        [DataMember(40)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public IMaterialNode SpecularCavity { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialOcclusionMapAttribute()
                {
                    OcclusionMap = new MaterialTextureNode(),
                    CavityMap = new MaterialTextureNode(),
                    DiffuseCavity = new MaterialFloatNode(1.0f),
                    SpecularCavity = new MaterialFloatNode(1.0f),
                };
            }
        }
    }

    /// <summary>
    /// The diffuse Lambertian for the diffuse material model attribute.
    /// </summary>
    [DataContract("MaterialDiffuseLambertianModelAttribute")]
    [Display("Lamtertian")]
    public class MaterialDiffuseLambertianModelAttribute : IMaterialDiffuseModelAttribute
    {
    }

    /// <summary>
    /// The normal map for a surface material attribute.
    /// </summary>
    [DataContract("MaterialNormalMapAttribute")]
    [Display("Normal Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialNormalMapAttribute : IMaterialSurfaceAttribute
    {
        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        /// <value>The normal map.</value>
        [Display("Normal Map")]
        [DefaultValue(null)]
        public IMaterialNode NormalMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialNormalMapAttribute() { NormalMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A composition material to blend different materials.
    /// </summary>
    [DataContract("MaterialBlendLayerStack")]
    [Display("Material Layers")]
    public class MaterialBlendLayerStack : IMaterialComposition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayerStack"/> class.
        /// </summary>
        public MaterialBlendLayerStack()
        {
            Layers = new List<MaterialBlendLayer>();
        }

        /// <summary>
        /// Gets the layers.
        /// </summary>
        /// <value>The layers.</value>
        public List<MaterialBlendLayer> Layers { get; private set; }
    }

    /// <summary>
    /// A material blend layer
    /// </summary>
    [DataContract("MaterialBlendLayer")]
    [Display("Blend Layer")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialBlendLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayer"/> class.
        /// </summary>
        public MaterialBlendLayer()
        {
            Enabled = true;
            Overrides = new MaterialBlendOverrides();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialBlendLayer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [DataMember(10)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this blend layer.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DefaultValue(null)]
        [DataMember(30)]
        public AssetReference<MaterialAsset2> Material { get; set; }

        /// <summary>
        /// Gets or sets the blend map.
        /// </summary>
        /// <value>The blend map.</value>
        [Display("Blend Map")]
        [DefaultValue(null)]
        [DataMember(40)]
        public IMaterialNode BlendMap { get; set; }

        /// <summary>
        /// Gets or sets the material overrides.
        /// </summary>
        /// <value>The overrides.</value>
        [DataMember(50)]
        public MaterialBlendOverrides Overrides { get; private set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialBlendLayer()
                {
                    BlendMap = new MaterialTextureNode(),
                };
            }
        }
    }

    /// <summary>
    /// Material overrides used in a <see cref="MaterialBlendLayer"/>
    /// </summary>
    [DataContract("MaterialBlendOverrides")]
    [Display("Material Overrides")]
    public class MaterialBlendOverrides
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendOverrides"/> class.
        /// </summary>
        public MaterialBlendOverrides()
        {
            SurfaceContribution = 1.0f;
            MicroSurfaceContribution = 1.0f;
            DiffuseContribution = 1.0f;
            SpecularContribution = 1.0f;
            OcclusionContribution = 1.0f;
            OffsetU = 0.0f;
            OffsetV = 0.0f;
            ScaleU = 1.0f;
            ScaleV = 1.0f;
        }

        /// <summary>
        /// Gets or sets the surface contribution.
        /// </summary>
        /// <value>The surface contribution.</value>
        [Display("Surface Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(10)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float SurfaceContribution { get; set; }

        /// <summary>
        /// Gets or sets the micro surface contribution.
        /// </summary>
        /// <value>The micro surface contribution.</value>
        [Display("MicroSurface Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(20)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float MicroSurfaceContribution { get; set; }

        /// <summary>
        /// Gets or sets the diffuse contribution.
        /// </summary>
        /// <value>The diffuse contribution.</value>
        [Display("Diffuse Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(30)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float DiffuseContribution { get; set; }

        /// <summary>
        /// Gets or sets the specular contribution.
        /// </summary>
        /// <value>The specular contribution.</value>
        [Display("Specular Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(40)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float SpecularContribution { get; set; }

        /// <summary>
        /// Gets or sets the occlusion contribution.
        /// </summary>
        /// <value>The occlusion contribution.</value>
        [Display("Occlusion Contribution")]
        [DefaultValue(1.0f)]
        [DataMember(50)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float OcclusionContribution { get; set; }

        // TODO: Use Vector2 for uv Offset and uv Scales (Check how to integrate with range attribute)

        /// <summary>
        /// Gets or sets the offset u.
        /// </summary>
        /// <value>The offset u.</value>
        [DefaultValue(0.0f)]
        [DataMember(60)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float OffsetU { get; set; }

        /// <summary>
        /// Gets or sets the offset v.
        /// </summary>
        /// <value>The offset v.</value>
        [DefaultValue(0.0f)]
        [DataMember(70)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float OffsetV { get; set; }

        /// <summary>
        /// Gets or sets the scale u.
        /// </summary>
        /// <value>The scale u.</value>
        [DefaultValue(1.0f)]
        [DataMember(80)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float ScaleU { get; set; }

        /// <summary>
        /// Gets or sets the scale v.
        /// </summary>
        /// <value>The scale v.</value>
        [DefaultValue(1.0f)]
        [DataMember(90)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        public float ScaleV { get; set; }
    }

    /// <summary>
    /// Common material attributes.
    /// </summary>
    [DataContract("MaterialAttributes")]
    [Display("Material Attributes")]
    public class MaterialAttributes : IMaterialComposition
    {
        /// <summary>
        /// Gets or sets the tessellation.
        /// </summary>
        /// <value>The tessellation.</value>
        [Display("Tessellation", "Geometry")]
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialTessellationAttribute Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the displacement.
        /// </summary>
        /// <value>The displacement.</value>
        [Display("Displacement", "Geometry")]
        [DefaultValue(null)]
        [DataMember(20)]
        public IMaterialDisplacementAttribute Displacement { get; set; }

        /// <summary>
        /// Gets or sets the surface.
        /// </summary>
        /// <value>The surface.</value>
        [Display("Surface", "Geometry")]
        [DefaultValue(null)]
        [DataMember(30)]
        public IMaterialSurfaceAttribute Surface { get; set; }

        /// <summary>
        /// Gets or sets the micro surface.
        /// </summary>
        /// <value>The micro surface.</value>
        [DefaultValue(null)]
        [DataMember(40)]
        public IMaterialMicroSurfaceAttribute MicroSurface { get; set; }

        /// <summary>
        /// Gets or sets the diffuse.
        /// </summary>
        /// <value>The diffuse.</value>
        [DefaultValue(null)]
        [DataMember(50)]
        public IMaterialDiffuseAttribute Diffuse { get; set; }

        /// <summary>
        /// Gets or sets the diffuse model.
        /// </summary>
        /// <value>The diffuse model.</value>
        [Display("Diffuse Model")]
        [DefaultValue(null)]
        [DataMember(60)]
        public IMaterialDiffuseModelAttribute DiffuseModel { get; set; }

        /// <summary>
        /// Gets or sets the specular.
        /// </summary>
        /// <value>The specular.</value>
        [DefaultValue(null)]
        [DataMember(70)]
        public IMaterialSpecularAttribute Specular { get; set; }

        /// <summary>
        /// Gets or sets the specular model.
        /// </summary>
        /// <value>The specular model.</value>
        [Display("Specular Model")]
        [DefaultValue(null)]
        [DataMember(80)]
        public IMaterialSpecularModelAttribute SpecularModel { get; set; }

        /// <summary>
        /// Gets or sets the occlusion.
        /// </summary>
        /// <value>The occlusion.</value>
        [DefaultValue(null)]
        [DataMember(90)]
        public IMaterialOcclusionAttribute Occlusion { get; set; }

        /// <summary>
        /// Gets or sets the emissive.
        /// </summary>
        /// <value>The emissive.</value>
        [DefaultValue(null)]
        [DataMember(100)]
        public IMaterialEmissiveAttribute Emissive { get; set; }

        /// <summary>
        /// Gets or sets the transparency.
        /// </summary>
        /// <value>The transparency.</value>
        [DefaultValue(null)]
        [DataMember(110)]
        public IMaterialTransparencyAttribute Transparency { get; set; }
    }

    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset2")]
    [Display("Material Asset", "A material asset")]
    public class MaterialAsset2 : Asset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAsset2"/> class.
        /// </summary>
        public MaterialAsset2()
        {
            Parameters = new ParameterCollectionData();
        }
        /// <summary>
        /// Gets or sets the material composition.
        /// </summary>
        /// <value>The material composition.</value>
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialComposition Composition { get; set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMember(20)]
        public ParameterCollectionData Parameters { get; private set; }
    }
}