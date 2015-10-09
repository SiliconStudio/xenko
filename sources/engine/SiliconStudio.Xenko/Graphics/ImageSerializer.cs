// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
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
                var image = Image.Load(stream.NativeStream);
                textureData.InitializeFrom(image);
            }
            else
            {
                textureData.Save(stream.NativeStream, ImageFileType.Paradox);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return new Image();
        }
    }
}