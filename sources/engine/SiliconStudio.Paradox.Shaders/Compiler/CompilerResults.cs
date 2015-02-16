// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Result of a compilation.
    /// </summary>
    public class CompilerResults : LoggerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerResult" /> class.
        /// </summary>
        public CompilerResults() : base(null)
        {
            Bytecodes = new Dictionary<string, TaskOrResult<EffectBytecodeCompilerResult>>();
            UsedParameters = new Dictionary<string, ShaderMixinParameters>();
        }

        /// <summary>
        /// Gets or sets the main bytecode.
        /// </summary>
        /// <value>
        /// The main bytecode.
        /// </value>
        public TaskOrResult<EffectBytecodeCompilerResult> MainBytecode { get; set; }

        /// <summary>
        /// Gets the bytecode. May be null if <see cref="LoggerResult.HasErrors"/> is <c>true</c>.
        /// </summary>
        /// <value>The bytecode.</value>
        public Dictionary<string, TaskOrResult<EffectBytecodeCompilerResult>> Bytecodes { get; set; }

        /// <summary>
        /// Parameters used to create this shader.
        /// </summary>
        /// <value>The ParameterCollection.</value>
        public ParameterCollection MainUsedParameters { get; set; }

        /// <summary>
        /// List of all the used parameters per child.
        /// </summary>
        public Dictionary<string, ShaderMixinParameters> UsedParameters { get; set; }
    }
}