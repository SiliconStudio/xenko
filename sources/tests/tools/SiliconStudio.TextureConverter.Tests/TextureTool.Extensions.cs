// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.TextureConverter.Tests
{
    internal static class TextureToolExtensions
    {
        public static TexImage Load(this TextureTool tool, string file)
        {
            return tool.Load(file, false);
        }
    }
}