using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    [DataContract]
    public class RemoteEffectCompilerEffectRequested
    {
        public EffectCompileRequest Request { get; set; }
    }
}