// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class StreamFieldVisitor : ShaderVisitor
    {
        private Variable typeInference = null;

        private Expression arrayIndex;

        public StreamFieldVisitor(Variable variable, Expression index = null)
            : base(false, false)
        {
            typeInference = variable;
            arrayIndex = index;
        }

        public Expression Run(Expression expression)
        {
            return Visit(expression);
        }

        [Visit]
        private Expression Visit(Expression expression)
        {
            Visit((Node)expression);

            if ((expression is VariableReferenceExpression || expression is MemberReferenceExpression || expression is IndexerExpression) && expression.TypeInference.TargetType is StreamsType) // TODO: exclude constants, test real type
            {
                var mre = new MemberReferenceExpression(expression, typeInference.Name) { TypeInference = { Declaration = typeInference, TargetType = typeInference.Type.ResolveType() } };
                if (arrayIndex == null)
                    return mre;
                else
                {
                    var ire = new IndexerExpression(mre, arrayIndex);
                    return ire;
                }
            }
            
            return expression;
        }
    }
}
