// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    internal class XenkoTypeCleaner : ShaderWalker
    {
        public XenkoTypeCleaner()
            : base(false, false)
        {
        }

        public void Run(Shader shader)
        {
            Visit(shader);
        }

        public override void DefaultVisit(Node node)
        {
            if (node is Expression || node is TypeBase)
                VisitTypeInferencer((ITypeInferencer)node);

            base.DefaultVisit(node);
        }

        private void VisitTypeInferencer(ITypeInferencer expression)
        {
            expression.TypeInference.Declaration = null;
            expression.TypeInference.TargetType = null;
            expression.TypeInference.ExpectedType = null;
        }
    }
}
