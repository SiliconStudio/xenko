// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using LinqExpression = System.Linq.Expressions.Expression;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Shader childrens iterator.
    /// </summary>
    public static class NodeIterator
    {
        private static readonly ConcurrentDictionary<Type, NodeProcessor> RegisteredProcessors = new ConcurrentDictionary<Type, NodeProcessor>();

        /// <summary>
        /// Iterate on childrens and apply the specified transform function.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="nodeProcessor">The process.</param>
        /// <param name="listProcessor">The list processor. Default is null using default implementation.</param>
        /// <returns>The source node</returns>
        public static Node Childrens(this Node node, NodeProcessor nodeProcessor, NodeListProcessor listProcessor = null)
        {
            if (node == null) return null;

            var type = node.GetType();
            var process = RegisteredProcessors.GetOrAdd(type, BuildChildrensIterator);
            if (listProcessor == null)
                listProcessor = DefaultListProcessor;
            var context = new NodeProcessorContext(nodeProcessor, listProcessor);
            return process(node, ref context);
        }

        /// <summary>
        /// Default <see cref="NodeListProcessor"/> implementation used by <see cref="Childrens"/> iterator.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="nodeProcessorContext">The node processor context.</param>
        public static void DefaultListProcessor(IList list, ref NodeProcessorContext nodeProcessorContext)
        {
            if (list != null)
            {
                var nodeProcessor = nodeProcessorContext.NodeProcessor;
                int i = 0;
                while (i < list.Count)
                {
                    var previousValue = (Node)list[i];
                    var temp = nodeProcessor(previousValue, ref nodeProcessorContext);

                    // Recover the position as the list can be modified while processing a node
                    for (i = 0; i < list.Count; i++)
                    {
                        if (ReferenceEquals(previousValue, list[i]))
                            break;
                    }

                    if (temp == null)
                    {
                        list.RemoveAt(i);
                    }  
                    else
                    {
                        if (!ReferenceEquals(previousValue, temp))
                            list[i] = temp;
                        i++;
                    }
                }
            }
        }


        private static FieldInfo FieldReflector<T>(Expression<Func<T,object>> access)
        {
            return (FieldInfo)((MemberExpression)access.Body).Member;
        }

        private static FieldInfo nodeProcessorProperty = FieldReflector<NodeProcessorContext>(obj => obj.NodeProcessor);
        private static FieldInfo nodeListProcessorProperty = FieldReflector<NodeProcessorContext>(obj => obj.ListProcessor);

        /// <summary>
        /// Builds the childrens iterator.
        /// </summary>
        /// <param name="rootType">Type of the root.</param>
        /// <returns>
        /// A function that is able to process childrens from a node
        /// </returns>
        private static NodeProcessor BuildChildrensIterator(Type rootType)
        {
            var sourceParameter = LinqExpression.Parameter(typeof(Node), "source");
            var explorerParameter = LinqExpression.Parameter(typeof(NodeProcessorContext).MakeByRefType(), "nodeProcessorContext");
            var variableNodeProcessor = LinqExpression.Variable(typeof(NodeProcessor), "nodeProcessor");
            var variableNodeListProcessor = LinqExpression.Variable(typeof(NodeListProcessor), "listProcessor");
            var statements = new List<LinqExpression>();

            // Cast source variable
            var castVar = LinqExpression.Variable(rootType, "cast");
            var variables = new List<ParameterExpression> { castVar, variableNodeProcessor, variableNodeListProcessor };

            statements.Add(LinqExpression.Assign(castVar, LinqExpression.Convert(sourceParameter, rootType)));

            statements.Add(LinqExpression.Assign(variableNodeProcessor, LinqExpression.Field(explorerParameter, nodeProcessorProperty)));
            statements.Add(LinqExpression.Assign(variableNodeListProcessor, LinqExpression.Field(explorerParameter, nodeListProcessorProperty)));

            // Get all types from most inherited
            var types = new List<Type>();
            for (var type = rootType; type != null; type = type.GetTypeInfo().BaseType)
                types.Add(type);
            types.Reverse();

            // Iterate on inherited types
            foreach (var type in types)
            {
                // Iterate on all properties in order to create the iterator.
                foreach (var sourceField in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {

                    // If the property is not read-writable or contains a visitor ignore attribute, skip it
                    if (sourceField.GetCustomAttribute<VisitorIgnoreAttribute>(true) != null)
                    {
                        continue;
                    }

                    var propertyType = sourceField.FieldType;

                    var interfaces = propertyType.GetTypeInfo().ImplementedInterfaces;

                    // Get the property type and check if the property inherit from IList<>
                    if (!typeof(StatementList).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
                    {
                        foreach (var interfaceBase in interfaces)
                        {
                            if (interfaceBase.GetTypeInfo().IsGenericType && interfaceBase.GetTypeInfo().GetGenericTypeDefinition() == typeof(IList<>))
                            {
                                var parameterType = interfaceBase.GetTypeInfo().GenericTypeArguments[0];
                                if (typeof(Node).GetTypeInfo().IsAssignableFrom(parameterType.GetTypeInfo()))
                                    statements.Add(
                                        LinqExpression.Invoke(variableNodeListProcessor, LinqExpression.Field(castVar, sourceField), explorerParameter));
                                break;
                            }
                        }
                    }

                    if (typeof(Node).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
                    {
                        statements.Add(
                            LinqExpression.Assign(
                                LinqExpression.Field(castVar, sourceField),
                                LinqExpression.Convert(LinqExpression.Invoke(variableNodeProcessor, LinqExpression.Field(castVar, sourceField), explorerParameter),propertyType)));
                    }
                }
            }

            // Return source parameter
            statements.Add(sourceParameter);

            // Lambda body
            var block = LinqExpression.Block(variables, statements);

            // Create lambda and return a compiled version
            var lambda = LinqExpression.Lambda<NodeProcessor>(block, sourceParameter, explorerParameter);
            return lambda.Compile();
        }
    }
}
