// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
