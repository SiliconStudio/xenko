// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    internal class XenkoReplaceExtern : ShaderRewriter
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

        public XenkoReplaceExtern(Variable variable, IndexerExpression expression)
            : base(false, true)
        {
            VariableToReplace = variable;
            IndexerReplacement = expression;
        }

        public void Run(Node initialNode)
        {
            VisitDynamic(initialNode);
        }

        #endregion

        public override Node Visit(MemberReferenceExpression expression)
        {
            base.Visit(expression);
            if (expression.Member.Text == VariableToReplace.Name.Text)
                return new IndexerExpression(new MemberReferenceExpression(expression.Target, (IndexerReplacement.Target as VariableReferenceExpression).Name.Text), IndexerReplacement.Index);

            return expression;
        }

        public override Node Visit(VariableReferenceExpression expression)
        {
            base.Visit(expression);
            if (expression.Name.Text == VariableToReplace.Name.Text)
                return IndexerReplacement;

            return expression;
        }
    }
}
