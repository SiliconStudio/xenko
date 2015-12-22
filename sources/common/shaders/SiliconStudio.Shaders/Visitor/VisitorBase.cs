// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Shaders.Ast;
using System.Linq;
using LinqExpression = System.Linq.Expressions.Expression;

namespace SiliconStudio.Shaders.Visitor
{
    /// <summary>
    /// Visitor base.
    /// </summary>
    public abstract class VisitorBase
    {
        private delegate object NodeVisitFunction(VisitorBase visitor, object node);

        private readonly static Dictionary<Type, Dictionary<Type, NodeVisitFunction>> AllVisitors = new Dictionary<Type, Dictionary<Type, NodeVisitFunction>>();
        private readonly static Dictionary<Type, List<Type>> MapDefaultTypeToInheritance = new Dictionary<Type, List<Type>>();
        private readonly Dictionary<Type, NodeVisitFunction> methodVisitors;
        private readonly Dictionary<Type, List<Type>> mapTypeToInheritance = new Dictionary<Type, List<Type>>();

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VisitorBase"/> class.
        /// </summary>
        /// <param name="useNodeStack">if set to <c>true</c> [use node stack].</param>
        protected VisitorBase(bool useNodeStack = false)
        {
            lock (AllVisitors)
            {
                Dictionary<Type, NodeVisitFunction> concurrentVisitors;
                var thisType = GetType();
                if (!AllVisitors.TryGetValue(thisType, out concurrentVisitors))
                {
                    concurrentVisitors = Initialize(this);
                    AllVisitors.Add(thisType, concurrentVisitors);
                }
                methodVisitors = new Dictionary<Type, NodeVisitFunction>(concurrentVisitors);
                mapTypeToInheritance = new Dictionary<Type, List<Type>>(MapDefaultTypeToInheritance);
            }

            if (useNodeStack)
            {
                NodeStack = new List<Node>();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the node stack.
        /// </summary>
        /// <value>
        /// The node stack.
        /// </value>
        public List<Node> NodeStack { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Visits the list.
        /// </summary>
        /// <typeparam name="T">Type of the item in the list</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="filter">The function filter.</param>
        protected void VisitDynamicList<T>(IList<T> list, Func<T, bool> filter = null) where T : Node
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                // Filter the element
                if (filter != null && filter(item)) continue;

                var newNode = VisitDynamic(list[i]);

                if (newNode == null)
                {
                    list.RemoveAt(i);
                    i--;
                }
                else if (!ReferenceEquals(newNode, item))
                {
                    list[i] = (T)newNode;
                }
            }
        }

        /// <summary>
        /// Visits the node.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="visitRealType">if set to <c>true</c> [visit real type].</param>
        /// <returns>
        /// A node
        /// </returns>
        protected virtual Node VisitDynamic<T>(T node, bool visitRealType = true) where T : Node
        {
            if (node == null)
            {
                return null;
            }

            bool nodeStackAdded = false;

            if (NodeStack != null)
            {
                if (NodeStack.Count > 0 && ReferenceEquals(NodeStack[NodeStack.Count - 1], node))
                    throw new InvalidOperationException(string.Format("Cannot visit recursively a node [{0}] already being visited", node));

                NodeStack.Add(node);
                nodeStackAdded = true;
            }

            // Only Visit in the Iterator
            bool doVisit = PreVisitNode(node);

            // Double-dispatch using dynamic, much more faster (x4) than expression or reflection methods.
            var result = (Node)node;
            if (doVisit)
            {
                NodeVisitFunction process = null;
                var typeToVisit = visitRealType ? node.GetType() : typeof(T);

                if (!methodVisitors.TryGetValue(typeToVisit, out process))
                {
                    foreach (var ancestor in Ancestors(typeToVisit))
                    {
                        if (methodVisitors.TryGetValue(ancestor, out process))
                        {
                            // Optimize for next pass on this kind of type
                            if (typeToVisit != ancestor)
                                methodVisitors[typeToVisit] = process;
                            break;
                        }
                    }
                }

                if (process == null)
                    throw new InvalidOperationException(string.Format("Unable to find a suitable visitor method for this type [{0}]", typeToVisit.FullName));

                result = (Node)process(this, node);
            }

            // Only Visit in the Iterator
            PostVisitNode(node, doVisit);

            if (NodeStack != null && nodeStackAdded)
            {
                NodeStack.RemoveAt(NodeStack.Count - 1);
            }

            return result;
        }

        /// <summary>
        /// Get all ancestors (type and interfaces) for a particular type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Ancestors of the type.</returns>
        private IEnumerable<Type> Ancestors(Type type)
        {
            return Ancestors(mapTypeToInheritance, type);
        }

        /// <summary>
        /// Get all ancestors (type and interfaces) for a particular type.
        /// </summary>
        /// <param name="registeredTypes">The registered types.</param>
        /// <param name="type">The type.</param>
        /// <returns>Ancestors of the type.</returns>
        private static IEnumerable<Type> Ancestors(Dictionary<Type, List<Type>> registeredTypes, Type type)
        {
            List<Type> inheritance;
            if (registeredTypes.TryGetValue(type, out inheritance))
                return inheritance;

                inheritance = new List<Type>();
                inheritance.AddRange(type.GetTypeInfo().ImplementedInterfaces);
                var baseType = type.GetTypeInfo().BaseType;
                while (baseType != null)
                {
                    inheritance.Add(baseType);
                    baseType = baseType.GetTypeInfo().BaseType;
                }
                registeredTypes[type] = inheritance;

                // If the ancestors is not registered, we will register it to both the local registry
                // and to the shared registry
                lock (AllVisitors)
                {
                    MapDefaultTypeToInheritance[type] = inheritance;
                }
            return inheritance;
        }

        private static Dictionary<Type, NodeVisitFunction> Initialize(VisitorBase visitor)
        {
            var methodVisitors = new Dictionary<Type, NodeVisitFunction>();
            var methods = visitor.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes<VisitAttribute>(true);
                if (attributes.Any())
                {
                    Type parameterType;
                    var function = BuildMethodVisitor(method, out parameterType);
                    methodVisitors[parameterType] = function;

                    // Pre-register default ancestors for all parameters
                    Ancestors(MapDefaultTypeToInheritance, parameterType);
                }
            }
            return methodVisitors;
        }

        private static NodeVisitFunction BuildMethodVisitor(MethodInfo method, out Type parameterType)
        {
            var declaringtype = method.DeclaringType;

            if (declaringtype == null)
                throw new InvalidOperationException(string.Format("No declaring type for method [{0}]", method.Name));

            // Throws an exception if parammeters.Count != 1
            if (method.GetParameters().Length != 1)
                throw new InvalidOperationException(
                    string.Format("Invalid number of parameters [{0}] for visit method [{1}] for type [{2}]. One parameter is expected", method.GetParameters().Length, method.Name, declaringtype.FullName));

            parameterType = method.GetParameters()[0].ParameterType;
            if (!parameterType.GetTypeInfo().IsInterface && !typeof(Node).GetTypeInfo().IsAssignableFrom(parameterType.GetTypeInfo()))
                throw new InvalidOperationException(
                    string.Format("Invalid type parameter [{0}] for visit method [{1}] for type [{2}]. Parameter must inherit from Node", parameterType, method.Name, declaringtype.FullName));

            var thisParameter = LinqExpression.Parameter(typeof(VisitorBase), "this");
            var nodeParameter = LinqExpression.Parameter(typeof(object), "node");
            var thisCastVariable = LinqExpression.Variable(declaringtype, "thisCast");

            var statements = new List<LinqExpression>
                {
                    LinqExpression.Assign(thisCastVariable, LinqExpression.Convert(thisParameter, declaringtype))
                };

            var callVisitMethod = LinqExpression.Call(thisCastVariable, method, LinqExpression.Convert(nodeParameter, parameterType));

            if (typeof(void) == method.ReturnType)
            {
                statements.Add(callVisitMethod);
                statements.Add(nodeParameter);
            } 
            else
            {
                statements.Add(callVisitMethod);
            }

            var block = LinqExpression.Block(new[] { thisCastVariable }, statements);

            var lambda = LinqExpression.Lambda<NodeVisitFunction>(block, thisParameter, nodeParameter);
            return lambda.Compile();
        }

        /// <summary>
        /// Called before visiting the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>True to continue visiting the node; false to skip the visit</returns>
        protected virtual bool PreVisitNode(Node node)
        {
            return true;
        }

        /// <summary>
        /// Called after visiting the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="nodeVisited">if set to <c>true</c> [node visited].</param>
        protected virtual void PostVisitNode(Node node, bool nodeVisited)
        {            
        }

        #endregion

        /// <summary>
        /// Tag a visitable method with this attribute.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
        protected class VisitAttribute : Attribute
        {
        }
    }
}