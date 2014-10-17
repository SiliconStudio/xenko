// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization
{
    public class EmptyDataSerializer<T> : DataSerializer<T>
    {
        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
        }
    }
}