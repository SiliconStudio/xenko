// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics
{
    internal partial class EffectProgram : GraphicsResourceBase
    {
        public static EffectProgram New(GraphicsDevice graphicsDevice, EffectBytecode bytecode)
        {
            var effectProgramLibrary = graphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, typeof(EffectProgram), d => new EffectProgramLibrary());
            return effectProgramLibrary.GetOrCreateShader(graphicsDevice, bytecode);
        }

        private class EffectProgramLibrary : IDisposable
        {
            private readonly Dictionary<EffectBytecode, EffectProgram> ShaderLibrary = new Dictionary<EffectBytecode, EffectProgram>();

            public EffectProgram GetOrCreateShader(GraphicsDevice graphicsDevice, EffectBytecode bytecode)
            {
                lock (ShaderLibrary)
                {
                    EffectProgram effectProgram;
                    if (!ShaderLibrary.TryGetValue(bytecode, out effectProgram))
                    {
                        effectProgram = new EffectProgram(graphicsDevice, bytecode);
                        ShaderLibrary.Add(bytecode, effectProgram);
                    }
                    return effectProgram;
                }
            }

            public void Dispose()
            {
                lock (ShaderLibrary)
                {
                    foreach (var effectProgram in ShaderLibrary)
                    {
                        effectProgram.Value.Dispose();
                    }
                    ShaderLibrary.Clear();
                }
            }
        }
    }
}