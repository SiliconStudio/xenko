// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContent"/> that gives an access to the content value of the parent node of its owner.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ParentValueViewModelContent : ContentBase
    {
        private IViewModelNode ownerNode;

        public ParentValueViewModelContent()
            : base(typeof(object), null, false)
        {
        }

        /// <inheritdoc/>
        public override IViewModelNode OwnerNode
        {
            get { return ownerNode; }
            set { ownerNode = value; Type = ownerNode.Content.Type; }
        }

        /// <inheritdoc/>
        public override object Value
        {
            get { return GetParentContent().Value; }
            set { GetParentContent().Value = value; }
        }

        protected IContent GetParentContent()
        {
            if (OwnerNode == null)
            {
                throw new InvalidOperationException("No OwnerNode is set to this ParentValueViewModelContent.");
            }
            if (OwnerNode.Parent == null)
            {
                throw new InvalidOperationException("The OwnerNode of this ParentValueViewModelContent has no parent.");
            }
            return OwnerNode.Parent.Content;
        }
    }
}