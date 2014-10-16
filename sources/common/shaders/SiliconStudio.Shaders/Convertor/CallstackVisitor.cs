// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Shaders.Convertor
{
    public class CallstackVisitor : ShaderVisitor
    {

        public CallstackVisitor() : base(true, true)
        {
        }

        public virtual void Run(MethodDefinition methodEntry)
        {
            this.Visit((Node)methodEntry);
        }

        [Visit]
        protected virtual void Visit(MethodInvocationExpression methodInvocationExpression)
        {
            // Visit childrens first
            Visit((Node)methodInvocationExpression);

            var nestedMethodRef = methodInvocationExpression.Target as VariableReferenceExpression;

            if (nestedMethodRef != null)
            {
                var nestedMethod = nestedMethodRef.TypeInference.Declaration as MethodDefinition;
                if (nestedMethod != null && !nestedMethod.IsBuiltin)
                {
                    this.ProcessMethodInvocation(methodInvocationExpression, nestedMethod);
                }
            }
        }

        protected virtual void ProcessMethodInvocation(MethodInvocationExpression invoke, MethodDefinition method)
        {
            this.VisitDynamic(method);
        }
    }
}