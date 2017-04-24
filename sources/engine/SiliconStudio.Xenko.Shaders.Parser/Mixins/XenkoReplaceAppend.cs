// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    internal class XenkoReplaceAppend : ShaderRewriter
    {
        #region Private members

        /// <summary>
        /// List of append methods
        /// </summary>
        private HashSet<MethodInvocationExpression> appendMethodsList;

        /// <summary>
        /// List of output statements replacing the append method
        /// </summary>
        private List<Statement> outputStatements;

        /// <summary>
        /// Variable replacing the stream in the append function
        /// </summary>
        private VariableReferenceExpression outputVre;

        #endregion

        #region Constructor

        public XenkoReplaceAppend(HashSet<MethodInvocationExpression> appendList, List<Statement> output, VariableReferenceExpression vre)
            : base(false, false)
        {
            appendMethodsList = appendList;
            outputStatements = output;
            outputVre = vre;
        }

        #endregion

        #region Public method

        public void Run(Node startNode)
        {
            VisitDynamic(startNode);
        }

        #endregion

        #region Protected method

        public override Node Visit(ExpressionStatement expressionStatement)
        {
            base.Visit(expressionStatement);

            if (appendMethodsList.Contains(expressionStatement.Expression))
            {
                var appendMethodCall = expressionStatement.Expression as MethodInvocationExpression;
                var blockStatement = new BlockStatement();
                blockStatement.Statements.AddRange(outputStatements);
                appendMethodCall.Arguments[0] = outputVre;
                blockStatement.Statements.Add(expressionStatement);
                return blockStatement;
            }
            return expressionStatement;
        }

        #endregion
    }
}
