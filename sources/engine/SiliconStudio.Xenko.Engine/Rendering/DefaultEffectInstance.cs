// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// An effect instance using a set of <see cref="ParameterCollection"/> for creating <see cref="Effect"/>.
    /// </summary>
    public class DefaultEffectInstance : DynamicEffectInstanceOld
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

        public override void FillParameterCollections(ref FastListStruct<ParameterCollection> parameterCollections)
        {
            // Test common types to avoid struct enumerator boxing
            var localParameterCollectionsList = localParameterCollections as List<ParameterCollection>;
            if (localParameterCollectionsList != null)
            {
                foreach (var parameter in localParameterCollectionsList)
                {
                    if (parameter != null)
                    {
                        parameterCollections.Add(parameter);
                    }
                }
            }
            else
            {
                var localParameterCollectionsArray = localParameterCollections as ParameterCollection[];
                if (localParameterCollectionsArray != null)
                {
                    foreach (var parameter in localParameterCollectionsArray)
                    {
                        if (parameter != null)
                        {
                            parameterCollections.Add(parameter);
                        }
                    }
                }
                else
                {
                    // Slow: enumerator will be boxed
                    foreach (var parameter in localParameterCollections)
                    {
                        if (parameter != null)
                        {
                            parameterCollections.Add(parameter);
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}