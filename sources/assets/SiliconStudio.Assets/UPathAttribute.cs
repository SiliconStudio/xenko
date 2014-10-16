// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{

    /// <summary>
    /// Enum UPathRelativeTo
    /// </summary>
    public enum UPathRelativeTo
    {
        /// <summary>
        /// The UPath is stored as-is without post-processing
        /// </summary>
        None,

        /// <summary>
        /// The UPath is stored in relative mode when storing on the disk and relative to the current package.
        /// </summary>
        Package,
    }

    /// <summary>
    /// Specifies how to normalize a UPath stored in a class after loading/saving an asset.
    /// </summary>
    public class UPathAttribute : Attribute
    {
        private readonly UPathRelativeTo relativeTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UPathAttribute"/> class.
        /// </summary>
        /// <param name="relativeTo">The relative to.</param>
        public UPathAttribute(UPathRelativeTo relativeTo)
        {
            this.relativeTo = relativeTo;
        }

        /// <summary>
        /// Gets how to normalize the path relative to.
        /// </summary>
        /// <value>The relative to.</value>
        public UPathRelativeTo RelativeTo
        {
            get
            {
                return relativeTo;
            }
        }
    }
}