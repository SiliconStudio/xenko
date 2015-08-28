// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes a raw import, as used in project file.
    /// </summary>
    [DataContract("RawImport")]
    [Obsolete]
    public sealed class RawImport
    {
        public RawImport()
        {
            Patterns = new List<string>();
        }

        /// <summary>
        /// Gets or sets the source directory.
        /// </summary>
        /// <value>
        /// The source directory.
        /// </value>
        [DataMember(10)]
        [DefaultValue(null)]
        [UPath(UPathRelativeTo.None)]
        public UDirectory SourceDirectory { get; set; }

        /// <summary>
        /// Gets the source file patterns.
        /// </summary>
        /// <value>
        /// The patterns.
        /// </value>
        [DataMember(20)]
        public List<string> Patterns { get; private set; }

        /// <summary>
        /// Gets or sets the asset target location.
        /// </summary>
        /// <value>
        /// The asset target location.
        /// </value>
        [DataMember(30)]
        [DefaultValue(null)]
        [UPath(UPathRelativeTo.None)]
        public UDirectory TargetLocation { get; set; }
    }
}