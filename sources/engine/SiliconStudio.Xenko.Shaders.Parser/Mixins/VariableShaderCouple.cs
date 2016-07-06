// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    [DataContract]
    internal class VariableShaderCouple
    {
        public Variable Variable;
        public ShaderClassType Shader;

        public VariableShaderCouple() : this(null, null) { }
        
        public VariableShaderCouple(Variable variable, ShaderClassType shader)
        {
            Variable = variable;
            Shader = shader;
        }
    }
}