// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Rendering.ProceduralModels;

namespace SiliconStudio.Paradox.Rendering.ProceduralModels
{
    /// <summary>
    /// A descriptor for a procedural geometry.
    /// </summary>
    [DataContract("ProceduralModelDescriptor")]
    [ContentSerializer(typeof(ProceduralModelDescriptorContentSerializer))]
    [ContentSerializer(typeof(DataContentSerializer<ProceduralModelDescriptor>))]    
    public class ProceduralModelDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProceduralModelDescriptor"/> class.
        /// </summary>
        public ProceduralModelDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProceduralModelDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ProceduralModelDescriptor(IProceduralModel type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the type of geometric primitive.
        /// </summary>
        /// <value>The type of geometric primitive.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Type", AlwaysExpand = true)]
        public IProceduralModel Type { get; set; }

        public Model GenerateModel(IServiceRegistry services)
        {
            var model = new Model();
            GenerateModel(services, model);
            return model;
        }

        public void GenerateModel(IServiceRegistry services, Model model)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (model == null) throw new ArgumentNullException("model");

            if (Type == null)
            {
                throw new InvalidOperationException("Invalid GeometricPrimitive [{0}]. Expecting a non-null Type");
            }

            Type.Generate(services, model);
        }
    }
}