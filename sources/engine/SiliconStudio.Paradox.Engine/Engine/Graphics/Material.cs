// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Engine.Graphics.Materials;

namespace SiliconStudio.Paradox.Effects
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
        /// Creates a new material from the specified descriptor.
        /// </summary>
        /// <param name="descriptor">The material descriptor.</param>
        /// <returns>An instance of a <see cref="Material"/>.</returns>
        /// <exception cref="System.ArgumentNullException">descriptor</exception>
        /// <exception cref="System.InvalidOperationException">If an error occurs with the material description</exception>
        public static Material New(MaterialDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            var context = new MaterialGeneratorContext();
            var result = MaterialGenerator.Generate(descriptor, context);

            if (result.HasErrors)
            {
                throw new InvalidOperationException(string.Format("Error when creating the material [{0}]", result.ToText()));
            }

            return new Material(result.Parameters);
        }
    }
}
