// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Specify the way to store a property or field of some class or structure.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class DataMemberAttribute : Attribute
    {
        private readonly DataMemberMode mode;
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public DataMemberAttribute(int order)
        {
            Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public DataMemberAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Specify the way to store a property or field of some class or structure.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mode">The serialize method.</param>
        public DataMemberAttribute(string name, DataMemberMode mode)
        {
            this.name = name;
            this.mode = mode;
        }

        /// <summary>
        /// Specify the way to store a property or field of some class or structure.
        /// </summary>
        /// <param name="mode">The serialize method.</param>
        public DataMemberAttribute(DataMemberMode mode)
        {
            this.mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="mode">The mode.</param>
        public DataMemberAttribute(int order, DataMemberMode mode)
        {
            Order = order;
            this.mode = mode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="name">The name.</param>
        /// <param name="mode">The mode.</param>
        public DataMemberAttribute(int order, string name, DataMemberMode mode)
        {
            Order = order;
            this.name = name;
            this.mode = mode;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the serialize method1.
        /// </summary>
        /// <value>The serialize method1.</value>
        public DataMemberMode Mode
        {
            get { return mode; }
        }


        /// <summary>
        /// Gets or sets the order. Default is -1 (default to alphabetical)
        /// </summary>
        /// <value>The order.</value>
        public int? Order { get; set; }
    }
}