// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class ParadoxTagCleaner : ShaderVisitor
    {
        public ParadoxTagCleaner()
            : base(false, false)
        {
        }

        public void Run(ShaderClassType shader)
        {
            Visit(shader);
        }

        [Visit]
        protected override Node Visit(Node node)
        {
            node.RemoveTag(ParadoxTags.ConstantBuffer);
            node.RemoveTag(ParadoxTags.ShaderScope);
            node.RemoveTag(ParadoxTags.StaticRef);
            node.RemoveTag(ParadoxTags.ExternRef);
            node.RemoveTag(ParadoxTags.StageInitRef);
            node.RemoveTag(ParadoxTags.CurrentShader);
            node.RemoveTag(ParadoxTags.VirtualTableReference);
            node.RemoveTag(ParadoxTags.BaseDeclarationMixin);
            node.RemoveTag(ParadoxTags.ShaderScope);
            return base.Visit(node);
        }
    }
}
