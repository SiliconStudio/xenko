// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Serialization
{
    public abstract class ClassDataSerializer<T> : DataSerializer<T> where T : class, new()
    {
        /// <inheritdoc/>
        public override void PreSerialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize && obj == null)
                obj = new T();
        }
    }
}
