// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Used as a fallback when <see cref="SerializerSelector.GetSerializer"/> didn't find anything.
    /// </summary>
    public abstract class SerializerFactory
    {
        public abstract DataSerializer GetSerializer(SerializerSelector selector, ref ObjectId typeId);
        public abstract DataSerializer GetSerializer(SerializerSelector selector, Type type);
    }
}
