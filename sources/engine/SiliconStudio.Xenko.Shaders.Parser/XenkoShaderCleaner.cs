// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser
{
    internal class XenkoShaderCleaner : ShaderVisitor
    {
        public XenkoShaderCleaner() : base(false, false)
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
            variable.RemoveTag(XenkoTags.ConstantBuffer);
            variable.Qualifiers.Values.Remove(XenkoStorageQualifier.Stream);
            variable.Qualifiers.Values.Remove(XenkoStorageQualifier.Stage);
            variable.Qualifiers.Values.Remove(XenkoStorageQualifier.PatchStream);
            Visit((Node)variable);
        }

        [Visit]
        public void Visit(MethodDeclaration methodDeclaration)
        {
            methodDeclaration.Qualifiers.Values.Remove(XenkoStorageQualifier.Override);
            methodDeclaration.Qualifiers.Values.Remove(XenkoStorageQualifier.Clone);
            methodDeclaration.Qualifiers.Values.Remove(XenkoStorageQualifier.Stage);
            Visit((Node)methodDeclaration);
        }

        [Visit]
        public AttributeDeclaration Visit(AttributeDeclaration attribute)
        {
            if (XenkoAttributes.AvailableAttributes.Contains(attribute.Name))
                return null;

            return attribute;
        }
    }
}