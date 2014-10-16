// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Visitor
{
    /// <summary>
    /// An expression evaluator.
    /// </summary>
    public class ExpressionEvaluator : ShaderVisitor
    {
        private static readonly List<string> hlslScalarTypeNames =
            new List<string>
                {
                    "bool",
                    "int",
                    "uint",
                    "dword",
                    "half",
                    "float",
                    "double",
                    "min16float",
                    "min10float",
                    "min16int",
                    "min12int",
                    "min16uint"
                };

        private readonly Stack<double> values;

        private ExpressionResult result;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        public ExpressionEvaluator() : base(false, false)
        {
            values = new Stack<double>();
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Result of the expression evaluated</returns>
        public ExpressionResult Evaluate(Expression expression)
        {
            values.Clear();
            result = new ExpressionResult();

            // Small optim, if LiteralExpression, we perform a direct eval.
            if (expression is LiteralExpression)
            {
                Visit((LiteralExpression) expression);
            }
            else
            {
                VisitDynamic(expression);
            }

            if (values.Count == 1)
                result.Value = values.Pop();
            else
            {
                result.Error("Cannot evaluate expression {0}", expression.Span, expression);
            }
            return result;
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(Expression expression)
        {
            result.Warning("Expression evaluation [{0}] is not supported", expression.Span, expression);
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(BinaryExpression binaryExpression)
        {
            Visit((Node) binaryExpression);

            var rightValue = values.Pop();
            var leftValue = values.Pop();

            switch (binaryExpression.Operator)
            {
                case BinaryOperator.Plus:
                    values.Push(leftValue + rightValue);
                    break;
                case BinaryOperator.Minus:
                    values.Push(leftValue - rightValue);
                    break;
                case BinaryOperator.Multiply:
                    values.Push(leftValue*rightValue);
                    break;
                case BinaryOperator.Divide:
                    values.Push(leftValue/rightValue);
                    break;
                case BinaryOperator.Modulo:
                    values.Push(leftValue%rightValue);
                    break;
                case BinaryOperator.LeftShift:
                    values.Push((int) leftValue << (int) rightValue);
                    break;
                case BinaryOperator.RightShift:
                    values.Push((int) leftValue >> (int) rightValue);
                    break;
                case BinaryOperator.BitwiseOr:
                    values.Push(((int) leftValue) | ((int) rightValue));
                    break;
                case BinaryOperator.BitwiseAnd:
                    values.Push(((int) leftValue) & ((int) rightValue));
                    break;
                case BinaryOperator.BitwiseXor:
                    values.Push(((int) leftValue) ^ ((int) rightValue));
                    break;
                default:
                    result.Error("Binary operator [{0}] is not supported", binaryExpression.Span, binaryExpression);
                    values.Push(0);
                    break;
            }
        }

        /*
        //[Visit]
        //protected virtual void Visit(IndexerExpression indexerExpression)
        //{
        //    Visit((Node)indexerExpression);
        //    // TODO implement indexer expression eval
        //    result.Error("Indexer expression evaluation [{0}] is not supported", indexerExpression.Span, indexerExpression);
        //}
        //[Visit]
        //protected virtual void Visit(MemberReferenceExpression memberReferenceExpression)
        //{
        //    Visit((Node)memberReferenceExpression);

        //    // TODO implement member reference expression eval
        //    result.Error("Member reference expression evaluation [{0}] is not supported", memberReferenceExpression.Span, memberReferenceExpression);
        //}
        */

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(MethodInvocationExpression methodInvocationExpression)
        {
            if (methodInvocationExpression.Target is TypeReferenceExpression)
            {
                var methodName = (methodInvocationExpression.Target as TypeReferenceExpression).Type.Name.Text;
                if (hlslScalarTypeNames.Contains(methodName))
                {
                    var evaluator = new ExpressionEvaluator();
                    var subResult = evaluator.Evaluate(methodInvocationExpression.Arguments[0]);

                    if (subResult.Value == null)
                        result.Error("Unable to evaluate cast [{0}]", methodInvocationExpression.Span, methodInvocationExpression);
                    else
                    {
                        try
                        {
                            values.Push(Convert.ToDouble(subResult.Value));
                        }
                        catch (Exception e)
                        {
                            result.Error(e.Message, methodInvocationExpression.Span);
                            result.Error("Unable to cast the value [{0}]", methodInvocationExpression.Span, methodInvocationExpression);
                        }
                    }
                }
                else
                    result.Error("Method invocation expression evaluation [{0}] is not supported", methodInvocationExpression.Span, methodInvocationExpression);
            }
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            Visit((Node)variableReferenceExpression);

            var variableDeclaration = variableReferenceExpression.TypeInference.Declaration as Variable;
            if (variableDeclaration == null)
            {
                result.Error("Unable to find variable [{0}]", variableReferenceExpression.Span, variableReferenceExpression);
            }
            else if (variableDeclaration.InitialValue == null)
            {
                result.Error("Variable [{0}] used in expression is not constant", variableReferenceExpression.Span, variableDeclaration);
            }
            else
            {
                var evaluator = new ExpressionEvaluator();
                var subResult = evaluator.Evaluate(variableDeclaration.InitialValue);
                subResult.CopyTo(result);

                if (subResult.HasErrors)
                {
                    values.Push(0.0f);
                }
                else
                {
                    values.Push(Convert.ToDouble(subResult.Value));
                }
            }
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(LiteralExpression literalExpression)
        {
            values.Push(Convert.ToDouble(literalExpression.Literal.Value));
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(ParenthesizedExpression parenthesizedExpression)
        {
            // value stack is unchanged
            Visit((Node)parenthesizedExpression);
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(UnaryExpression unaryExpression)
        {
            Visit((Node)unaryExpression);

            var value = values.Pop();

            switch (unaryExpression.Operator)
            {
                case UnaryOperator.Plus:
                    values.Push(value);
                    break;
                case UnaryOperator.Minus:
                    values.Push(-value);
                    break;
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PostIncrement:
                    // TODO Pre/Post increment/decrement are not correctly handled
                    value++;
                    values.Push(value);
                    break;
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostDecrement:
                    value--;
                    values.Push(value);
                    break;
                default:
                    result.Error("Unary operator [{0}] is not supported", unaryExpression.Span, unaryExpression);
                    values.Push(0);
                    break;
            }
        }
    }
}
