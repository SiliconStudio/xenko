// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Shaders.Parser.Analysis;
using SiliconStudio.Paradox.Shaders.Parser.Ast;
using SiliconStudio.Paradox.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Utility;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    internal class ParadoxStreamAnalyzer : ShaderVisitor
    {
        #region Private members

        /// <summary>
        /// Current stream usage
        /// </summary>
        private StreamUsage currentStreamUsage = StreamUsage.Read;

        /// <summary>
        /// List of stream usage
        /// </summary>
        private List<StreamUsageInfo> currentStreamUsageList = null;

        /// <summary>
        /// List of already added methods.
        /// </summary>
        private List<MethodDeclaration> alreadyAddedMethodsList = null;

        /// <summary>
        /// Status of the assignment
        /// </summary>
        private AssignmentOperatorStatus currentAssignmentOperatorStatus = AssignmentOperatorStatus.Read;

        /// <summary>
        /// Log of all the warnings and errors
        /// </summary>
        private LoggerResult errorWarningLog;

        /// <summary>
        /// Name of the shader
        /// </summary>
        private string shaderName = "Mix";

        #endregion

        #region Public members

        /// <summary>
        /// List of assignations in the form of "streams = ...;"
        /// </summary>
        public HashSet<AssignmentExpression> StreamAssignations = new HashSet<AssignmentExpression>();

        /// <summary>
        /// List of assignations in the form of "... = streams;"
        /// </summary>
        public HashSet<AssignmentExpression> AssignationsToStream = new HashSet<AssignmentExpression>();

        /// <summary>
        /// streams usage by method
        /// </summary>
        public Dictionary<MethodDeclaration, List<StreamUsageInfo>> StreamsUsageByMethodDefinition = new Dictionary<MethodDeclaration, List<StreamUsageInfo>>();

        /// <summary>
        /// A list containing all the "streams" Variable references
        /// </summary>
        public HashSet<MethodInvocationExpression> AppendMethodCalls = new HashSet<MethodInvocationExpression>();

        #endregion

        #region Constructor

        public ParadoxStreamAnalyzer(LoggerResult errorLog)
            : base(false, true)
        {
            errorWarningLog = errorLog ?? new LoggerResult();
        }

        #endregion

        public void Run(ShaderClassType shaderClassType)
        {
            shaderName = shaderClassType.Name.Text;
            Visit(shaderClassType);
        }

        #region Private methods

        /// <summary>
        /// Analyse the method definition and store it in the correct lists (based on storage and stream usage)
        /// </summary>
        /// <param name="methodDefinition">the MethodDefinition</param>
        [Visit]
        protected void Visit(MethodDefinition methodDefinition)
        {
            currentStreamUsageList = new List<StreamUsageInfo>();
            alreadyAddedMethodsList = new List<MethodDeclaration>();
            
            Visit((Node)methodDefinition);

            if (currentStreamUsageList.Count > 0)
                StreamsUsageByMethodDefinition.Add(methodDefinition, currentStreamUsageList);
        }

        /// <summary>
        /// Calls the base method but modify the stream usage beforehand
        /// </summary>
        /// <param name="expression">the method expression</param>
        [Visit]
        protected void Visit(MethodInvocationExpression expression)
        {
            Visit((Node)expression);

            var methodDecl = expression.Target.TypeInference.Declaration as MethodDeclaration;
            
            if (methodDecl != null)
            {
                // Stream analysis
                if (methodDecl.ContainsTag(ParadoxTags.ShaderScope)) // this will prevent built-in function to appear in the list
                {
                    // test if the method was previously added
                    if (!alreadyAddedMethodsList.Contains(methodDecl))
                    {
                        currentStreamUsageList.Add(new StreamUsageInfo { CallType = StreamCallType.Method, MethodDeclaration = methodDecl, Expression = expression });
                        alreadyAddedMethodsList.Add(methodDecl);
                    }
                }
                for (int i = 0; i < expression.Arguments.Count; ++i)
                {
                    var arg = expression.Arguments[i] as MemberReferenceExpression; // TODO:

                    if (arg != null && IsStreamMember(arg))
                    {
                        var isOut = methodDecl.Parameters[i].Qualifiers.Contains(SiliconStudio.Shaders.Ast.ParameterQualifier.Out);

                        //if (methodDecl.Parameters[i].Qualifiers.Contains(Ast.ParameterQualifier.InOut))
                        //    Error(MessageCode.ErrorInOutStream, expression.Span, arg, methodDecl, contextModuleMixin.MixinName);

                        var usage = methodDecl.Parameters[i].Qualifiers.Contains(SiliconStudio.Shaders.Ast.ParameterQualifier.Out) ? StreamUsage.Write : StreamUsage.Read;
                        AddStreamUsage(arg.TypeInference.Declaration as Variable, arg, usage);
                    }
                }
            }

            // TODO: <shaderclasstype>.Append should be avoided
            if (expression.Target is MemberReferenceExpression && (expression.Target as MemberReferenceExpression).Target.TypeInference.TargetType is ClassType && (expression.Target as MemberReferenceExpression).Member.Text == "Append")
                AppendMethodCalls.Add(expression);
        }

        private static bool IsStreamMember(MemberReferenceExpression expression)
        {
            if (expression.TypeInference.Declaration is Variable)
            {
                return (expression.TypeInference.Declaration as Variable).Qualifiers.Contains(ParadoxStorageQualifier.Stream);
            }
            return false;
        }

        /// <summary>
        /// Analyse the VariableReferenceExpression, detects streams, propagate type inference, get stored in the correct list for later analysis
        /// </summary>
        /// <param name="variableReferenceExpression">the VariableReferenceExpression</param>
        [Visit]
        protected void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            Visit((Node)variableReferenceExpression);
            // HACK: force types on base, this and stream keyword to eliminate errors in the log an use the standard type inference
            if (variableReferenceExpression.Name == StreamsType.ThisStreams)
            {
                if (!(ParentNode is MemberReferenceExpression)) // streams is alone
                    currentStreamUsageList.Add(new StreamUsageInfo { CallType = StreamCallType.Direct, Variable = StreamsType.ThisStreams, Expression = variableReferenceExpression, Usage = currentStreamUsage });
            }
        }

        [Visit]
        protected void Visit(MemberReferenceExpression memberReferenceExpression)
        {
            Visit((Node)memberReferenceExpression);

            // check if it is a stream
            if (IsStreamMember(memberReferenceExpression))
                AddStreamUsage(memberReferenceExpression.TypeInference.Declaration as Variable, memberReferenceExpression, currentStreamUsage);
        }

        [Visit]
        protected void Visit(BinaryExpression expression)
        {
            var prevStreamUsage = currentStreamUsage;
            currentStreamUsage = StreamUsage.Read;
            Visit((Node)expression);
            currentStreamUsage = prevStreamUsage;
        }

        [Visit]
        protected void Visit(UnaryExpression expression)
        {
            var prevStreamUsage = currentStreamUsage;
            currentStreamUsage = StreamUsage.Read;
            Visit((Node)expression);
            currentStreamUsage = prevStreamUsage;
        }

        /// <summary>
        /// Analyse the AssignmentExpression to correctly infer the potential stream usage
        /// </summary>
        /// <param name="assignmentExpression">the AssignmentExpression</param>
        [Visit]
        private void Visit(AssignmentExpression assignmentExpression)
        {
            if (currentAssignmentOperatorStatus != AssignmentOperatorStatus.Read)
                errorWarningLog.Error(ParadoxMessageCode.ErrorNestedAssignment, assignmentExpression.Span, assignmentExpression, shaderName);

            var prevStreamUsage = currentStreamUsage;
            currentStreamUsage = StreamUsage.Read;
            assignmentExpression.Value = (Expression)VisitDynamic(assignmentExpression.Value);
            currentAssignmentOperatorStatus = (assignmentExpression.Operator != AssignmentOperator.Default) ? AssignmentOperatorStatus.ReadWrite : AssignmentOperatorStatus.Write;

            currentStreamUsage = StreamUsage.Write;
            assignmentExpression.Target = (Expression)VisitDynamic(assignmentExpression.Target);

            currentAssignmentOperatorStatus = AssignmentOperatorStatus.Read;
            currentStreamUsage = prevStreamUsage;

            if (assignmentExpression.Operator == AssignmentOperator.Default)
            {
                if (assignmentExpression.Target is VariableReferenceExpression && (assignmentExpression.Target as VariableReferenceExpression).TypeInference.TargetType is StreamsType) // "streams = ...;"
                    StreamAssignations.Add(assignmentExpression);
                else if (assignmentExpression.Value is VariableReferenceExpression && (assignmentExpression.Value as VariableReferenceExpression).TypeInference.TargetType is StreamsType) // "... = streams;"
                    AssignationsToStream.Add(assignmentExpression);
            }
        }

        /// <summary>
        /// Adds a stream usage to the current method
        /// </summary>
        /// <param name="variable">the stream Variable</param>
        /// <param name="expression">the calling expression</param>
        /// <param name="usage">the encountered usage</param>
        private void AddStreamUsage(Variable variable, SiliconStudio.Shaders.Ast.Expression expression, StreamUsage usage)
        {
            currentStreamUsageList.Add(new StreamUsageInfo { CallType = StreamCallType.Member, Variable = variable, Expression = expression, Usage = usage });
        }

        #endregion
    }

    [Flags]
    internal enum StreamUsage
    {
        Unknown = 0,
        Read = 1,
        Write = 2
    }

    internal enum StreamCallType
    {
        Unknown = 0,
        Member = 1,
        Method = 2,
        Direct = 3
    }

    internal class StreamUsageInfo
    {
        public StreamUsage Usage = StreamUsage.Unknown;
        public StreamCallType CallType = StreamCallType.Unknown;
        public Variable Variable = null;
        public MethodDeclaration MethodDeclaration = null;
        public Expression Expression;
    }
}
