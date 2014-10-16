// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Assets.Effect.ValueGenerators;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// A set of permutation for a specific effect.
    /// </summary>
    [DataContract("!fx.permutation.group")]
    public class EffectPermutation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectPermutation"/> class.
        /// </summary>
        public EffectPermutation()
        {
            Keys = new EffectParameterKeyStandardGenerator();
            Children = new List<EffectPermutation>();
        }

        /// <summary>
        /// Gets or sets the common permutation generator for the specified <see cref="Effect"/>
        /// </summary>
        /// <value>The keys.</value>
        [DataMember(10)]
        public EffectParameterKeyStandardGenerator Keys { get; set; }

        /// <summary>
        /// Gets or sets the permutation generators for the specified <see cref="Effect"/>
        /// </summary>
        /// <value>The permutations.</value>
        [DataMember(20)]
        public List<EffectPermutation> Children { get; set; }
    }
}