// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Graphics.Font;

namespace SiliconStudio.Paradox.Graphics
{
    internal class StaticSpriteFontDataConverter : DataConverter<StaticSpriteFontData, SpriteFont>
    {
        public override void ConvertFromData(ConverterContext converterContext, StaticSpriteFontData data, ref SpriteFont obj)
        {
            var services = converterContext.Tags.Get(ServiceRegistry.ServiceRegistryKey);
            var fontSystem = services.GetSafeServiceAs<GameFontSystem>().FontSystem;

            var staticSpriteFont = new StaticSpriteFont(fontSystem, data);

            for (int index = 0; index < data.Bitmaps.Length; index++)
            {
                var bitmap = data.Bitmaps[index];

                // Convert to texture ref so that Converter system doesn't get lost
                // TODO: Support that directly in converter?
                var textureRef = new ContentReference<Texture> { Location = bitmap.Location };
                Texture texture = null;
                converterContext.ConvertFromData(textureRef, ref texture);
                staticSpriteFont.StaticTextures[index] = texture;
            }

            obj = staticSpriteFont;
        }

        public override void ConvertToData(ConverterContext converterContext, ref StaticSpriteFontData data, SpriteFont obj)
        {
            throw new System.NotImplementedException();
        }

        [ModuleInitializer]
        public static void Initialize()
        {
            ConverterContext.RegisterConverter(new StaticSpriteFontDataConverter());
        }
    }
}