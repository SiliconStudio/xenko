// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization.Converters
{
    /// <summary>
    /// Generates Data and DataConverter types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DataConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether there is a separate data type to create (default true).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [data type]; otherwise, <c>false</c>.
        /// </value>
        public bool DataType { get; set; }

        /// <summary>
        /// Auto-generate data type and converter.
        /// </summary>
        public bool AutoGenerate { get; set; }

        /// <summary>
        /// This type needs to be embedded in a ContentReference when another Data type refers to it.
        /// </summary>
        public bool ContentReference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ConvertToData will be written by the user.
        /// </summary>
        /// <value>
        /// <c>true</c> if ConvertToData will be written by the user; otherwise, <c>false</c>.
        /// </value>
        public bool CustomConvertToData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ConvertFromData will be written by the user.
        /// </summary>
        /// <value>
        /// <c>true</c> if ConvertFromData will be written by the user; otherwise, <c>false</c>.
        /// </value>
        public bool CustomConvertFromData { get; set; }

        /// <summary>
        /// Gets or sets the name of the data type.
        /// </summary>
        /// <value>
        /// The name of the data type.
        /// </value>
        public string DataTypeName { get; set; }

        public DataConverterAttribute()
        {
            DataType = true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DataAdditionalConverterAttribute : Attribute
    {
        /// <summary>
        /// Auto-generate data converter.
        /// </summary>
        public bool AutoGenerate { get; set; }
        
        public Type BaseType { get; set; }

        public DataAdditionalConverterAttribute(Type baseType)
        {
            this.BaseType = baseType;
        }
    }
}