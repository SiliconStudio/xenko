// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Base class that describes a uniform path and provides method to manipulate them. Concrete class are <see cref="UFile"/> and <see cref="UDirectory"/>.
    /// This class is immutable and its descendants are immutable. See remarks.
    /// </summary>
    /// <remarks>
    /// <para>A uniform path contains only characters '/' to separate directories and doesn't contain any successive
    /// '/' or './'. This class is used to represent a path, relative or absolute to a directory or filename.</para>
    /// <para>This class can be used to represent uniforms paths both on windows or unix platforms</para>
    /// TODO Provide more documentation on how to use this class
    /// </remarks>
    public abstract class UPath : IEquatable<UPath>, IComparable
    {
        private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        private readonly int hashCode;

        protected readonly StringSpan DriveSpan;

        protected readonly StringSpan DirectorySpan;

        protected readonly StringSpan NameSpan;

        protected readonly StringSpan ExtensionSpan;

        /// <summary>
        /// The directory separator char '/' used to separate directory in an url. 
        /// </summary>
        public const char DirectorySeparatorChar = '/';

        /// <summary>
        /// The directory separator char '\' used to separate directory in an url. 
        /// </summary>
        public const char DirectorySeparatorCharAlt = '\\';

        /// <summary>
        /// The directory separator string '/' used to separate directory in an url. 
        /// </summary>
        public const string DirectorySeparatorString = "/";

        /// <summary>
        /// The directory separator string '\' used to separate directory in an url. 
        /// </summary>
        public const string DirectorySeparatorStringAlt = "\\";

        /// <summary>
        /// Initializes a new instance of the <see cref="UPath" /> class from a file path.
        /// </summary>
        /// <param name="filePath">The full path to a file.</param>
        /// <param name="isDirectory">if set to <c>true</c> the filePath is considered as a directory and not a filename.</param>
        internal UPath(string filePath, bool isDirectory)
        {
            if (!isDirectory && filePath != null && (filePath.EndsWith(DirectorySeparatorString) || filePath.EndsWith(DirectorySeparatorStringAlt) || filePath.EndsWith(Path.VolumeSeparatorChar)))
            {
                throw new ArgumentException("A file path cannot end with with directory char '\\' or '/' ");
            }

            FullPath = Decode(filePath, isDirectory, out DriveSpan, out DirectorySpan, out NameSpan, out ExtensionSpan);
            hashCode = ComputeStringHashCodeCaseInsensitive(FullPath);
        }

        protected UPath(string fullPath, StringSpan driveSpan, StringSpan directorySpan)
        {
            if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
            FullPath = fullPath;
            hashCode = ComputeStringHashCodeCaseInsensitive(fullPath);
            DriveSpan = driveSpan;
            DirectorySpan = directorySpan;
        }

        /// <summary>
        /// Gets the full path ((drive?)(directory?/)(name.ext?)). An empty path is an empty string.
        /// </summary>
        /// <value>The full path.</value>
        /// <remarks>This property cannot be null.</remarks>
        public string FullPath { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has a <see cref="Drive"/> != null.
        /// </summary>
        /// <value><c>true</c> if this instance has drive; otherwise, <c>false</c>.</value>
        public bool HasDrive => DriveSpan.IsValid;

        /// <summary>
        /// Gets a value indicating whether this instance has a <see cref="GetDirectory()"/> != null;
        /// </summary>
        /// <value><c>true</c> if this instance has directory; otherwise, <c>false</c>.</value>
        public bool HasDirectory => !IsFile || NameSpan.Start > 0;

        /// <summary>
        /// Gets a value indicating whether this location is a relative location.
        /// </summary>
        /// <value><c>true</c> if this instance is relative; otherwise, <c>false</c>.</value>
        public bool IsRelative => !IsAbsolute;

        /// <summary>
        /// Determines whether this instance is absolute.
        /// </summary>
        /// <returns><c>true</c> if this instance is absolute; otherwise, <c>false</c>.</returns>
        public bool IsAbsolute => HasDrive || (DirectorySpan.IsValid && FullPath[DirectorySpan.Start] == DirectorySeparatorChar);

        /// <summary>
        /// Gets a value indicating whether this instance is a location to a file. Can be null.
        /// </summary>
        /// <value><c>true</c> if this instance is file; otherwise, <c>false</c>.</value>
        public bool IsFile => NameSpan.IsValid || ExtensionSpan.IsValid;

        /// <summary>
        /// Gets the type of the path (absolute or relative).
        /// </summary>
        /// <value>The type of the path.</value>
        public UPathType PathType => IsAbsolute ? UPathType.Absolute : UPathType.Relative;

        /// <summary>
        /// Indicates whether the specified <see cref="UPath"/> is null or empty.
        /// </summary>
        /// <param name="path">The path to test</param>
        /// <returns><c>true</c> if the value parameter is null or empty, otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty(UPath path)
        {
            return string.IsNullOrEmpty(path?.FullPath);
        }

        /// <summary>
        /// Gets the drive (contains the ':' if any), can be null.
        /// </summary>
        /// <returns>The drive.</returns>
        public string GetDrive()
        {
            return DriveSpan.IsValid ? FullPath.Substring(DriveSpan) : null;
        }

        /// <summary>
        /// Gets the directory. Can be null.
        /// </summary>
        /// <returns>The directory.</returns>
        [Obsolete("This method is obsolete. Use GetFullDirectory")]
        public string GetDirectory()
        {
            return DirectorySpan.IsValid ? FullPath.Substring(DirectorySpan) : null;
        }

        /// <summary>
        /// Gets the parent directory of this instance. For a file, this is the directory directly containing the file. 
        /// For a directory, this is the parent directory.
        /// </summary>
        /// <returns>The parent directory or <see cref="UDirectory.Empty"/> if no directory found.</returns>
        public UDirectory GetParent()
        {
            if (IsFile)
            {
                if (DirectorySpan.IsValid)
                {
                    return new UDirectory(FullPath.Substring(0, DirectorySpan.Next), DriveSpan, DirectorySpan);
                }
                if (DriveSpan.IsValid)
                {
                    return new UDirectory(FullPath.Substring(DriveSpan), DriveSpan, new StringSpan());
                }
            } 
            else if (DirectorySpan.IsValid)
            {
                if (DirectorySpan.Length > 1)
                {
                    var index = FullPath.IndexOfReverse(DirectorySeparatorChar);
                    if (index >= 0)
                    {
                        index = index == 0 ? index + 1 : index;
                        return new UDirectory(FullPath.Substring(0, index), DriveSpan, new StringSpan(DirectorySpan.Start, index - DirectorySpan.Start));
                    }
                }
                if (DriveSpan.IsValid)
                {
                    return new UDirectory(FullPath.Substring(DriveSpan), DriveSpan, new StringSpan());
                }
            }

            return UDirectory.Empty;
        }

        /// <summary>
        /// Gets the full directory with <see cref="GetDrive()"/> + <see cref="GetDirectory()"/> or empty directory.
        /// </summary>
        /// <returns>System.String.</returns>
        public UDirectory GetFullDirectory()
        {
            if (IsFile)
            {
                // No directory in this path
                if (NameSpan.Start == 0)
                    return new UDirectory(null);
                // This path only contains a leading '/', we should return it
                if (NameSpan.Start == 1)
                    return new UDirectory("/", DriveSpan, DirectorySpan);

                // Return the path until the name, excluding the last '/'
                var subPath = FullPath.Substring(0, NameSpan.Start - 1);
                return new UDirectory(subPath, DriveSpan, DirectorySpan);
            }
            return this is UDirectory ? (UDirectory)this : new UDirectory(null);
        }

        public bool Equals(UPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is UPath && obj.GetType() == GetType() && Equals((UPath)obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        private static int ComputeStringHashCodeCaseInsensitive(string text)
        {
            return text.Aggregate(0, (current, t) => (current*397) ^ char.ToLowerInvariant(t));
        }

        public int CompareTo(object obj)
        {
            var uPath = obj as UPath;
            if (uPath != null)
            {
                if (FullPath != null && uPath.FullPath != null)
                {
                    return String.Compare(FullPath, uPath.FullPath, StringComparison.OrdinalIgnoreCase);
                }
            }
            return 0;
        }

        public override string ToString()
        {
            return FullPath;
        }

        /// <summary>
        /// Converts this path to a Windows path (/ replaced by \)
        /// </summary>
        /// <returns>A string representation of this path in windows form.</returns>
        public string ToWindowsPath()
        {
            return FullPath.Replace('/', '\\');
        }

        /// <summary>
        /// Implements the ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(UPath left, UPath right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(UPath left, UPath right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Combines the specified left uniform location and right location and return a new <see cref="UPath"/>
        /// </summary>
        /// <param name="leftPath">The left path.</param>
        /// <param name="rightPath">The right path.</param>
        /// <returns>The combination of both paths.</returns>
        public static T Combine<T>(UDirectory leftPath, T rightPath) where T : UPath
        {
            if (leftPath == null) throw new ArgumentNullException(nameof(leftPath));
            if (rightPath == null) throw new ArgumentNullException(nameof(rightPath));

            // If right path is absolute, return it directly
            if (rightPath.IsAbsolute)
            {
                return rightPath;
            }

            var path = $"{leftPath.FullPath}{(string.IsNullOrEmpty(leftPath.FullPath) ? string.Empty : DirectorySeparatorString)}{rightPath.FullPath}";
            return rightPath is UFile ? (T)(object)new UFile(path) : (T)(object)new UDirectory(path);
        }

        /// <summary>
        /// Makes this instance relative to the specified anchor directory.
        /// </summary>
        /// <param name="anchorDirectory">The anchor directory.</param>
        /// <returns>A relative path of this instance to the anchor directory.</returns>
        public UPath MakeRelative(UDirectory anchorDirectory)
        {
            if (anchorDirectory == null) throw new ArgumentNullException(nameof(anchorDirectory));

            // If the toRelativize path is already relative, don't bother
            if (IsRelative)
            {
                return this;
            }

            // If anchor directory is not absolute directory, throw an error
            if (!anchorDirectory.IsAbsolute)
            {
                throw new ArgumentException(@"Expecting an absolute directory", nameof(anchorDirectory));
            }

            if (anchorDirectory.HasDrive != HasDrive)
            {
                throw new InvalidOperationException("Path should have no drive information/or both drive information simultaneously");
            }

            // Return a "." when the directory is the same
            if (this is UDirectory && anchorDirectory == this)
            {
                return UDirectory.This;
            }

            // Get the full path of the anchor directory
            var anchorPath = anchorDirectory.FullPath;

            // Builds an absolute path for the toRelative path (directory-only)
            var absoluteFile = Combine(anchorDirectory, this);
            var absolutePath = absoluteFile.GetFullDirectory().FullPath;

            var relativePath = new StringBuilder();

            int index = anchorPath.Length;
            bool foundCommonRoot = false;
            for (; index >= 0; index--)
            {
                // Need to be a directory separator or end of string
                if (!((index == anchorPath.Length || anchorPath[index] == DirectorySeparatorChar)))
                    continue;

                // Absolute path needs to also have a directory separator at the same location (or end of string)
                if (index == absolutePath.Length || (index < absolutePath.Length && absolutePath[index] == DirectorySeparatorChar))
                {
                    if (string.Compare(anchorPath, 0, absolutePath, 0, index, true) == 0)
                    {
                        foundCommonRoot = true;
                        break;
                    }
                }

                relativePath.Append("..").Append(DirectorySeparatorChar);
            }

            if (!foundCommonRoot)
            {
                return this;
            }

            if (index < absolutePath.Length && absolutePath[index] == DirectorySeparatorChar)
            {
                index++;
            }

            relativePath.Append(absolutePath.Substring(index));
            if (absoluteFile is UFile)
            {
                // If not empty, add a separator
                if (relativePath.Length > 0)
                    relativePath.Append(DirectorySeparatorChar);

                // Add filename
                relativePath.Append(((UFile)absoluteFile).GetFileNameWithExtension());
            }
            var newPath = relativePath.ToString();
            return !IsFile ? (UPath)new UDirectory(newPath) : new UFile(newPath);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UPath"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(UPath url)
        {
            return url?.FullPath;
        }

        /// <summary>
        /// Determines whether the specified path contains some directory characeters '\' or '/'
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified path contains some directory characeters '\' or '/'; otherwise, <c>false</c>.</returns>
        public static bool HasDirectoryChars(string path)
        {
            return (path != null && (path.Contains(DirectorySeparatorChar) || path.Contains(DirectorySeparatorCharAlt)));
        }

        /// <summary>
        /// Determines whether the specified path is a valid <see cref="UPath"/>
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified path is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValid(string path)
        {
            string error;
            Normalize(path, out error);
            return error == null;
        }

        /// <summary>
        /// Normalize a path by replacing '\' by '/' and transforming relative '..' or current path '.' to an absolute path. See remarks.
        /// </summary>
        /// <param name="pathToNormalize">The path automatic normalize.</param>
        /// <returns>A normalized path.</returns>
        /// <exception cref="System.ArgumentException">If path is invalid</exception>
        /// <remarks>Unlike <see cref="System.IO.Path" /> , this doesn't make a path absolute to the actual file system.</remarks>
        public static string Normalize(string pathToNormalize)
        {
            string error;
            var result = Normalize(pathToNormalize, out error);
            if (error != null)
            {
                throw new ArgumentException(error, nameof(pathToNormalize));
            }
            return result.ToString();
        }

        /// <summary>
        /// Normalize a path by replacing '\' by '/' and transforming relative '..' or current path '.' to an absolute path. See remarks.
        /// </summary>
        /// <param name="pathToNormalize">The path automatic normalize.</param>
        /// <param name="error">The error or null if no errors.</param>
        /// <returns>A normalized path or null if there is an error.</returns>
        /// <remarks>Unlike <see cref="System.IO.Path" /> , this doesn't make a path absolute to the actual file system.</remarks>
        public static StringBuilder Normalize(string pathToNormalize, out string error)
        {
            StringSpan drive;
            StringSpan directoryOrFileName;
            StringSpan fileName;
            return Normalize(pathToNormalize, out drive, out directoryOrFileName, out fileName, out error);
        }

        /// <summary>
        /// Normalize a path by replacing '\' by '/' and transforming relative '..' or current path '.' to an absolute path. See remarks.
        /// </summary>
        /// <param name="pathToNormalize">The path automatic normalize.</param>
        /// <param name="drive">The drive character region.</param>
        /// <param name="directoryOrFileName">The directory.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="error">The error or null if no errors.</param>
        /// <returns>A normalized path or null if there is an error.</returns>
        /// <remarks>Unlike <see cref="System.IO.Path" /> , this doesn't make a path absolute to the actual file system.</remarks>
        public static unsafe StringBuilder Normalize(string pathToNormalize, out StringSpan drive, out StringSpan directoryOrFileName, out StringSpan fileName, out string error)
        {
            drive = new StringSpan();
            directoryOrFileName = new StringSpan();
            fileName = new StringSpan();
            error = null;
            string path = pathToNormalize;
            if (path == null)
            {
                return null;
            }
            int countDirectories = pathToNormalize.Count(pathItem => pathItem == DirectorySeparatorChar ||
                                                                     pathItem == DirectorySeparatorCharAlt ||
                                                                     pathItem == Path.VolumeSeparatorChar);

            // Safeguard if count directories is going wild
            if (countDirectories > 1024)
            {
                error = "Path contains too many directory '/' separator or ':'";
                return null;
            }

            // Optimize the code by using stack alloc in order to avoid allocation of a List<CharRegion>()
            int currentPath = 0;
            var paths = stackalloc StringSpan[countDirectories + 1];
            var builder = new StringBuilder(pathToNormalize.Length);

            // Iterate on all chars on original path
            foreach (var pathItem in pathToNormalize)
            {
                // Check if we have a directory separator
                if (pathItem == DirectorySeparatorChar || pathItem == DirectorySeparatorCharAlt)
                {
                    // Add only non consecutive '/'
                    if (builder.Length == 0 || builder[builder.Length - 1] != DirectorySeparatorChar)
                    {
                        // If '/' is the first char in the path, place the start at position 1 instead of 0
                        if (builder.Length == 0)
                        {
                            paths[0].Start = 1;
                        }

                        // if '/' is closing a '..' or '.', then handle them here and go to next char
                        if (TrimParentAndSelfPath(builder, ref currentPath, paths, false))
                        {
                            continue;
                        }

                        // Append the directory '/' separator
                        builder.Append(DirectorySeparatorChar);

                        // Stack a new path entry
                        currentPath++;

                        // Next entry start right after '/'
                        paths[currentPath].Start = builder.Length;
                    }
                }
                else if (pathItem == Path.VolumeSeparatorChar)
                {
                    // Check in case of volume separator ':'
                    if (IsDriveSpan(paths[0]))
                    {
                        error = "Path contains more than one drive ':' separator";
                        return null;
                    }

                    if (currentPath > 0)
                    {
                        error = "Path cannot contain a drive ':' separator after a backslash";
                        return null;
                    }

                    if (builder.Length == 0)
                    {
                        error = "Path cannot start with a drive ':' separator";
                        return null;
                    }

                    // Append the volume ':' if no error
                    builder.Append(pathItem);

                    // Update the path entry
                    paths[0].Length = -paths[0].Length;  // Use of a negative length to identify a drive information

                    // Next entry right after ':'
                    paths[1].Start = builder.Length;
                    currentPath = 1;
                }
                else if (!InvalidFileNameChars.Contains(pathItem))
                {
                    if (currentPath == 1 && IsDriveSpan(paths[0]))
                    {
                        error = @"Path must contain a separator '/' or '\' after the volume separator ':'";
                        return null;
                    }

                    // If no invalid character, we can add the current character
                    builder.Append(pathItem);
                    paths[currentPath].Length++;
                }
                else
                {
                    // Else the character is invalid
                    error = "Invalid character [{0}] found in path [{1}]".ToFormat(pathItem, pathToNormalize);
                    return null;
                }
            }

            // Remove trailing '/'
            RemoveTrailing(builder, DirectorySeparatorChar);

            // Remove trailing '..'
            if (TrimParentAndSelfPath(builder, ref currentPath, paths, true))
            {
                // Remove trailing '/'
                RemoveTrailing(builder, DirectorySeparatorChar);
            }

            // Go back to upper path if current is not vaid
            if (currentPath > 0 && !paths[currentPath].IsValid)
            {
                currentPath--;
            }

            // Copy the drive, directory, filename information to the output
            int startDirectory = 0;
            if (IsDriveSpan(paths[0]))
            {
                drive = paths[0];
                // Make sure to revert to a conventional span (as we use the negative value to identify a drive)
                drive.Length = -drive.Length + 1;
                startDirectory = 1;
            }
                
            // If there is any directory information, process it
            if (startDirectory <= currentPath)
            {
                directoryOrFileName.Start = paths[startDirectory].Start == 1 ? 0 : paths[startDirectory].Start;
                if (currentPath == startDirectory)
                {
                    directoryOrFileName.Length = paths[startDirectory].Length == 0 && paths[startDirectory].Start == 1
                        ? 1
                        : paths[startDirectory].Length;
                }
                else
                {
                    directoryOrFileName.Length = paths[currentPath - 1].Start + paths[currentPath - 1].Length - directoryOrFileName.Start;

                    if (paths[currentPath].IsValid)
                    {
                        // In case last path is a parent '..' don't include it in fileName
                        if (IsParentPath(builder, paths[currentPath]))
                        {
                            directoryOrFileName.Length += paths[currentPath].Length + 1;
                        }
                        else
                        {
                            fileName.Start = paths[currentPath].Start;
                            fileName.Length = builder.Length - fileName.Start;
                        }
                    }
                }
            }

            return builder;
        }

        private static void RemoveTrailing(StringBuilder builder, char charToRemove)
        {
            if (builder.Length > 1 && builder[builder.Length - 1] == charToRemove)
            {
                builder.Length = builder.Length - 1;
            }
        }

        private static bool IsParentPath(StringBuilder builder, StringSpan path)
        {
            return path.Length == 2 &&
                   builder[path.Start] == '.' &&
                   builder[path.Start + 1] == '.';
        }

        private static bool IsRelativeCurrentPath(StringBuilder builder, StringSpan path)
        {
            return path.Length == 1 && builder[path.Start] == '.';
        }

        private static bool IsDriveSpan(StringSpan stringSpan)
        {
            return stringSpan.Length < 0;
        }

        /// <summary>
        /// Trims the path by removing unecessary '..' and '.' path items.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="currentPath">The current path.</param>
        /// <param name="paths">The paths.</param>
        /// <param name="isLastTrim">if set to <c>true</c> is last trim to occur.</param>
        /// <returns><c>true</c> if trim has been done, <c>false</c> otherwise.</returns>
        private static unsafe bool TrimParentAndSelfPath(StringBuilder builder, ref int currentPath, StringSpan* paths, bool isLastTrim)
        {
            var path = paths[currentPath];
            if (currentPath > 0 && IsParentPath(builder, path))
            {
                // If previous path is already a relative path, then we probably can popup
                var previousPath = paths[currentPath - 1];
                if (IsParentPath(builder, previousPath))
                {
                    return false;
                }

                // Note: the drive path has a negative Length at that moment so it will also be considered invalid (which is what we want)
                if (!previousPath.IsValid)
                {
                    // Swallow the parent path if we reached some root level
                    paths[currentPath].Length = 0;
                    builder.Length = paths[currentPath].Start;
                    return true;
                }

                // We can popup the previous path
                paths[currentPath] = new StringSpan();
                currentPath--;
                paths[currentPath].Length = 0;
                builder.Length = paths[currentPath].Start;
                return true;
            }

            var isRelativeCurrentPath = IsRelativeCurrentPath(builder, path);
            if (!(isLastTrim && currentPath == 0 && isRelativeCurrentPath) && isRelativeCurrentPath)
            {
                // We can popup the previous path
                paths[currentPath].Length = 0;
                builder.Length = paths[currentPath].Start;
                return true;
            }
            return false;
        }

        private static string Decode(string pathToNormalize, bool isPathDirectory, out StringSpan drive, out StringSpan directory, out StringSpan fileName, out StringSpan fileExtension)
        {
            drive = new StringSpan();
            directory = new StringSpan();
            fileName = new StringSpan();
            fileExtension = new StringSpan();

            if (string.IsNullOrWhiteSpace(pathToNormalize))
            {
                return string.Empty;
            }

            // Normalize path
            // TODO handle network path/http/file path
            string error;
            var path = Normalize(pathToNormalize, out drive, out directory, out fileName, out error);
            if (error != null)
            {
                throw new ArgumentException(error);
            }

            if (isPathDirectory)
            {
                // If we are expecting a directory, merge the fileName with the directory
                if (fileName.IsValid)
                {
                    // Handle the case when the directory is just / and we don't have a leading drive
                    var separatorLength = directory.Length != 1 ? 1 : 0;
                    directory.Length = directory.Length + separatorLength + fileName.Length;
                    fileName = new StringSpan();
                }
            }
            else
            {
                // In case this is only a directory name and we are expecting a filename, gets the directory name as a filename
                if (directory.IsValid && !fileName.IsValid)
                {
                    fileName = directory;
                    directory = new StringSpan();
                }

                if (fileName.IsValid)
                {
                    var extensionIndex = path.LastIndexOf('.', fileName.Start);
                    if (extensionIndex >= 0)
                    {
                        fileName.Length = extensionIndex - fileName.Start;
                        fileExtension.Start = extensionIndex;
                        fileExtension.Length = path.Length - extensionIndex;
                    }
                }
            }

            return path.ToString();
        }
    }
}
