// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Serialization
{
    public class EmptyDataSerializer<T> : DataSerializer<T>
    {
        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
        }
    }
}
