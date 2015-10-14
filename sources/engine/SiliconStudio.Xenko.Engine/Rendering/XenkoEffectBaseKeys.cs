// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering
{
    public static class ParadoxEffectBaseKeys
    {
        public static readonly ParameterKey<ShaderSource> ExtensionPostVertexStageShader = ParameterKeys.New<ShaderSource>();
    }
}