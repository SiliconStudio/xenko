// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An entry to a nested <see cref="IMaterialNode"/>
    /// </summary>
    public struct MaterialNodeEntry
    {
        private readonly IMaterialNode node;
        private readonly Action<IMaterialNode> setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeEntry"/> struct.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="setter">The setter.</param>
        /// <exception cref="System.ArgumentNullException">setter</exception>
        public MaterialNodeEntry(IMaterialNode node, Action<IMaterialNode> setter  )
        {
            if (setter == null) throw new ArgumentNullException("setter");
            this.node = node;
            this.setter = setter;
        }

        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        public IMaterialNode Node
        {
            get
            {
                return node;
            }
            set
            {
                setter(value);
            }
        }
    }
}