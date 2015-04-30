// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    internal class ImageSerializer : ContentSerializerBase<Image>
    {
        public override void Serialize(ContentSerializerContext context, SerializationStream stream, Image textureData)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                textureData = Image.Load(stream.NativeStream);
            }
            else
            {
                textureData.Save(stream.NativeStream, ImageFileType.Paradox);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return null;
        }
    }
}