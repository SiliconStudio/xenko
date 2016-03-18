// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// An helper class to attach a unique identifier object to runtime objects.
    /// </summary>
    public static class IdentifiableHelper
    {
        /// <summary>
        /// Special member id used to serialize attached id to an object.
        /// </summary>
        public const string YamlSpecialId = "~Id";

        // TODO: Should we reinitialize this when assemblies are reloaded?
        private static readonly Dictionary<Type, bool> IdentifiableTypes = new Dictionary<Type, bool>();

        public static bool IsIdentifiable(Type type)
        {
            bool result;
            lock (IdentifiableTypes)
            {
                if (!IdentifiableTypes.TryGetValue(type, out result))
                {
                    var nonIdentifiable = type.GetTypeInfo().GetCustomAttribute<NonIdentifiableAttribute>();

                    // Early exit if we don't need to add a unique identifier to a type
                    result = !( type == typeof(string)
                            || type.GetTypeInfo().IsValueType
                            || type.GetTypeInfo().IsArray
                            || TypeHelper.IsCollection(type)
                            || TypeHelper.IsDictionary(type)
                            || nonIdentifiable != null);

                    IdentifiableTypes.Add(type, result);
                }
            }
            return result;
        }

        public static bool TryGetId(object instance, out Guid id)
        {
            var shadow = ShadowObject.Get(instance);
            if (shadow == null)
            {
                id = Guid.Empty;
                return false;
            }
            id = shadow.GetId(instance);
            return true;
        }

        public static Guid GetId(object instance)
        {
            var shadow = ShadowObject.GetOrCreate(instance);
            return shadow.GetId(instance);
        }

        public static void SetId(object instance, Guid id)
        {
            var shadow = ShadowObject.GetOrCreate(instance);
            shadow?.SetId(instance, id);
        }
    }
}