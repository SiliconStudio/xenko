// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Use this attribute on a class to specify its data serializer type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DataSerializerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializerAttribute"/> class.
        /// </summary>
        /// <param name="dataSerializerType">Type of the data serializer.</param>
        public DataSerializerAttribute([NotNull] Type dataSerializerType)
        {
            DataSerializerType = dataSerializerType;
        }

        /// <summary>
        /// Gets the type of the data serializer.
        /// </summary>
        /// <value>
        /// The type of the data serializer.
        /// </value>
        [NotNull]
        public Type DataSerializerType;

        public DataSerializerGenericMode Mode;
    }
}
