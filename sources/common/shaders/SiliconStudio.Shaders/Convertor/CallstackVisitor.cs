// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Shaders.Convertor
{
    public class CallstackVisitor : ShaderRewriter
    {

        public CallstackVisitor() : base(true, true)
        {
        }

        public virtual void Run(MethodDefinition methodEntry)
        {
            base.Visit(methodEntry);
        }

        public override Node Visit(MethodInvocationExpression methodInvocationExpression)
        {
            // Visit childrens first
            base.Visit(methodInvocationExpression);

            var nestedMethodRef = methodInvocationExpression.Target as VariableReferenceExpression;

            if (nestedMethodRef != null)
            {
                var nestedMethod = nestedMethodRef.TypeInference.Declaration as MethodDefinition;
                if (nestedMethod != null && !nestedMethod.IsBuiltin)
                {
                    this.ProcessMethodInvocation(methodInvocationExpression, nestedMethod);
                }
            }

            return methodInvocationExpression;
        }

        protected virtual void ProcessMethodInvocation(MethodInvocationExpression invoke, MethodDefinition method)
        {
            this.VisitDynamic(method);
        }
    }
}
