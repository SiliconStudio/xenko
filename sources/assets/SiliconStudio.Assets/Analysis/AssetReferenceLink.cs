// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Updatable reference link returned by <see cref="AssetReferenceAnalysis.Visit"/>.
    /// </summary>
    [DebuggerDisplay("{Path}")]
    public class AssetReferenceLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetReferenceLink" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="reference">The reference.</param>
        /// <param name="updateReference">The update reference.</param>
        public AssetReferenceLink(MemberPath path, object reference, Func<Guid?, string, object> updateReference)
        {
            Path = path;
            this.reference = reference;
            this.updateReference = updateReference;
        }

        /// <summary>
        /// The path to the member holding this reference.
        /// </summary>
        public readonly MemberPath Path;

        /// <summary>
        /// A <see cref="IReference"/> or <see cref="UFile"/>.
        /// </summary>
        public object Reference
        {
            get
            {
                return reference;
            }
        }

        /// <summary>
        /// Updates the reference.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="location">The location.</param>
        public void UpdateReference(Guid? guid, string location)
        {
            reference = updateReference(guid, location);
        }

        /// <summary>
        /// A specialized method to update the reference (guid, and location).
        /// </summary>
        private readonly Func<Guid?, string, object> updateReference;

        private object reference;
    }
}
