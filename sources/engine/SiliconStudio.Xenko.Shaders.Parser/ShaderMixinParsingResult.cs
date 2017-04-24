// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Xenko.Shaders.Parser
{
    public class ShaderMixinParsingResult : ParsingResult
    {
        public ShaderMixinParsingResult()
        {
            EntryPoints = new Dictionary<ShaderStage, string>();
            HashSources = new HashSourceCollection();
        }

        public EffectReflection Reflection { get; set; }

        public Dictionary<ShaderStage, string> EntryPoints;

        public HashSourceCollection HashSources { get; set; }
    }
}
