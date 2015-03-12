﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;

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
            var literalExpression = expression as LiteralExpression;
            if (literalExpression != null)
            {
                Visit(literalExpression);
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

            if (values.Count < 2)
            {
                return;
            }

            var rightValue = values.Pop();
            var leftValue = values.Pop();

            var resultValue = 0.0;

            switch (binaryExpression.Operator)
            {
                case BinaryOperator.Plus:
                    resultValue = leftValue + rightValue;
                    break;
                case BinaryOperator.Minus:
                    resultValue = leftValue - rightValue;
                    break;
                case BinaryOperator.Multiply:
                    resultValue = leftValue*rightValue;
                    break;
                case BinaryOperator.Divide:
                    resultValue = leftValue/rightValue;
                    break;
                case BinaryOperator.Modulo:
                    resultValue = leftValue%rightValue;
                    break;
                case BinaryOperator.LeftShift:
                    resultValue = (int) leftValue << (int) rightValue;
                    break;
                case BinaryOperator.RightShift:
                    resultValue = (int) leftValue >> (int) rightValue;
                    break;
                case BinaryOperator.BitwiseOr:
                    resultValue = ((int) leftValue) | ((int) rightValue);
                    break;
                case BinaryOperator.BitwiseAnd:
                    resultValue = ((int) leftValue) & ((int) rightValue);
                    break;
                case BinaryOperator.BitwiseXor:
                    resultValue = ((int) leftValue) ^ ((int) rightValue);
                    break;
                case BinaryOperator.LogicalAnd:
                    resultValue = leftValue != 0.0f && rightValue != 0.0f ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.LogicalOr:
                    resultValue = leftValue != 0.0f || rightValue != 0.0f ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.GreaterEqual:
                    resultValue = leftValue >= rightValue ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.Greater:
                    resultValue = leftValue > rightValue ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.Less:
                    resultValue = leftValue < rightValue ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.LessEqual:
                    resultValue = leftValue <= rightValue ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.Equality:
                    resultValue = leftValue == rightValue ? 1.0f : 0.0f;
                    break;
                case BinaryOperator.Inequality:
                    resultValue = leftValue != rightValue ? 1.0f : 0.0f;
                    break;
                default:
                    result.Error("Binary operator [{0}] is not supported", binaryExpression.Span, binaryExpression);
                    break;
            }

            values.Push(resultValue);
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
                    values.Push(Convert.ToDouble(subResult.Value, CultureInfo.InvariantCulture));

                    try
                    {
                    }
                    catch (Exception e)
                    {
                        result.Error(e.Message, methodInvocationExpression.Span);
                        result.Error("Unable to cast the value [{0}]", methodInvocationExpression.Span, methodInvocationExpression);
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
                    values.Push(Convert.ToDouble(subResult.Value, CultureInfo.InvariantCulture));
                }
            }
        }

        /// <inheritdoc/>
        [Visit]
        protected virtual void Visit(LiteralExpression literalExpression)
        {
            var value = Convert.ToDouble(literalExpression.Literal.Value, CultureInfo.InvariantCulture);
            values.Push(value);
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

            if (values.Count == 0)
            {
                return;
            }

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
                case UnaryOperator.LogicalNot:
                    values.Push(value == 0.0 ? 1.0f : 0.0f);
                    break;
                default:
                    result.Error("Unary operator [{0}] is not supported", unaryExpression.Span, unaryExpression);
                    values.Push(0);
                    break;
            }
        }
    }
}
