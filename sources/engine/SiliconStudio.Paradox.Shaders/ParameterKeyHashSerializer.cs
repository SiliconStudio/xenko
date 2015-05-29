// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Shaders
{
    class ParameterKeyHashSerializer : DataSerializer<ParameterKey>
    {
        public unsafe override void Serialize(ref ParameterKey obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode != ArchiveMode.Serialize)
                throw new InvalidOperationException();

            // Just use parameter key hash code
            // Hopefully there won't be any clash...
            fixed (ulong* objId = &obj.HashCode)
            {
                stream.Serialize(ref *objId);
            }
        }
    }
}