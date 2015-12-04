// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// Base class for all items in a collection (array, list or dictionary)
    /// </summary>
    public abstract class DataVisitNode : IDataVisitNode<DataVisitNode>
    {
        private object instance;
        private readonly ITypeDescriptor instanceDescriptor;
        private readonly Type instanceType;

        internal DataVisitNode(object instance, ITypeDescriptor instanceDescriptor)
        {
            if (instanceDescriptor == null) throw new ArgumentNullException("instanceDescriptor");

            this.instance = instance;
            this.instanceDescriptor = instanceDescriptor;
            this.instanceType = instanceDescriptor.Type;
        }

        public DataVisitNode Parent { get; set; }

        public bool HasMembers
        {
            get
            {
                return Members != null;
            }
        }

        public bool HasItems
        {
            get
            {
                return Items != null;
            }
        }

        public List<DataVisitNode> Members { get; set; }

        public List<DataVisitNode> Items { get; set; }


        public object Instance
        {
            get
            {
                return instance;
            }
            protected set
            {
                instance = value;
            }
        }

        public ITypeDescriptor InstanceDescriptor
        {
            get
            {
                return instanceDescriptor;
            }
        }

        public Type InstanceType
        {
            get
            {
                return instance != null ? instance.GetType() : instanceType;
            }
        }

        public abstract void SetValue(object instance);

        /// <summary>
        /// Removes this node where it is used.
        /// </summary>
        public abstract void RemoveValue();

        public virtual DataVisitNode CreateWithEmptyInstance()
        {
            throw new NotImplementedException();
        }
    }
}