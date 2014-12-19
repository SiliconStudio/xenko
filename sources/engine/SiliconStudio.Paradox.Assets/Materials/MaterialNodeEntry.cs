// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An entry to a nested <see cref="IMaterialComputeColor"/>
    /// </summary>
    public struct MaterialNodeEntry
    {
        private readonly IMaterialComputeColor computeColor;
        private readonly Action<IMaterialComputeColor> setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeEntry"/> struct.
        /// </summary>
        /// <param name="computeColor">The node.</param>
        /// <param name="setter">The setter.</param>
        /// <exception cref="System.ArgumentNullException">setter</exception>
        public MaterialNodeEntry(IMaterialComputeColor computeColor, Action<IMaterialComputeColor> setter  )
        {
            if (setter == null) throw new ArgumentNullException("setter");
            this.computeColor = computeColor;
            this.setter = setter;
        }

        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        public IMaterialComputeColor ComputeColor
        {
            get
            {
                return computeColor;
            }
            set
            {
                setter(value);
            }
        }
    }
}