// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Declares a serializer like <see cref="DataSerializerAttribute"/> or <see cref="DataContractAttribute"/>, but externally.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class DataSerializerGlobalAttribute : Attribute
    {
        /// <summary>
        /// Declares a data serializer, either by its serializer type (it will act like <see cref="DataSerializerAttribute"/> and guess data type from the generic type of <see cref="DataSerializer{T}"/>) or the data type (it will act just like if <see cref="DataContractAttribute"/> was set on the data type).
        /// </summary>
        /// <param name="serializerType">The serializer type. Can be null if <paramref name="dataType"/> if set.</param>
        /// <param name="dataType">The data type. Can be null if <paramref name="serializerType"/> is set.</param>
        /// <param name="mode">Defines how generic type are added to <paramref name="serializerType"/>.</param>
        /// <param name="inherited">Similar to <see cref="DataContractAttribute.Inherited"/></param>
        /// <param name="complexSerializer"></param>
        public DataSerializerGlobalAttribute(Type serializerType, Type dataType = null, DataSerializerGenericMode mode = DataSerializerGenericMode.None, bool inherited = false, bool complexSerializer = false)
        {
            
        }

        public string Profile { get; set; }
    }
}
