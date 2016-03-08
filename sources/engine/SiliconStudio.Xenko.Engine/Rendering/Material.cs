// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A compiled version of <see cref="MaterialDescriptor"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<Material>), Profile = "Content")]
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
        /// Overrides the cullmode for this material.
        /// </summary>
        public CullMode? CullMode;

        /// <summary>
        /// The tessellation method used by the material.
        /// </summary>
        public XenkoTessellationMethod TessellationMethod;

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
        /// Determines if this material is affected by lighting.
        /// </summary>
        /// <value><c>true</c> if this instance affects lighting; otherwise, <c>false</c>.</value>
        public bool IsLightDependent { get; set; }

        /// <summary>
        /// Creates a new material from the specified descriptor.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="descriptor">The material descriptor.</param>
        /// <returns>An instance of a <see cref="Material"/>.</returns>
        /// <exception cref="System.ArgumentNullException">descriptor</exception>
        /// <exception cref="System.InvalidOperationException">If an error occurs with the material description</exception>
        public static Material New(GraphicsDevice device, MaterialDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            var context = new MaterialGeneratorContext(new Material());
            var result = MaterialGenerator.Generate(descriptor, context, string.Format("{0}:RuntimeMaterial", descriptor.MaterialId));

            if (result.HasErrors)
            {
                throw new InvalidOperationException(string.Format("Error when creating the material [{0}]", result.ToText()));
            }

            var material = result.Material;
            // TODO GRAPHICS REFACTOR
            //var blendState = material.Parameters.GetResourceSlow(Graphics.Effect.BlendStateKey);
            //if (blendState != null && blendState.GraphicsDevice == null)
            //{
            //    var newState = BlendState.New(device, blendState.Description);
            //    material.Parameters.SetResourceSlow(Effect.BlendStateKey, newState);
            //}
            // TODO: Add other states?

            return material;
        }
    }
}
