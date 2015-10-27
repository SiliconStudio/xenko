// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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