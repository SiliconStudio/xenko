// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Graphics.Font;

namespace SiliconStudio.Paradox.Graphics
{
    internal class DynamicSpriteFontDataConverter : DataSerializer<SpriteFont>
    {
        public override void Serialize(ref SpriteFont obj, ArchiveMode mode, SerializationStream stream)
        {
            throw new System.NotImplementedException();
        }

        //public override void ConvertFromData(ConverterContext converterContext, DynamicSpriteFontData data, ref SpriteFont obj)
        //{
        //    var services = converterContext.Tags.Get(ServiceRegistry.ServiceRegistryKey);
        //    var fontSystem = services.GetSafeServiceAs<GameFontSystem>().FontSystem;
        //    obj = new DynamicSpriteFont(fontSystem, data);
        //}
        //
        //public override void ConvertToData(ConverterContext converterContext, ref DynamicSpriteFontData data, SpriteFont obj)
        //{
        //    throw new System.NotImplementedException();
        //}
        //
        //[ModuleInitializer]
        //public static void Initialize()
        //{
        //    ConverterContext.RegisterConverter(new DynamicSpriteFontDataConverter());
        //}
    }
}