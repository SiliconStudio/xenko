// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class ParadoxTypeCleaner : ShaderVisitor
    {
        public ParadoxTypeCleaner()
            : base(false, false)
        {
        }

        public void Run(Shader shader)
        {
            Visit(shader);
        }

        [Visit]
        protected void Visit(Expression expression)
        {
            expression.TypeInference.Declaration = null;
            expression.TypeInference.TargetType = null;
            expression.TypeInference.ExpectedType = null;
            Visit((Node)expression);
        }

        [Visit]
        protected void Visit(TypeBase typeBase)
        {
            typeBase.TypeInference.Declaration = null;
            typeBase.TypeInference.TargetType = null;
            typeBase.TypeInference.ExpectedType = null;
            Visit((Node)typeBase);
        }
    }
}
