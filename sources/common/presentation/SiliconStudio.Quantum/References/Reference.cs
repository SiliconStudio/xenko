// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Threading;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.References
{
    internal static class Reference
    {
        private static readonly ThreadLocal<int> CreatingReference = new ThreadLocal<int>();

        /// <summary>
        /// A constant value used as index of a reference that is not in a collection.
        /// </summary>
        internal static readonly object NotInCollection = new object();

        internal static IReference CreateReference(object objectValue, Type objectType, object index)
        {
            if (objectValue != null && !objectType.IsInstanceOfType(objectValue)) throw new ArgumentException(@"objectValue type does not match objectType", nameof(objectValue));

            if (!CreatingReference.IsValueCreated)
                CreatingReference.Value = 0;

            ++CreatingReference.Value;

            IReference reference;
            var isCollection = HasCollectionReference(objectValue?.GetType() ?? objectType);
            if (objectValue != null && isCollection && index == NotInCollection)
            {
                reference = new ReferenceEnumerable((IEnumerable)objectValue, objectType, index);
            }
            else
            {
                reference = new ObjectReference(objectValue, objectType, index);
            }

            --CreatingReference.Value;

            return reference;
        }

        private static bool HasCollectionReference(Type type)
        {
            return type.IsArray || CollectionDescriptor.IsCollection(type) || DictionaryDescriptor.IsDictionary(type);
        }


        internal static Type GetReferenceType(object objectValue, object index)
        {
            return objectValue != null && HasCollectionReference(objectValue.GetType()) && index == NotInCollection ? typeof(ReferenceEnumerable) : typeof(ObjectReference);
        }

        internal static void CheckReferenceCreationSafeGuard()
        {
            if (!CreatingReference.IsValueCreated || CreatingReference.Value == 0)
                throw new InvalidOperationException("A reference can only be constructed with the method Reference.CreateReference");
        }
    }
}
