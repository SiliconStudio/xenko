// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    public class VariableShaderCouple
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