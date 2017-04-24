// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Result of an effect bytecode compilation.
    /// </summary>
    public struct EffectBytecodeCompilerResult
    {
        private static readonly LoggerResult emptyLoggerResult = new LoggerResult();

        /// <summary>
        /// The effect bytecode. Might be null.
        /// </summary>
        public readonly EffectBytecode Bytecode;

        /// <summary>
        /// The compilation log.
        /// </summary>
        public readonly LoggerResult CompilationLog;

        /// <summary>
        /// Gets or sets a value that specifies how the shader was loaded.
        /// </summary>
        public readonly EffectBytecodeCacheLoadSource LoadSource;

        public EffectBytecodeCompilerResult(EffectBytecode bytecode, EffectBytecodeCacheLoadSource loadSource) : this()
        {
            Bytecode = bytecode;
            CompilationLog = emptyLoggerResult;
            LoadSource = loadSource;
        }

        public EffectBytecodeCompilerResult(EffectBytecode bytecode, LoggerResult compilationLog)
        {
            Bytecode = bytecode;
            CompilationLog = compilationLog;
            LoadSource = EffectBytecodeCacheLoadSource.JustCompiled;
        }
    }
}
