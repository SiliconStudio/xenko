// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class DataSerializerGlobalAttribute : Attribute
    {
        public DataSerializerGlobalAttribute(Type serializerType, Type dataType = null, DataSerializerGenericMode mode = DataSerializerGenericMode.None, bool inherited = false, bool complexSerializer = false)
        {
            
        }

        public string Profile { get; set; }
    }
}