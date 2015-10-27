// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class ParadoxReplaceExtern : ShaderVisitor
    {
        #region Private members

        /// <summary>
        /// The variable to replace
        /// </summary>
        private Variable VariableToReplace = null;

        /// <summary>
        /// the expression that will replace the variable
        /// </summary>
        private IndexerExpression IndexerReplacement = null;

        #endregion

        #region Constructor

        public ParadoxReplaceExtern(Variable variable, IndexerExpression expression)
            : base(false, true)
        {
            VariableToReplace = variable;
            IndexerReplacement = expression;
        }

        public void Run(Node initialNode)
        {
            Visit(initialNode);
        }

        #endregion

        [Visit]
        protected Node Visit(MemberReferenceExpression expression)
        {
            Visit((Node)expression);
            if (expression.Member.Text == VariableToReplace.Name.Text)
                return new IndexerExpression(new MemberReferenceExpression(expression.Target, (IndexerReplacement.Target as VariableReferenceExpression).Name.Text), IndexerReplacement.Index);

            return expression;
        }

        [Visit]
        protected Node Visit(VariableReferenceExpression expression)
        {
            Visit((Node)expression);
            if (expression.Name.Text == VariableToReplace.Name.Text)
                return IndexerReplacement;

            return expression;
        }
    }
}
