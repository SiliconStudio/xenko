using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Design
{
    /// <summary>
    /// Defines how <see cref="EffectSystem.CreateEffectCompiler"/> tries to create compiler.
    /// </summary>
    public enum EffectCompilationMode
    {
        /// <summary>
        /// Effects can't be compiled. <see cref="Shaders.Compiler.NullEffectCompiler"/> will be used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Effects can only be compiled in process (if possible). <see cref="Shaders.Compiler.EffectCompiler"/> will be used.
        /// </summary>
        Local = 1,

        /// <summary>
        /// Effects can only be compiled over network. <see cref="Shaders.Compiler.RemoteEffectCompiler"/> will be used.
        /// </summary>
        Remote = 2,

        /// <summary>
        /// Effects can be compiled either in process (if possible) or over network otherwise.
        /// </summary>
        LocalOrRemote = Local | Remote,
    }
}