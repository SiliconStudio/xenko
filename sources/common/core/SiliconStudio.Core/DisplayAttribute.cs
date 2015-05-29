// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core
{
    // ReSharper disable once CSharpWarnings::CS1584
    /// <summary>
    /// Portable DisplayAttribute equivalent to <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>.
    /// </summary>
    public class DisplayAttribute : Attribute
    {
        private static readonly Dictionary<MemberInfo, DisplayAttribute> RegisteredDisplayAttributes = new Dictionary<MemberInfo, DisplayAttribute>();

        private readonly int? order;

        private readonly string name;

        private readonly string category;

        private readonly string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="order">The order weight of the column.</param>
        /// <param name="name">A value that is used for display in the UI..</param>
        /// <param name="description">A value that is used to display a description in the UI..</param>
        /// <param name="category">A value that is used to group fields in the UI..</param>
        public DisplayAttribute(int order, string name = null, string description = null, string category = null)
            : this(name, description, category)
        {
            this.order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class.
        /// </summary>
        /// <param name="name">A value that is used for display in the UI..</param>
        /// <param name="description">A value that is used to display a description in the UI.</param>
        /// <param name="category">A value that is used to group fields in the UI..</param>
        public DisplayAttribute(string name = null, string description = null, string category = null)
        {
            this.name = name;
            this.category = category;
            this.description = description;
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

        /// <summary>
        /// Gets or sets a value indicating whether the property must be expanded by default in the editor.
        /// </summary>
        /// <value><c>true</c> if [automatic expand]; otherwise, <c>false</c>.</value>
        public bool AlwaysExpand { get; set; }

        /// <summary>
        /// Gets the display attribute attached to the specified member info.
        /// </summary>
        /// <param name="memberInfo">Member type (Property, Field or Type).</param>
        /// <returns>DisplayAttribute.</returns>
        /// <exception cref="System.ArgumentNullException">memberInfo</exception>
        public static DisplayAttribute GetDisplay(MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException("memberInfo");
            lock (RegisteredDisplayAttributes)
            {
                DisplayAttribute value;
                if (!RegisteredDisplayAttributes.TryGetValue(memberInfo, out value))
                {
                    value = memberInfo.GetCustomAttribute<DisplayAttribute>() ?? new DisplayAttribute(memberInfo.Name, string.Format("Description of {0}", memberInfo.Name));
                    RegisteredDisplayAttributes.Add(memberInfo, value);
                }
                return value;
            }
        }

        public static int? GetOrder(MemberInfo memberInfo)
        {
            var display = GetDisplay(memberInfo);
            return display.Order;
        }
    }
}