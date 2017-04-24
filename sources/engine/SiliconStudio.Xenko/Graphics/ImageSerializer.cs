// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics
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
                textureData.Save(stream.NativeStream, ImageFileType.Xenko);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return new Image();
        }
    }
}
