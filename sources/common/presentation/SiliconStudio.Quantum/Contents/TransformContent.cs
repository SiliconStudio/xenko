// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Gives the ability to transform the member to another type.
    /// </summary>
    public abstract class TransformContent : ContentBase
    {
        protected readonly IContent Content;

        protected TransformContent(INodeBuilder nodeBuilder, ITypeDescriptor descriptor, IContent content, bool isPrimitive, IReference reference = null)
            : base(nodeBuilder, descriptor, isPrimitive, reference)
        {
            Content = content;
            //Content = new MemberContent(nodeBuilder, container, member, isPrimitive, null);
        }

        /// <inheritdoc/>
        public override object Value
        {
            get
            {
                var obj = Content.Value;
                return TransformGet(obj);
            }
            set
            {
                var obj = TransformSet(value);
                Content.Value = obj;
            }
        }

        /// <summary>
        /// Transforms <see cref="Value"/> when reading.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        protected abstract object TransformGet(object obj);

        /// <summary>
        /// Transforms <see cref="Value"/> when writing.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        protected abstract object TransformSet(object obj);
    }
}