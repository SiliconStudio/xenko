// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    // TODO: Move all these interfaces/classes to single file once the design is stabilized

    /// <summary>
    /// Base interface for a material feature.
    /// </summary>
    public interface IMaterialFeature : IMaterialShaderGenerator
    {
    }

    /// <summary>
    /// Base interface for a tessellation material feature.
    /// </summary>
    public interface IMaterialTessellationFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for a displacement material feature.
    /// </summary>
    public interface IMaterialDisplacementFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the surface material feature (normals...etc.)
    /// </summary>
    public interface IMaterialSurfaceFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for a diffuse material feature.
    /// </summary>
    public interface IMaterialDiffuseFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for a specular material feature.
    /// </summary>
    public interface IMaterialSpecularFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for a micro-surface material feature.
    /// </summary>
    public interface IMaterialMicroSurfaceFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the diffuse model material feature.
    /// </summary>
    public interface IMaterialDiffuseModelFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the specular model material feature.
    /// </summary>
    public interface IMaterialSpecularModelFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the occlusion material feature.
    /// </summary>
    public interface IMaterialOcclusionFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the emissive material feature.
    /// </summary>
    public interface IMaterialEmissiveFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// Base interface for the transparency material feature.
    /// </summary>
    public interface IMaterialTransparencyFeature : IMaterialFeature
    {
    }

    /// <summary>
    /// A material composition.
    /// </summary>
    public interface IMaterialComposition : IMaterialFeature
    {
    }

    /// <summary>
    /// Type of a stream used by <see cref="MaterialStreamAttribute"/>
    /// </summary>
    public enum MaterialStreamType
    {
        Float3,

        Float
    }

    /// <summary>
    /// An attribute used to identify associate a shader stream with a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MaterialStreamAttribute : Attribute
    {
        private readonly string stream;
        private readonly MaterialStreamType type;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialStreamAttribute"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="type">The type.</param>
        public MaterialStreamAttribute(string stream, MaterialStreamType type)
        {
            this.stream = stream;
            this.type = type;
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <value>The stream.</value>
        public string Stream
        {
            get
            {
                return stream;
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public MaterialStreamType Type
        {
            get
            {
                return type;
            }
        }
    }

    /// <summary>
    /// Base class for <see cref="IMaterialFeature"/>.
    /// </summary>
    /// <remarks>
    /// This base class automatically iterates on properties to generate the shader
    /// </remarks>
    public abstract class MaterialFeatureBase : IMaterialFeature
    {
        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            var typeDescriptor = TypeDescriptorFactory.Default.Find(this.GetType());

            foreach (var member in typeDescriptor.Members.OfType<MemberDescriptorBase>())
            {
                var memberValue = member.Get(this);

                // TODO: Should we log an error if a property/field is not supported?
                // TODO: Handle list/collection of IMaterialFeature?

                var memberShaderGen = memberValue as IMaterialFeature;
                if (memberShaderGen != null)
                {
                    memberShaderGen.GenerateShader(context);
                }
                else
                {
                    var materialStreamAttribute = member.MemberInfo.GetCustomAttribute<MaterialStreamAttribute>();
                    if (materialStreamAttribute != null)
                    {
                        if (string.IsNullOrWhiteSpace(materialStreamAttribute.Stream))
                        {
                            context.Log.Error("Material stream cannot be null for member [{0}.{1}]", member.DeclaringType, member.MemberInfo.Name);
                            continue;
                        }

                        var materialNode = memberValue as IMaterialNode;
                        if (materialNode != null)
                        {
                            var classSource = materialNode.GenerateShaderSource(context);
                            switch (materialStreamAttribute.Type)
                            {
                                case MaterialStreamType.Float3:
                                    context.CurrentStack.AddBlendColor3(materialStreamAttribute.Stream, classSource);
                                    break;

                                case MaterialStreamType.Float:
                                    context.CurrentStack.AddBlendColor(materialStreamAttribute.Stream, classSource);
                                    break;
                            }
                        }
                        else
                        {
                            context.Log.Error("Error in [{0}.{1}] support only IMaterialNode instead of [{2}]", member.DeclaringType, member.MemberInfo.Name, member.Type);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A smoothness map for the micro-surface material feature.
    /// </summary>
    [DataContract("MaterialSmoothnessMapFeature")]
    [Display("Smoothness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSmoothnessMapFeature : MaterialFeatureBase, IMaterialMicroSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Smoothness Map")]
        [DefaultValue(null)]
        [MaterialStreamAttribute("matSmoothness", MaterialStreamType.Float)]
        public IMaterialNode SmoothnessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSmoothnessMapFeature() { SmoothnessMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A Diffuse map for the diffuse material feature.
    /// </summary>
    [DataContract("MaterialDiffuseMapFeature")]
    [Display("Diffuse Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialDiffuseMapFeature : MaterialFeatureBase, IMaterialDiffuseFeature
    {
        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Diffuse Map")]
        [DefaultValue(null)]
        [MaterialStreamAttribute("matDiffuse", MaterialStreamType.Float3)]
        public IMaterialNode DiffuseMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialDiffuseMapFeature() { DiffuseMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A Specular map for the specular material feature.
    /// </summary>
    [DataContract("MaterialSpecularMapFeature")]
    [Display("Specular Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSpecularMapFeature : MaterialFeatureBase, IMaterialSpecularFeature
    {
        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [Display("Specular Map")]
        [DefaultValue(null)]
        [MaterialStreamAttribute("matSpecular", MaterialStreamType.Float3)]
        public IMaterialNode SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(null)]
        [MaterialStreamAttribute("matSpecularIntensity", MaterialStreamType.Float)]
        public IMaterialNode Intensity { get; set; }

        /// <summary>
        /// Gets or sets the fresnel.
        /// </summary>
        /// <value>The fresnel.</value>
        [DefaultValue(null)]
        [MaterialStreamAttribute("matSpecularFresnel", MaterialStreamType.Float)]
        public IMaterialNode Fresnel { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSpecularMapFeature()
                {
                    SpecularMap = new MaterialTextureNode(),
                    Intensity = new MaterialFloatNode(1.0f),
                    Fresnel = new MaterialFloatNode(1.0f),
                };
            }
        }
    }

    /// <summary>
    /// A Metalness map for the specular material feature.
    /// </summary>
    [DataContract("MaterialMetalnessMapFeature")]
    [Display("Metalness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialMetalnessMapFeature : MaterialFeatureBase, IMaterialSpecularFeature
    {
        /// <summary>
        /// Gets or sets the metalness map.
        /// </summary>
        /// <value>The metalness map.</value>
        [Display("Metalness Map")]
        [DefaultValue(null)]
        [MaterialStreamAttribute("matMetalnessMap", MaterialStreamType.Float)]
        public IMaterialNode MetalnessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialMetalnessMapFeature() { MetalnessMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// An occlusion map for the occlusion material feature.
    /// </summary>
    [DataContract("MaterialOcclusionMapFeature")]
    [Display("Occlusion Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialOcclusionMapFeature : MaterialFeatureBase, IMaterialOcclusionFeature
    {
        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DefaultValue(null)]
        [DataMember(10)]
        [MaterialStreamAttribute("matAmbientOcclusion", MaterialStreamType.Float)]
        public IMaterialNode AmbientOcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DefaultValue(null)]
        [DataMember(20)]
        [MaterialStreamAttribute("matCavity", MaterialStreamType.Float)]
        public IMaterialNode CavityMap { get; set; }

        /// <summary>
        /// Gets or sets the diffuse cavity influence.
        /// </summary>
        /// <value>The diffuse cavity.</value>
        [Display("Diffuse Cavity")]
        [DefaultValue(null)]
        [DataMember(30)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        [MaterialStreamAttribute("matCavityDiffuse", MaterialStreamType.Float)]
        public IMaterialNode DiffuseCavity { get; set; }

        /// <summary>
        /// Gets or sets the specular cavity.
        /// </summary>
        /// <value>The specular cavity.</value>
        [Display("Specular Cavity")]
        [DefaultValue(null)]
        [DataMember(40)]
        [DataRangeAttribute(0.0f, 1.0f, 0.01f)]
        [MaterialStreamAttribute("matCavitySpecular", MaterialStreamType.Float)]
        public IMaterialNode SpecularCavity { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialOcclusionMapFeature()
                {
                    AmbientOcclusionMap = new MaterialTextureNode(),
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
    [DataContract("MaterialDiffuseLambertianModelFeature")]
    [Display("Lamtertian")]
    public class MaterialDiffuseLambertianModelFeature : IMaterialDiffuseModelFeature
    {
        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// The normal map for a surface material feature.
    /// </summary>
    [DataContract("MaterialNormalMapFeature")]
    [Display("Normal Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialNormalMapFeature : MaterialFeatureBase, IMaterialSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        /// <value>The normal map.</value>
        [Display("Normal Map")]
        [DefaultValue(null)]
        [MaterialStreamAttribute("matNormal", MaterialStreamType.Float3)]
        public IMaterialNode NormalMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialNormalMapFeature() { NormalMap = new MaterialTextureNode() };
            }
        }
    }

    /// <summary>
    /// A composition material to blend different materials.
    /// </summary>
    [DataContract("MaterialBlendLayerStack")]
    [Display("Material Layers")]
    [ObjectFactory(typeof(Factory))]
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

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                var stack = new MaterialBlendLayerStack();
                stack.Layers.Add(ObjectFactory.NewInstance<MaterialBlendLayer>());
                return stack;
            }
        }

        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            foreach (var layer in Layers)
            {
                layer.GenerateShader(context);
            }
        }
    }

    /// <summary>
    /// A material blend layer
    /// </summary>
    [DataContract("MaterialBlendLayer")]
    [Display("Material Layer")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialBlendLayer : IMaterialShaderGenerator
    {
        private const string BlendStream = "matBlend";

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
        public AssetReference<MaterialAsset> Material { get; set; }

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

        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            if (!Enabled || Material == null)
            {
                return;
            }

            var material = context.FindMaterial(Material);
            if (material == null)
            {
                context.Log.Error("Unable to find material [{0}]", Material);
                return;
            }

            var stack = context.PushStack();
            material.GenerateShader(context);

            // Backup stream variables that will be modified by the materials
            var backupStreamBuilder = new StringBuilder();
            foreach (var stream in stack.Streams)
            {
                backupStreamBuilder.AppendFormat("        var __backup__{0} = streams.{0};", stream).AppendLine();
            }

            // Blend stream variables modified by the material with the previous backup
            var copyFromLayerBuilder = new StringBuilder();
            foreach (var stream in stack.Streams)
            {
                copyFromLayerBuilder.AppendFormat("        streams.{0} = lerp(__backup__{0}, streams.{0}, streams.matBlend;", stream).AppendLine();
            }

            // Generate a dynamic shader
            var shaderName = string.Format("MaterialBlendLayer{0}", context.NextId());
            var shaderClassSource = new ShaderClassSource(shaderName)
            {
                Inline = string.Format(DynamicBlendingShader, shaderName, backupStreamBuilder, copyFromLayerBuilder)
            };

            // Blend setup
            var blendSetup = new ShaderMixinSource();
            blendSetup.Mixins.Add(new ShaderClassSource("MaterialLayerComputeColorInit", BlendStream, "r"));
            var blendMapSource = BlendMap.GenerateShaderSource(context);
            blendSetup.AddComposition("Source", blendMapSource);

            // Create a mixin
            var shaderMixinSource = new ShaderMixinSource();
            shaderMixinSource.Mixins.Add(shaderClassSource);
            stack.Operations.Add(blendSetup);
            var materialOperations = stack.SquashOperations();
            shaderMixinSource.AddComposition("subLayer", materialOperations);
            context.PopStack();

            context.CurrentStack.Operations.Add(shaderMixinSource);
        }

        private const string DynamicBlendingShader = @"
class {0} : IMaterialLayer
{{
    compose IMaterialLayer subLayer;

    override void Compute()
    {{
{1}        subLayer.Compute();
{2}}}
}};
";
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
    [DataContract("MaterialFeatures")]
    [Display("Material Features")]
    public class MaterialFeatures : MaterialFeatureBase, IMaterialComposition
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
    }

    /// <summary>
    /// The material asset.
    /// </summary>
    [DataContract("MaterialAsset2")]
    [AssetFileExtension(FileExtension)]
    [ThumbnailCompiler(PreviewerCompilerNames.MaterialThumbnailCompilerQualifiedName, true)]
    [AssetCompiler(typeof(MaterialAssetCompiler))]
    [ObjectFactory(typeof(MaterialFactory))]
    [Display("Material", "A material")]
    public class MaterialAsset : Asset, IMaterialShaderGenerator
    {
        /// <summary>
        /// The default file extension used by the <see cref="MaterialAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxmat";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialAsset"/> class.
        /// </summary>
        public MaterialAsset()
        {
            Parameters = new ParameterCollectionData();
            Overrides = new Dictionary<string, IMaterialNode>();
        }
        /// <summary>
        /// Gets or sets the material composition.
        /// </summary>
        /// <value>The material composition.</value>
        [DefaultValue(null)]
        [DataMember(10)]
        public IMaterialComposition Composition { get; set; }

        /// <summary>
        /// XXXX
        /// </summary>
        /// <userdoc>
        /// All the color mapping nodes of the materials. They are map descriptions (texture or values) and operations on them.
        /// </userdoc>
        [DataMember(20)]
        public Dictionary<string, IMaterialNode> Overrides { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMember(30)]
        public ParameterCollectionData Parameters { get; private set; }

        private class MaterialFactory : IObjectFactory
        {
            public object New(Type type)
            {
                var newMaterial = new MaterialAsset { Composition = new MaterialFeatures() };
                return newMaterial;
            }
        }

        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            if (Composition != null)
            {
                Composition.GenerateShader(context);
            }
        }
    }
}