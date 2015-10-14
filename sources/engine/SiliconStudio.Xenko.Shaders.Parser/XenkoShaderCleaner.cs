// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser
{
    internal class ParadoxShaderCleaner : ShaderVisitor
    {
        public ParadoxShaderCleaner() : base(false, false)
        {
        }

        /// <summary>
        /// Runs this instance on the specified node.
        /// </summary>
        /// <param name="shader">The shader.</param>
        public void Run(Shader shader)
        {
            Visit(shader);
        }

        public void Run(ShaderClassType shaderClassType)
        {
            var shader = new Shader();
            shader.Declarations.Add(shaderClassType);
            Run(shader);
        }

        [Visit]
        public void Visit(Variable variable)
        {
            variable.Qualifiers.Values.Remove(ParadoxStorageQualifier.Stream);
            variable.Qualifiers.Values.Remove(ParadoxStorageQualifier.Stage);
            variable.Qualifiers.Values.Remove(ParadoxStorageQualifier.PatchStream);
            Visit((Node)variable);
        }

        [Visit]
        public void Visit(MethodDeclaration methodDeclaration)
        {
            methodDeclaration.Qualifiers.Values.Remove(ParadoxStorageQualifier.Override);
            methodDeclaration.Qualifiers.Values.Remove(ParadoxStorageQualifier.Clone);
            methodDeclaration.Qualifiers.Values.Remove(ParadoxStorageQualifier.Stage);
            Visit((Node)methodDeclaration);
        }

        [Visit]
        public AttributeDeclaration Visit(AttributeDeclaration attribute)
        {
            if (ParadoxAttributes.AvailableAttributes.Contains(attribute.Name))
                return null;

            return attribute;
        }
    }
}