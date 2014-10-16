// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// An attribute to modify the output style of a sequence or mapping. 
    /// This attribute can be apply directly on a type or on a property/field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class DataStyleAttribute : Attribute
    {
        private readonly DataStyle style;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStyleAttribute"/> class.
        /// </summary>
        /// <param name="style">The style.</param>
        public DataStyleAttribute(DataStyle style)
        {
            this.style = style;
        }

        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <value>The style.</value>
        public DataStyle Style
        {
            get { return style; }
        }
    }
}