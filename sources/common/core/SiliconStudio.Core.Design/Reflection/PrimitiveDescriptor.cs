// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Describes a descriptor for a primitive (bool, char, sbyte, byte, int, uint, long, ulong, float, double, decimal, string, DateTime).
    /// </summary>
    public class PrimitiveDescriptor : ObjectDescriptor
    {
        private static readonly List<IMemberDescriptor> EmptyMembers = new List<IMemberDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptor" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentException">Type [{0}] is not a primitive</exception>
        public PrimitiveDescriptor(ITypeDescriptorFactory factory, Type type, bool emitDefaultValues, IMemberNamingConvention namingConvention)
            : base(factory, type, emitDefaultValues, namingConvention)
        {
            if (!IsPrimitive(type))
                throw new ArgumentException("Type [{0}] is not a primitive");
        }

        public override DescriptorCategory Category => DescriptorCategory.Primitive;

        /// <summary>
        /// Determines whether the specified type is a primitive.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is primitive; otherwise, <c>false</c>.</returns>
        public static bool IsPrimitive(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                case TypeCode.Empty:
                    return type == typeof(string) || type == typeof(TimeSpan) || type == typeof(DateTime);
            }
            return true;
        }

        protected override List<IMemberDescriptor> PrepareMembers()
        {
            return EmptyMembers;
        }
    }
}
