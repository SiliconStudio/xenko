// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Arguments of the <see cref="INodeBuilder.NodeConstructing"/> event.
    /// </summary>
    public  class NodeConstructingArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConstructingArgs"/> class.
        /// </summary>
        /// <param name="containerObjectDescriptor">The descriptor of the container of the member being constructed, or of the object itself it is a root object.</param>
        /// <param name="memberDescriptor">The member descriptor of the object being constructed if it is a member, or <c>null</c> otherwise.</param>
        public NodeConstructingArgs(ObjectDescriptor containerObjectDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (containerObjectDescriptor == null) throw new ArgumentNullException(nameof(containerObjectDescriptor));
            ContainerObjectDescriptor = containerObjectDescriptor;
            MemberDescriptor = memberDescriptor;
        }

        /// <summary>
        /// Gets the descriptor of the container of the member being constructed, or of the object itself it is a root object.
        /// </summary>
        public ObjectDescriptor ContainerObjectDescriptor { get; private set; }

        /// <summary>
        /// Gets the member descriptor of the object being constructed if it is a member, or <c>null</c> otherwise.
        /// </summary>
        public MemberDescriptorBase MemberDescriptor { get; private set; }
    }
}
