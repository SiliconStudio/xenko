// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// A null serializer that can be used to add dummy serialization attributes.
    /// </summary>
    public class NullSerializer<T> : DataSerializer<T>
    {
        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
