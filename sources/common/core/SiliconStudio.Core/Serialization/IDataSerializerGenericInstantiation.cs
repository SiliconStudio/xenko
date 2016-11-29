// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Allows enumeration of required data serializers.
    /// </summary>
    public interface IDataSerializerGenericInstantiation
    {
        /// <summary>
        /// Enumerates required <see cref="DataSerializer"/> required by this instance of DataSerializer.
        /// </summary>
        /// <remarks>
        /// The code won't be executed, it will only be scanned for typeof() operands by the assembly processor.
        /// Null is authorized in enumeration (for now).
        /// </remarks>
        /// <param name="serializerSelector"></param>
        /// <param name="genericInstantiations"></param>
        /// <returns></returns>
        void EnumerateGenericInstantiations(SerializerSelector serializerSelector, IList<Type> genericInstantiations);
    }
}