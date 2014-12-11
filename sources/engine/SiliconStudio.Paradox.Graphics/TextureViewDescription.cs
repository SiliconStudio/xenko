// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    public struct TextureViewDescription
    {
        public TextureFlags Flags;

        public ViewType Type;

        public PixelFormat Format;

        public int ArraySlice;

        public int MipLevel;

        public TextureViewDescription ToStagingDescription()
        {
            var viewDescription = this;
            viewDescription.Flags = TextureFlags.None;
            return viewDescription;
        }
    }
}