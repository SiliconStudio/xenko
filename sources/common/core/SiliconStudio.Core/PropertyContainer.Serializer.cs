// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core
{
    public partial struct PropertyContainer
    {
        /// <summary>
        /// Serializer for the PropertyContainer
        /// </summary>
        internal class Serializer : DataSerializer<PropertyContainer>
        {
            public override void Serialize(ref PropertyContainer propertyCollection, ArchiveMode mode, SerializationStream stream)
            {
                stream.SerializeExtended(ref propertyCollection.properties, mode);
            }
        }
    }
}