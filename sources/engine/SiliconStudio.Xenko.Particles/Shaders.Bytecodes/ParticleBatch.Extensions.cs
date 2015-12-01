// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles
{
    public partial class ParticleBatch
    {
        private static EffectBytecode bytecode = null;
        private static EffectBytecode bytecodeSRgb = null;
        private static EffectBytecode bytecodeTex0 = null;
        private static EffectBytecode bytecodeSRgbTex0 = null;

        internal static EffectBytecode Bytecode
        {
            get
            {
                return bytecode ?? (bytecode = EffectBytecode.FromBytesSafe(binaryBytecode));
            }
        }

        internal static EffectBytecode BytecodeSRgb
        {
            get
            {
                return bytecodeSRgb ?? (bytecodeSRgb = EffectBytecode.FromBytesSafe(binaryBytecodeSRgb));
            }
        }

        internal static EffectBytecode BytecodeTex0
        {
            get
            {
                return bytecodeTex0 ?? (bytecodeTex0 = EffectBytecode.FromBytesSafe(binaryBytecodeTex0));
            }
        }

        internal static EffectBytecode BytecodeSRgbTex0
        {
            get
            {
                return bytecodeSRgbTex0 ?? (bytecodeSRgbTex0 = EffectBytecode.FromBytesSafe(binaryBytecodeSRgbTex0));
            }
        }
    }
}