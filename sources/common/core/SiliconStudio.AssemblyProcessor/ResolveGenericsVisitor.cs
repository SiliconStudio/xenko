// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Transform open generic types to closed instantiation using context information.
    /// See <see cref="Process"/> for more details.
    /// </summary>
    class ResolveGenericsVisitor : CecilTypeReferenceVisitor
    {
        private Dictionary<TypeReference, TypeReference> genericTypeMapping;

        public ResolveGenericsVisitor(Dictionary<TypeReference, TypeReference> genericTypeMapping)
        {
            this.genericTypeMapping = genericTypeMapping;
        }

        /// <summary>
        /// Transform open generic types to closed instantiation using context information.
        /// As an example, if B{T} inherits from A{T}, running it with B{C} as context and A{B.T} as type, ti will return A{C}.
        /// </summary>
        public static TypeReference Process(TypeReference context, TypeReference type)
        {
            if (type == null)
                return null;

            var genericInstanceTypeContext = context as GenericInstanceType;
            if (genericInstanceTypeContext == null)
                return type;

            if (genericInstanceTypeContext.ContainsGenericParameter())
                return type;

            // Build dictionary that will map generic type to their real implementation type
            var resolvedType = context.Resolve();
            var genericTypeMapping = new Dictionary<TypeReference, TypeReference>();
            for (int i = 0; i < resolvedType.GenericParameters.Count; ++i)
            {
                var genericParameter = context.GetElementType().Resolve().GenericParameters[i];
                genericTypeMapping.Add(genericParameter, genericInstanceTypeContext.GenericArguments[i]);
            }

            var visitor = new ResolveGenericsVisitor(genericTypeMapping);
            var result = visitor.VisitDynamic(type);

            // Make sure type is closed now
            if (result.ContainsGenericParameter())
                throw new InvalidOperationException("Unsupported generic resolution.");

            return result;
        }

        public override TypeReference Visit(GenericParameter type)
        {
            TypeReference result;
            if (genericTypeMapping.TryGetValue(type, out result))
                return result;

            return base.Visit(type);
        }
    }
}