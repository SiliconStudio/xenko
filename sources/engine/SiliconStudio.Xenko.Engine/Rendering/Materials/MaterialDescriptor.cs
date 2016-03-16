// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A descriptor of a <see cref="Material"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(ReferenceSerializer<MaterialDescriptor>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<MaterialDescriptor>))]
    [DataContract("MaterialDescriptor")]
    public class MaterialDescriptor : IMaterialDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDescriptor"/> class.
        /// </summary>
        public MaterialDescriptor()
        {
            Attributes = new MaterialAttributes();
            Layers = new MaterialBlendLayers();
            // An instance id, only used to match descriptor
            MaterialId = Guid.NewGuid();
        }

        [DataMemberIgnore]
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Gets or sets the material attributes.
        /// </summary>
        /// <value>The material attributes.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Attributes", Expand = ExpandRule.Always)]
        public MaterialAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the material compositor.
        /// </summary>
        /// <value>The material compositor.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        [NotNull]
        [NotNullItems]
        public MaterialBlendLayers Layers { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            if (Attributes != null)
            {
                Attributes.Visit(context);
            }

            if (Layers != null)
            {
                Layers.Visit(context);
            }
        }
    }
}