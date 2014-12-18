// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public class MaterialShaderGenerator
    {
        public static ShaderSource Generate(MaterialAsset material)
        {
            var context = new MaterialShaderGeneratorContext()
            {
                Log = new LoggerResult()
            };

            material.GenerateShader(context);

            return context.Operations.FirstOrDefault();
        }
    }
}