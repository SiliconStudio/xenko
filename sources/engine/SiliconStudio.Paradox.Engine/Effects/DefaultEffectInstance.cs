// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// An effect instance using a set of <see cref="ParameterCollection"/> for creating <see cref="Effect"/>.
    /// </summary>
    public class DefaultEffectInstance : DynamicEffectInstance
    {
        private readonly IEnumerable<ParameterCollection> localParameterCollections;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEffectInstance"/> class.
        /// </summary>
        /// <param name="parameterCollections">The parameter collections.</param>
        public DefaultEffectInstance(params ParameterCollection[] parameterCollections)
        {
            this.localParameterCollections = parameterCollections;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEffectInstance"/> class.
        /// </summary>
        /// <param name="parameterCollections">The parameter collections.</param>
        public DefaultEffectInstance(IEnumerable<ParameterCollection> parameterCollections)
        {
            this.localParameterCollections = parameterCollections;
        }

        public override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
        {
            foreach (var parameter in this.localParameterCollections)
            {
                if (parameter != null)
                {
                    parameterCollections.Add(parameter);
                }
            }
        }
    }
}