// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A composition material to blend different materials in a stack based manner.
    /// </summary>
    [DataContract("MaterialBlendLayers")]
    [Display("Material Layers")]
    public class MaterialBlendLayers : List<MaterialBlendLayer>, IMaterialLayers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayers"/> class.
        /// </summary>
        public MaterialBlendLayers()
        {
            Enabled = true;
        }

        [DataMemberIgnore]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        public virtual void Visit(MaterialGeneratorContext context)
        {
            if (!Enabled)
                return;

            foreach (var layer in this)
            {
                layer.Visit(context);
            }
        }
    }
}
