using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Shaders.Compiler
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

        public EffectBytecodeCompilerResult(EffectBytecode bytecode) : this()
        {
            Bytecode = bytecode;
            CompilationLog = emptyLoggerResult;
        }

        public EffectBytecodeCompilerResult(EffectBytecode bytecode, LoggerResult compilationLog)
        {
            Bytecode = bytecode;
            CompilationLog = compilationLog;
        }
    }
}