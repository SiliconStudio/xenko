// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Lights
{

    public sealed class LightPermutationsEntry
    {
        public LightPermutationsEntry()
        {
            ShaderSources = new List<ShaderSource>();
            Parameters = new ParameterCollection();
        }

        public readonly List<ShaderSource> ShaderSources;

        public readonly ParameterCollection Parameters;
    }
}