// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Describe a file in a package by giving the <see cref="Source"/> of a file or set of files, the destination <see cref="Target"/> where they will be copied
    /// with some exclude rules <see cref="Exclude"/>.
    /// Both Source and Exclude can use regular expressions.
    /// </summary>
    public class ManifestFile
    {
        /// <summary>
        /// Set of source files that will be copied to <see cref="Target"/>.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Target location where files described by <see cref="Source"/> will be copied.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Rules excluding copies of files from <see cref="Source"/>.
        /// </summary>
        public string Exclude { get; set; }
    }
}