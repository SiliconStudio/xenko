// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialContext
    {
        public MaterialDescription Material;

        public Func<MaterialContext, ShaderSource> GenerateShaderSource;

        public bool ExploreGenerics = false;

        public LoggerResult Log;
    }
}
