using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    // TODO: Make that private as soon as we stop signing assemblies (so that EffectCompilerServer can use it)
    public class RemoteEffectCompilerEffectRequest : SocketMessage
    {
        public ShaderMixinSource MixinTree { get; set; }
        
        // MixinTree.UsedParameters is DataMemberIgnore, so transmit it manually
        public ShaderMixinParameters UsedParameters { get; set; }
    }
}