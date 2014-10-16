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
    public abstract class MaterialNodeBase : IMaterialNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeBase"/> class.
        /// </summary>
        protected MaterialNodeBase()
        {
            IsReducible = true;
        }

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