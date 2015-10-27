// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    /// <summary>
    /// Class to replace a node by another in an AST
    /// </summary>
    internal class XenkoReplaceVisitor : ShaderVisitor
    {
        #region Private members

        /// <summary>
        /// The node to replace
        /// </summary>
        protected Node nodeToReplace;

        /// <summary>
        /// the replacement node
        /// </summary>
        protected Node replacementNode;

        /// <summary>
        /// a boolean stating that the operation is complete
        /// </summary>
        protected bool complete = false;

        #endregion

        #region Constructor

        public XenkoReplaceVisitor(Node toReplace, Node replacement) : base(false, false)
        {
            nodeToReplace = toReplace;
            replacementNode = replacement;
        }

        #endregion

        #region Public method

        public bool Run(Node startNode)
        {
            Visit(startNode);

            return complete;
        }

        #endregion

        #region Protected method

        [Visit]
        protected override Node Visit(Node node)
        {
            if (node == nodeToReplace)
            {
                complete = true;
                return replacementNode;
            }
            
            return base.Visit(node);
        }

        #endregion
    }
}
