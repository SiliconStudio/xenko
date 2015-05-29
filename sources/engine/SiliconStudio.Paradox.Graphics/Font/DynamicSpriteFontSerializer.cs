// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics.Font
{
    /// <summary>
    /// Serializer for <see cref="DynamicSpriteFont"/>.
    /// </summary>
    internal class DynamicSpriteFontSerializer : DataSerializer<DynamicSpriteFont>, IDataSerializerInitializer
    {
        private DataSerializer<SpriteFont> parentSerializer;

        public override void PreSerialize(ref DynamicSpriteFont texture, ArchiveMode mode, SerializationStream stream)
        {
            // Do not create object during pre-serialize (OK because not recursive)
        }

        public void Initialize(SerializerSelector serializerSelector)
        {
            parentSerializer = serializerSelector.GetSerializer<SpriteFont>();
            if (parentSerializer == null)
            {
                throw new InvalidOperationException(string.Format("Could not find parent serializer for type {0}", "SiliconStudio.Paradox.Graphics.SpriteFont"));
            }
        }

        public override void Serialize(ref DynamicSpriteFont font, ArchiveMode mode, SerializationStream stream)
        {
            SpriteFont spriteFont = font;
            parentSerializer.Serialize(ref spriteFont, mode, stream);
            font = (DynamicSpriteFont)spriteFont;

            if (mode == ArchiveMode.Deserialize)
            {
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var fontSystem = services.GetSafeServiceAs<FontSystem>();

                font.FontName = stream.Read<string>();
                font.Style = stream.Read<FontStyle>();
                font.UseKerning = stream.Read<bool>();
                font.AntiAlias = stream.Read<FontAntiAliasMode>();

                font.FontSystem = fontSystem;
            }
            else
            {
                stream.Write(font.FontName);
                stream.Write(font.Style);
                stream.Write(font.UseKerning);
                stream.Write(font.AntiAlias);
            }
        }
    }
}