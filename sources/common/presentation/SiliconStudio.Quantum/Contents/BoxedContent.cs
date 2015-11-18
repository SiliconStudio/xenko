// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Contents
{
    public class BoxedContent : ObjectContent
    {
        public BoxedContent(object value, ITypeDescriptor descriptor, bool isPrimitive)
            : base(value, descriptor, isPrimitive, null)
        {
        }

        public override object Value
        {
            get { return base.Value; }
            set
            {
                SetValue(value);
                if (BoxedStructureOwner != null)
                {
                    if (BoxedStructureOwnerIndices != null)
                    {
                        var currentObj = BoxedStructureOwner.Value;
                        for (int i = 0; i < BoxedStructureOwnerIndices.Length - 1; ++i)
                        {
                            currentObj = FetchItem(currentObj, BoxedStructureOwnerIndices[i]);
                        }
                        SetItem(currentObj, BoxedStructureOwnerIndices[BoxedStructureOwnerIndices.Length - 1], value);
                    }
                    else
                        BoxedStructureOwner.Value = value;
                }
            }
        }

        internal IContent BoxedStructureOwner { get; set; }

        internal object[] BoxedStructureOwnerIndices { get; set; }

        private static object FetchItem(object enumerable, object index)
        {
            var list = enumerable as IList;
            if (list != null && index is int)
                return list[(int)index];

            var dictionary = enumerable as IDictionary;
            if (dictionary != null)
                return dictionary[index];

            var type = enumerable.GetType();
            if (type.HasInterface(typeof(IDictionary<,>)))
            {
                var keyType = type.GetInterface(typeof(IDictionary<,>)).GetGenericArguments()[0];
                var valueType = type.GetInterface(typeof(IDictionary<,>)).GetGenericArguments()[0];
                var indexerMethod = type.GetProperty("Item", valueType, new[] { keyType });
                return indexerMethod.GetValue(enumerable, new [] { index });
            }
            throw new ArgumentException(@"Enumerable object has no indexing and is not supported.", nameof(enumerable));
        }

        private static void SetItem(object enumerable, object index, object value)
        {
            var list = enumerable as IList;
            if (list != null && index is int)
            {
                list[(int)index] = value;
                return;
            }

            var dictionary = enumerable as IDictionary;
            if (dictionary != null)
            {
                dictionary[index] = value;
                return;
            }

            var type = enumerable.GetType();
            if (type.HasInterface(typeof(IDictionary<,>)))
            {
                var keyType = type.GetInterface(typeof(IDictionary<,>)).GetGenericArguments()[0];
                var valueType = type.GetInterface(typeof(IDictionary<,>)).GetGenericArguments()[0];
                var indexerMethod = type.GetProperty("Item", valueType, new[] { keyType });
                indexerMethod.SetValue(enumerable, value, new[] { index });
                return;
            }
            throw new ArgumentException(@"Enumerable object has no indexing and is not supported.", nameof(enumerable));
        }
    }
}
