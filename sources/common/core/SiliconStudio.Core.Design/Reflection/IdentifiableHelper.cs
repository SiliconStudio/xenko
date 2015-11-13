// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// An helper class to attach a unique identifier object to runtime objects.
    /// </summary>
    public static class IdentifiableHelper
    {
        // TODO: Should we reinitialize this when assemblies are reloaded?
        private static readonly Dictionary<Type, bool> IdentifiableTypes = new Dictionary<Type, bool>();

        public static bool IsIdentifiable(Type type)
        {
            bool result;
            lock (IdentifiableTypes)
            {
                if (!IdentifiableTypes.TryGetValue(type, out result))
                {
                    var attributes = TypeDescriptorFactory.Default.AttributeRegistry.GetAttributes(type);

                    // Early exit if we don't need to add a unique identifier to a type
                    result = !(type.IsValueType
                            || type.IsArray
                            || CollectionDescriptor.IsCollection(type)
                            || DictionaryDescriptor.IsDictionary(type)
                            || attributes.OfType<NonIdentifitableAttribute>().Any());

                    IdentifiableTypes.Add(type, result);
                }
            }
            return result;
        }

        public static Guid GetId(object instance)
        {
            var shadow = ShadowObject.GetShadow(instance);
            if (shadow == null)
            {
                return Guid.Empty;
            }
            return shadow.GetId(instance);
        }

        public static void SetId(object instance, Guid id)
        {
            var shadow = ShadowObject.GetShadow(instance);
            shadow?.SetId(instance, id);
        }
    }
}