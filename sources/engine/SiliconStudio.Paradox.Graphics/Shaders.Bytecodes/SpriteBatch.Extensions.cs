// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class SpriteBatch
    {
        private static EffectBytecode bytecode = null;

        private static EffectBytecode Bytecode
        {
            get
            {
                return bytecode ?? (bytecode = EffectBytecode.FromBytesSafe(binaryBytecode));
            }
        }
    }
}