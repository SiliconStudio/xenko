// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    [DataContract]
    internal class MethodDeclarationShaderCouple
    {
        public MethodDeclaration Method;
        public ShaderClassType Shader;

        public MethodDeclarationShaderCouple() : this(null, null){}

        public MethodDeclarationShaderCouple(MethodDeclaration method, ShaderClassType shader)
        {
            Method = method;
            Shader = shader;
        }
    }
}
