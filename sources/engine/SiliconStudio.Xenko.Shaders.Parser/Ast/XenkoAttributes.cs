// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public static class ParadoxAttributes
    {
        public static HashSet<string> AvailableAttributes = new HashSet<string> { "Link", "RenameLink", "EntryPoint", "StreamOutput", "Map", "Type", "Color" };
    }
}