// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Visitor
{
    /// <summary>
    /// A Generic Visitor.
    /// </summary>
    /// <remarks>
    /// An derived classs need to set the Iterator with this instance.
    /// </remarks>
    public abstract class ShaderVisitor : VisitorBase
    {
        private readonly NodeProcessor nodeProcessor;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderVisitor"/> class.
        /// </summary>
        /// <param name="buildScopeDeclaration">if set to <c>true</c> [build scope declaration].</param>
        /// <param name="useNodeStack">if set to <c>true</c> [use node stack].</param>
        protected ShaderVisitor(bool buildScopeDeclaration, bool useNodeStack) : base(useNodeStack)
        {
            nodeProcessor = OnNodeProcessor;
            if (buildScopeDeclaration)
            {
                ScopeStack = new Stack<ScopeDeclaration>();
                ScopeStack.Push(this.NewScope());
            }
        }

        #endregion

        #region Properties

        protected virtual ScopeDeclaration NewScope(IScopeContainer container = null)
        {
            return new ScopeDeclaration(container);
        }

        /// <summary>
        /// Gets the parent node or null if no parents
        /// </summary>
        public Node ParentNode
        {
            get
            {
                return NodeStack.Count > 1 ? NodeStack[NodeStack.Count - 2] : null;
            }
        }

        /// <summary>
        /// Gets the scope stack.
        /// </summary>
        protected Stack<ScopeDeclaration> ScopeStack { get; private set; }

        #endregion

        #region Public Methods

        [Visit]
        protected virtual Node Visit(Node node)
        {
            return node.Childrens(nodeProcessor);
        }

        private Node OnNodeProcessor(Node nodeArg, ref NodeProcessorContext explorer)
        {
            return VisitDynamic(nodeArg);
        }

        /// <summary>
        /// Finds a list of declaration by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A list of declaration</returns>
        protected virtual IEnumerable<IDeclaration> FindDeclarations(string name)
        {
            return ScopeStack.SelectMany(scopeDecl => scopeDecl.FindDeclaration(name));
        }

        /// <summary>
        /// Finds a declaration by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A declaration</returns>
        protected IDeclaration FindDeclaration(string name)
        {
            return FindDeclarations(name).FirstOrDefault();
        }

        protected override bool PreVisitNode(Node node)
        {
            if (ScopeStack != null)
            {
                var declaration = node as IDeclaration;
                if (declaration != null)
                {
                    // If this is a variable, add only instance variables
                    if (declaration is Variable)
                    {
                        foreach (var variable in ((Variable)declaration).Instances())
                            ScopeStack.Peek().AddDeclaration(variable);
                    }
                    else
                    {
                        ScopeStack.Peek().AddDeclaration(declaration);
                    }
                }

                var scopeContainer = node as IScopeContainer;
                if (scopeContainer != null)
                {
                    ScopeStack.Push(this.NewScope(scopeContainer));
                }
            }
            return true;
        }

        protected override void PostVisitNode(Node node, bool nodeVisited)
        {
            if (ScopeStack != null && node is IScopeContainer)
                ScopeStack.Pop();
        }

        #endregion
    }
}