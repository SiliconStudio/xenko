// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A compiled version of <see cref="MaterialDescriptor"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<Material>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<Material>))]
    [DataContract]
    public class Material
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        public Material()
        {
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public Material(ParameterCollection parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// The tessellation method used by the material.
        /// </summary>
        public ParadoxTessellationMethod TessellationMethod;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has transparent.
        /// </summary>
        /// <value><c>true</c> if this instance has transparent; otherwise, <c>false</c>.</value>
        public bool HasTransparency { get; set; }

        /// <summary>
        /// Gets or sets the descriptor (this field is null at runtime).
        /// </summary>
        /// <value>The descriptor.</value>
        [DataMemberIgnore]
        public MaterialDescriptor Descriptor { get; set; }

        /// <summary>
        /// Creates a new material from the specified descriptor.
        /// </summary>
        /// <param name="descriptor">The material descriptor.</param>
        /// <returns>An instance of a <see cref="Material"/>.</returns>
        /// <exception cref="System.ArgumentNullException">descriptor</exception>
        /// <exception cref="System.InvalidOperationException">If an error occurs with the material description</exception>
        public static Material New(MaterialDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            var context = new MaterialGeneratorContext(new Material());
            var result = MaterialGenerator.Generate(descriptor, context);

            if (result.HasErrors)
            {
                throw new InvalidOperationException(string.Format("Error when creating the material [{0}]", result.ToText()));
            }

            return result.Material;
        }

        public static Material NewDiffuseOnly(Texture diffuseTexture)
        {
            return New(
                new MaterialDescriptor()
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor(diffuseTexture)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    }
                });
        }

        public static Material NewDiffuseAndMetal(Texture diffuseTexture, float metalness = 0.0f, float glossiness = 0.0f)
        {
            return New(
                new MaterialDescriptor()
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor(diffuseTexture)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(glossiness)),
                        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(metalness)),
                        SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                    }
                });
        }

        public static Material NewDiffuseOnly(Color4 diffuseColor)
        {
            return New(
                new MaterialDescriptor()
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(diffuseColor)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    }
                });
        }

        public static Material NewDiffuseAndMetal(Color4 diffuseColor, float metalness = 0.0f, float glossiness = 0.0f)
        {
            return New(
                new MaterialDescriptor()
                {
                    Attributes =
                    {
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(diffuseColor)),
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                        MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(glossiness)),
                        Specular = new MaterialMetalnessMapFeature(new ComputeFloat(metalness)),
                        SpecularModel = new MaterialSpecularMicrofacetModelFeature()
                    }
                });
        }
    }
}
