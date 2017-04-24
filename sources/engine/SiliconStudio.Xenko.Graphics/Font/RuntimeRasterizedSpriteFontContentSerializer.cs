// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics.Font
{
    internal class RuntimeRasterizedSpriteFontContentSerializer : DataContentSerializer<RuntimeRasterizedSpriteFont>
    {
        public override object Construct(ContentSerializerContext context)
        {
            return new RuntimeRasterizedSpriteFont();
        }
    }
}
