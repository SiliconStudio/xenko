// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    internal partial class SignedDistanceFieldFontShader
    {
        private static EffectBytecode bytecode;

        internal static EffectBytecode Bytecode
        {
            get
            {
                return bytecode ?? (bytecode = EffectBytecode.FromBytesSafe(signedDistanceFieldFontBytecode));
            }
        }
    }
}
