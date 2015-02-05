// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A FXAA anti-aliasing pass.
    /// </summary>
    public class FXAAEffect : ImageEffectShader
    {
        public FXAAEffect(string antialiasShaderName = "FXAAShader")
        {
            EffectName = antialiasShaderName;
        }
    }
}