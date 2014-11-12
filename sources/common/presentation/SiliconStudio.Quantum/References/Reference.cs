// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;

using SiliconStudio.Core;

namespace SiliconStudio.Quantum.References
{
    internal static class Reference
    {
        private static int creatingReference;

        /// <summary>
        /// A constant value used as index of a reference that is not in a collection.
        /// </summary>
        internal static readonly object NotInCollection = new object();

        internal static IReference CreateReference(object objectValue, Type objectType, object index)
        {
            if (objectValue != null && !objectType.IsInstanceOfType(objectValue)) throw new ArgumentException(@"objectValue type does not match objectType", "objectValue");

            ++creatingReference;

            IReference reference;
            var enumerableValue = objectValue as IEnumerable;

            if (enumerableValue != null && index == NotInCollection)
            {
                reference = new ReferenceEnumerable(enumerableValue, objectType, index);
            }
            else
            {
                reference = new ObjectReference(objectValue, objectType, index);
            }

            --creatingReference;

            return reference;
        }

        internal static Type GetReferenceType(object objectValue, object index)
        {
            return objectValue is IEnumerable && index == NotInCollection ? typeof(ReferenceEnumerable) : typeof(ObjectReference);
        }

        internal static void CheckReferenceCreationSafeGuard()
        {
            if (creatingReference == 0) throw new InvalidOperationException("A reference can only be constructed with the method Reference.CreateReference");
        }
    }
}
