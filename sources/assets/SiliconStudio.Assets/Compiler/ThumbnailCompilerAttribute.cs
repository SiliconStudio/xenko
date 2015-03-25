// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define for a thumbnail compiler for an <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ThumbnailCompilerAttribute : CompilerAttribute
    {
        private readonly bool dynamicThumbnails;

        public ThumbnailCompilerAttribute(Type type, bool dynamicThumbnails = false)
            : base(type)
        {
            this.dynamicThumbnails = dynamicThumbnails;
        }

        public ThumbnailCompilerAttribute(string typeName, bool dynamicThumbnails = false)
            : base(typeName)
        {
            this.dynamicThumbnails = dynamicThumbnails;
        }

        /// <summary>
        /// Gets or sets the priority of this thumbnail.
        /// </summary>
        /// <value>
        /// The priority of this thumbnail.
        /// </value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets whether the thumbnails of the asset type are dynamic and should be regenerated each time a property changes.
        /// </summary>
        public bool DynamicThumbnails
        {
            get
            {
                return dynamicThumbnails;
            }
        }
    }
}