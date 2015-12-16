// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// The root node used for storing a hierarchy of <see cref="DataVisitNode"/>
    /// </summary>
    public sealed class DataVisitObject : DataVisitNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitObject" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceDescriptor">The instance descriptor.</param>
        /// <exception cref="System.ArgumentNullException">instance
        /// or
        /// instanceDescriptor</exception>
        public DataVisitObject(object instance, ObjectDescriptor instanceDescriptor) : base(instance, instanceDescriptor)
        {
        }

        public override string ToString()
        {
            return string.Format("{0}", InstanceDescriptor.Type);
        }

        public override void SetValue(object instance)
        {
        }

        public override void RemoveValue()
        {
        }
    }
}