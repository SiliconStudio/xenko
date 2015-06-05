using System;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Engine.Design
{
    /// <summary>
    /// Defines how <see cref="EffectSystem.CreateEffectCompiler"/> tries to create compiler.
    /// </summary>
    [Flags]
    public enum EffectCompilationMode
    {
        None = 0,
        Local = 1,
        Remote = 2,
        LocalOrRemote = Local | Remote,
    }
}