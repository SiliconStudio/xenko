using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    // TODO: Make that private as soon as we stop signing assemblies (so that EffectCompilerServer can use it)
    public class RemoteEffectCompilerEffectAnswer : SocketMessage
    {
        // TODO: Support LoggerResult as well
        public EffectBytecode EffectBytecode { get; set; }
    }
}