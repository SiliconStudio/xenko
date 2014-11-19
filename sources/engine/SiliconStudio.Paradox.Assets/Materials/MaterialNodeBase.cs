// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base implementation for <see cref="IMaterialNode"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class MaterialNodeBase : IMaterialNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeBase"/> class.
        /// </summary>
        protected MaterialNodeBase()
        {
            IsReducible = true;
        }

        /// <summary>
        /// The flag to allow the node to be reducible.
        /// </summary>
        /// <userdoc>
        /// If checked, the material will try to merge this node with its children and parents. As a consequence, parameters made runtime-editable (through the use of custom ParameterKey) might no longer be.
        /// </userdoc>
        [DataMember(1000)]  
        [DefaultValue(true)]
        public bool IsReducible { get; set; }

        /// <inheritdoc/>
        public virtual IEnumerable<MaterialNodeEntry> GetChildren(object context = null)
        {
            return Enumerable.Empty<MaterialNodeEntry>();
        }
    }
}