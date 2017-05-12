// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Graphics.Data
{
    /// <summary>
    /// Serializer for <see cref="Texture"/>.
    /// </summary>
    public class TextureSerializer : DataSerializer<Texture>
    {
        /// <inheritdoc/>
        public override void PreSerialize(ref Texture texture, ArchiveMode mode, SerializationStream stream)
        {
            // Do not create object during preserialize (OK because not recursive)
        }

        /// <inheritdoc/>
        public override void Serialize(ref Texture texture, ArchiveMode mode, SerializationStream stream)
        {
            TextureContentSerializer.Serialize(mode, stream, texture);
        }
    }
}
