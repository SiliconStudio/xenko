// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public static class XenkoAttributes
    {
        public static HashSet<string> AvailableAttributes = new HashSet<string> { "Link", "RenameLink", "EntryPoint", "StreamOutput", "Map", "Type", "Color" };
    }
}
