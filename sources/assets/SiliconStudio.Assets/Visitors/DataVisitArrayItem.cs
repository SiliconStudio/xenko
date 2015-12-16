// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// Defines an item in an array.
    /// </summary>
    public sealed class DataVisitArrayItem : DataVisitNode
    {
        private readonly int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitArrayItem"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <param name="itemDescriptor">The item descriptor.</param>
        public DataVisitArrayItem(int index, object item, ITypeDescriptor itemDescriptor) : base(item, itemDescriptor)
        {
            this.index = index;
        }

        /// <summary>
        /// Gets the array.
        /// </summary>
        /// <value>The array.</value>
        public Array Array
        {
            get
            {
                return (Array)(Parent != null ? Parent.Instance : null);
            }
        }

        /// <summary>
        /// Gets the descriptor.
        /// </summary>
        /// <value>The descriptor.</value>
        public ArrayDescriptor Descriptor
        {
            get
            {
                return (ArrayDescriptor)(Parent != null ? Parent.InstanceDescriptor : null);
            }
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index
        {
            get
            {
                return index;
            }
        }

        public override void SetValue(object instance)
        {
            var array = Array;
            if (array != null)
            {
                array.SetValue(instance, index);
            }
        }

        public override void RemoveValue()
        {
            SetValue(null);
        }

        public override string ToString()
        {
            return string.Format("[{0}] = {1}", index, Instance ?? "null");
        }
    }
}