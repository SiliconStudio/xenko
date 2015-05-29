// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// A list to ensure that all items are not null.
    /// </summary>
    /// <typeparam name="T">Type of the item</typeparam>
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public class SafeList<T> : ConstrainedList<T> where T : class
    {
        private const string ExceptionError = "The item added to the list cannot be null";

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeList{T}"/> class.
        /// </summary>
        public SafeList()
            : base(Constraint, true, ExceptionError)
        {
        }

        private static bool Constraint(ConstrainedList<T> constrainedList, T arg2)
        {
            return arg2 != null;
        }
    }
}