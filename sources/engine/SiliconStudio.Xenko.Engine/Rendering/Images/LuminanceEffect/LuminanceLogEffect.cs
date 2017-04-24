// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Class LuminanceLogEffect.
    /// </summary>
    public class LuminanceLogEffect : ImageEffectShader
    {
        public LuminanceLogEffect(string luminanceShaderName = "LuminanceLogShader")
        {
            EffectName = luminanceShaderName;
        }
    }
}
