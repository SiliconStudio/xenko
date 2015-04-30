// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Implements <see cref="IContentReference"/> for a <see cref="AttachedReference"/>.
    /// </summary>
    internal class AttachedContentReference : IContentReference
    {
        private readonly AttachedReference attachedReference;

        /// <inheritdoc/>
        public Guid Id { get { return attachedReference.Id; } }

        /// <inheritdoc/>
        public string Location { get { return attachedReference.Url; } }

        public AttachedContentReference(AttachedReference attachedReference)
        {
            this.attachedReference = attachedReference;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0}:{1}", attachedReference.Id, attachedReference.Url);
        }
    }
}