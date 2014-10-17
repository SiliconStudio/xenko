// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization.Converters
{
    /// <summary>
    /// Base class for converters to/from a data type.
    /// </summary>
    public abstract class DataConverter
    {
        /// <summary>
        /// Gets the data type.
        /// </summary>
        /// <value>
        /// The data type.
        /// </value>
        public abstract Type DataType { get; }

        /// <summary>
        /// Gets the source type.
        /// </summary>
        /// <value>
        /// The source type.
        /// </value>
        public abstract Type ObjectType { get; }

        /// <summary>
        /// Gets a value indicating whether converted result should be cached in <see cref="ConverterContext"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result should be cached; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CacheResult
        {
            get { return true; }
        }

        public virtual bool CanConstruct
        {
            get { return false; }
        }

        public virtual void ConstructFromData(ConverterContext converterContext, object data, ref object obj)
        {
        }

        /// <summary>
        /// Converts the given data to its object counterpart.
        /// </summary>
        /// <param name="converterContext">The converter context.</param>
        /// <param name="data">The data.</param>
        /// <param name="obj">The object.</param>
        public abstract void ConvertFromData(ConverterContext converterContext, object data, ref object obj);

        /// <summary>
        /// Converts the given source object to its data counterpart.
        /// </summary>
        /// <param name="converterContext">The converter context.</param>
        /// <param name="data">The data.</param>
        /// <param name="obj">The object.</param>
        public abstract void ConvertToData(ConverterContext converterContext, ref object data, object obj);
    }
}