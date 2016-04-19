// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Defines a normalized directory path. See <see cref="UPath"/> for details. This class cannot be inherited.
    /// </summary>
    [DataContract("UDirectory")]
    [TypeConverter(typeof(UDirectoryTypeConverter))]
    public sealed class UDirectory : UPath
    {
        /// <summary>
        /// An empty directory.
        /// </summary>
        public static readonly UDirectory Empty = new UDirectory(string.Empty);

        /// <summary>
        /// A this '.' directory.
        /// </summary>
        public static readonly UDirectory This = new UDirectory(".");

        /// <summary>
        /// Initializes a new instance of the <see cref="UDirectory"/> class.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public UDirectory(string directoryPath) : base(directoryPath, true)
        {
        }

        internal UDirectory(string fullPath, StringSpan driveSpan, StringSpan directorySpan) : base(fullPath, driveSpan, directorySpan)
        {
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        /// <returns>The name of the directory.</returns>
        public string GetDirectoryName()
        {
            var directory = GetDirectory();
            if (directory == null)
                return string.Empty;

            var index = directory.IndexOfReverse(DirectorySeparatorChar);
            return index > 0 ? directory.Substring(Math.Min(index + 1, directory.Length)) : string.Empty;
        }

        /// <summary>
        /// Makes this instance relative to the specified anchor directory.
        /// </summary>
        /// <param name="anchorDirectory">The anchor directory.</param>
        /// <returns>A relative path of this instance to the anchor directory.</returns>
        public new UDirectory MakeRelative(UDirectory anchorDirectory)
        {
            return (UDirectory)base.MakeRelative(anchorDirectory);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="UPath"/>.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator UDirectory(string fullPath)
        {
            return fullPath != null ? new UDirectory(fullPath) : null;
        }

        /// <summary>
        /// Determines whether this directory contains the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if this directory contains the specified path; otherwise, <c>false</c>.</returns>
        public bool Contains(UPath path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (FullPath == null) return false;
            if (path.FullPath == null) return false;

            return path.FullPath.StartsWith(FullPath, StringComparison.OrdinalIgnoreCase) && path.FullPath.Length > FullPath.Length && path.FullPath[FullPath.Length] == DirectorySeparatorChar;
        }
    }
}
