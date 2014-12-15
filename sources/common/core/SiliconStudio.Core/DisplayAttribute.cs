// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Portable DisplayAttribute equivalent to <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>.
    /// </summary>
    public class DisplayAttribute : Attribute
    {
        private readonly int? order;

        private readonly string name;

        private readonly string category;

        private readonly string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="order">The order weight of the column.</param>
        /// <param name="name">A value that is used for display in the UI..</param>
        /// <param name="category">A value that is used to group fields in the UI..</param>
        /// <param name="description">A value that is used to display a description in the UI..</param>
        public DisplayAttribute(int? order, string name = null, string category = null, string description = null)
        {
            this.order = order;
            this.name = name;
            this.category = category;
            this.description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="name">A value that is used for display in the UI..</param>
        /// <param name="category">A value that is used to group fields in the UI..</param>
        /// <param name="description">A value that is used to display a description in the UI.</param>
        public DisplayAttribute(string name = null, string category = null, string description = null) : this(null, name, category, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="name">A value that is used for display in the UI..</param>
        /// <param name="description">A value that is used to display a description in the UI.</param>
        public DisplayAttribute(string name, string description)
            : this(null, name, null, description)
        {
        }

        /// <summary>
        /// Gets the order weight of the column.
        /// </summary>
        /// <value>The order.</value>
        public int? Order
        {
            get
            {
                return order;
            }
        }

        /// <summary>
        /// Gets a value that is used for display in the UI.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Gets a value that is used to group fields in the UI.
        /// </summary>
        /// <value>The category.</value>
        public string Category
        {
            get
            {
                return category;
            }
        }

        /// <summary>
        /// Gets a value that is used to display a description in the UI.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return description;
            }
        }

        public static bool IsDisplayable(object obj)
        {
            return IsDisplayable(obj.GetType());
        }

        public static bool IsDisplayable(Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes(typeof(DisplayAttribute), true).GetEnumerator().MoveNext();
        }

        public static DisplayAttribute GetDisplay(Type type)
        {
            var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(DisplayAttribute), true);
            return attributes.FirstOrDefault() as DisplayAttribute;
        }

        public static DisplayAttribute GetDisplay(MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(DisplayAttribute), true);
            return attributes.FirstOrDefault() as DisplayAttribute;
        }
    }
}