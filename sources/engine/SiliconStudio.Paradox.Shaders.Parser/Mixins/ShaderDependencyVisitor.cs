// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Utility;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    class ShaderDependencyVisitor : ShaderVisitor
    {
        public HashSet<Tuple<IdentifierGeneric, Node>> FoundIdentifiers = new HashSet<Tuple<IdentifierGeneric, Node>>();

        public HashSet<string> FoundClasses = new HashSet<string>();

        private readonly LoggerResult log;

        /// <summary>
        /// Name of the classes
        /// </summary>
        //private HashSet<string> shaderClassNames;
        private readonly ShaderSourceManager sourceManager;

        public ShaderDependencyVisitor(LoggerResult log, ShaderSourceManager sourceManager)
            : base(false, true)
        {
            if (log == null) throw new ArgumentNullException("log");
            if (sourceManager == null) throw new ArgumentNullException("sourceManager");

            this.log = log;
            this.sourceManager = sourceManager;
        }

        public void Run(ShaderClassType shaderClassType)
        {
            Visit(shaderClassType);
        }

        [Visit]
        private void Visit(IdentifierGeneric identifier)
        {
            Visit((Node)identifier);

            FoundIdentifiers.Add(Tuple.Create(identifier, ParentNode));
        }

        [Visit]
        private void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            Visit((Node)variableReferenceExpression);

            if (sourceManager.IsClassExists(variableReferenceExpression.Name.Text))
                FoundClasses.Add(variableReferenceExpression.Name.Text);
        }

        [Visit]
        private void Visit(MemberReferenceExpression memberReferenceExpression)
        {
            Visit((Node)memberReferenceExpression);

            if (sourceManager.IsClassExists(memberReferenceExpression.Member.Text))
                FoundClasses.Add(memberReferenceExpression.Member.Text);
        }

        [Visit]
        private void Visit(TypeBase typeBase)
        {
            Visit((Node)typeBase);

            if (sourceManager.IsClassExists(typeBase.Name.Text))
            {
                FoundClasses.Add(typeBase.Name.Text);
            }
            else if (typeBase is ShaderTypeName)
            {
                // Special case for ShaderTypeName as we must generate an error if it is not found
                log.Error(ParadoxMessageCode.ErrorClassNotFound, typeBase.Span, typeBase.Name.Text);
            }
        }
    }
}
