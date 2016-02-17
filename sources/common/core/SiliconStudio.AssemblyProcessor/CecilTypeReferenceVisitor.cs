// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Visit Cecil types recursively, and replace them if requested.
    /// </summary>
    public class CecilTypeReferenceVisitor
    {
        protected IList<T> VisitDynamicList<T>(IList<T> list) where T : TypeReference
        {
            var result = list;
            for (int i = 0; i < list.Count; i++)
            {
                var item = result[i];

                var newNode = VisitDynamic(item);

                if (newNode == null)
                {
                    if (result == list)
                        result = new List<T>(list);
                    result.RemoveAt(i);
                    i--;
                }
                else if (!ReferenceEquals(newNode, item))
                {
                    if (result == list)
                        result = new List<T>(list);
                    result[i] = (T)newNode;
                }
            }
            return result;
        }

        public virtual TypeReference VisitDynamic(TypeReference type)
        {
            var arrayType = type as ArrayType;
            if (arrayType != null)
                return Visit(arrayType);

            var genericInstanceType = type as GenericInstanceType;
            if (genericInstanceType != null)
                return Visit(genericInstanceType);

            var genericParameter = type as GenericParameter;
            if (genericParameter != null)
                return Visit(genericParameter);

            var pointerType = type as PointerType;
            if (pointerType != null)
                return Visit(pointerType);

            if (type.GetType() != typeof(TypeReference) && type.GetType() != typeof(TypeDefinition))
                throw new NotSupportedException();

            return Visit(type);
        }

        public virtual TypeReference Visit(GenericParameter type)
        {
            return type;
        }

        public virtual TypeReference Visit(PointerType type)
        {
            type = type.ChangePointerType(VisitDynamic(type.ElementType));
            return type.ChangeGenericParameters(VisitDynamicList(type.GenericParameters));
        }

        public virtual TypeReference Visit(TypeReference type)
        {
            return type.ChangeGenericParameters(VisitDynamicList(type.GenericParameters));
        }

        public virtual TypeReference Visit(ArrayType type)
        {
            type = type.ChangeArrayType(VisitDynamic(type.ElementType), type.Rank);
            return type.ChangeGenericParameters(VisitDynamicList(type.GenericParameters));
        }

        public virtual TypeReference Visit(GenericInstanceType type)
        {
            type = type.ChangeGenericInstanceType(VisitDynamic(type.ElementType), VisitDynamicList(type.GenericArguments));
            return type.ChangeGenericParameters(VisitDynamicList(type.GenericParameters));
        }
    }
}