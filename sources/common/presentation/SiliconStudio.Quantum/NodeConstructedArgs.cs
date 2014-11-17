// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Arguments of the <see cref="INodeBuilder.NodeConstructed"/> event.
    /// </summary>
    public class NodeConstructedArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConstructedArgs"/> class.
        /// </summary>
        /// <param name="content">The content of the node that has been constructed.</param>
        public NodeConstructedArgs(IContent content)
        {
            TypeDescriptor = content.Descriptor;
            MemberDescriptor = content is MemberContent ? ((MemberContent)content).Member : null;
        }

        /// <summary>
        /// Gets the type descriptor of the node that has been constructed.
        /// </summary>
        public ITypeDescriptor TypeDescriptor { get; private set; }

        /// <summary>
        /// Gets the member of the node that has been constructed, if the node corresponds to a member of another object.
        /// </summary>
        public IMemberDescriptor MemberDescriptor { get; private set; }
    }
}