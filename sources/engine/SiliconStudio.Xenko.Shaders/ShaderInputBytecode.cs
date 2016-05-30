using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Structure containing SPIR-V bytecode, as well as mappings from input attribute locations to semantics.
    /// </summary>
    [DataContract]
    public struct ShaderInputBytecode
    {
        public Dictionary<int, string> InputAttributeNames;

        public Dictionary<string, int> ResourceBindings;

        public byte[] Data;
    }
}