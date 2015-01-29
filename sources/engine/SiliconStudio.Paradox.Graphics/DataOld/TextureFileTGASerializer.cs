// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.Data
{
    [ContentSerializerExtension("tga")]
    public class TextureFileTGASerializer : ContentSerializerBase<Image>
    {
        public override unsafe void Serialize(ContentSerializerContext context, SerializationStream stream, Image textureData)
        {
            if (context.Mode != ArchiveMode.Deserialize)
                throw new NotSupportedException("Texture needs to be in package to be saved.");

            Header header;
            var headerBytes = stream.ReadBytes(Utilities.SizeOf<Header>());
            fixed (byte* p = &headerBytes[0])
            {
                Utilities.ReadOut((IntPtr)p, out header);
            }

            if (header.ImageType != 2 || header.ColorMapType != 0 || (header.Bits != 32 && header.Bits != 24))
            {
                throw new NotSupportedException("Only RGB 24 or 32 bits TGA are currently supported.");
            }

            var pixelData = new byte[header.Width * header.Height * 4];

            for (int y = 0; y < header.Height; ++y)
            {
                var scanlineData = stream.ReadBytes(header.Width * (header.Bits / 8));

                // Convert 24 to 32 bits
                if (header.Bits == 24)
                {
                    var expandedScanlineData = new byte[header.Width * 4];
                    for (int i = 0; i < header.Width; ++i)
                    {
                        expandedScanlineData[i * 4 + 0] = scanlineData[i * 3 + 0];
                        expandedScanlineData[i * 4 + 1] = scanlineData[i * 3 + 1];
                        expandedScanlineData[i * 4 + 2] = scanlineData[i * 3 + 2];
                        expandedScanlineData[i * 4 + 3] = 255;
                    }
                    scanlineData = expandedScanlineData;
                }
                int targetY = ((header.Descriptor & 0x10) == 0x10) ? y : header.Height - 1 - y;
                fixed (byte* dest = &pixelData[header.Width * 4 * targetY])
                fixed (byte* src = &scanlineData[0])
                    Utilities.CopyMemory((IntPtr)dest, (IntPtr)src, header.Width * 4);
            }

            //textureData.Width = header.Width;
            //textureData.Height = header.Height;
            //textureData.MipLevels = 1;
            //// TODO: Check why RGB/BGR seems reverted (maybe little endian/big endian applies?)
            //textureData.PixelFormat = PixelFormat.B8G8R8A8_UNorm;
            //textureData.Pixels = new[] { new TexturePixelData { Pitch = header.Width * 4, Data = pixelData } };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public byte IdentSize;
            public byte ColorMapType;
            public byte ImageType;

            public short ColorMapStart;
            public short ColorMapLength;
            public byte ColorMapBits;

            public short XOffset;
            public short YOffset;
            public short Width;
            public short Height;
            public byte Bits;
            public byte Descriptor;
        }
    }
}