// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
