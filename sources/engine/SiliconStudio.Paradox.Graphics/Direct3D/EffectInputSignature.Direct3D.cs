// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class EffectInputSignature
    {
        private readonly byte[] bytecode;

        internal EffectInputSignature(ObjectId id, byte[] bytecode)
        {
            this.Id = id;
            this.bytecode = bytecode;
        }

        internal byte[] NativeSignature
        {
            get
            {
                return bytecode;
            }
        }

        public override string ToString()
        {
            var description = "Input Parameters: ";

            var shaderReflection = new SharpDX.D3DCompiler.ShaderReflection(NativeSignature);
            for (int i = 0; i < shaderReflection.Description.InputParameters; i++)
            {
                var parameterDescription = shaderReflection.GetInputParameterDescription(i);
                description += parameterDescription.SemanticName+parameterDescription.SemanticIndex;

                if (i != shaderReflection.Description.InputParameters - 1)
                    description += ", ";
            }

            return description;
        }
    }
}
#endif