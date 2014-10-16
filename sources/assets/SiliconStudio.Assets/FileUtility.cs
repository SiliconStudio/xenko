// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// File Utilities methods.
    /// </summary>
    public class FileUtility
    {
        /// <summary>
        /// Determines whether the specified file is locked.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if the specified file is locked; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">filePath</exception>
        public static bool IsFileLocked(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            return IsFileLocked(new FileInfo(filePath));
        }


        /// <summary>
        /// Determines whether the specified file is locked.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if the specified file is locked; otherwise, <c>false</c>.</returns>
        public static bool IsFileLocked(FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;            
        }

        /// <summary>
        /// Gets the absolute path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>An absolute path.</returns>
        public static string GetAbsolutePath(string filePath)
        {
            return filePath == null ? null : Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filePath));
        }

        /// <summary>
        /// Normalizes the file extension by adding a '.' prefix and making it lowercase.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>A normalized file extension.</returns>
        public static string NormalizeFileExtension(string fileExtension)
        {
            if (String.IsNullOrEmpty(fileExtension))
            {
                return fileExtension;
            }

            fileExtension = fileExtension.ToLower();
            if (fileExtension.StartsWith("."))
            {
                return fileExtension;
            }
            return String.Format(".{0}", fileExtension);
        }


        /// <summary>
        /// Gets the file extensions normalized separated by ',' ';'.
        /// </summary>
        /// <param name="fileExtensions">The file extensions separated by ',' ';'.</param>
        /// <returns>An array of file extensions.</returns>
        public static HashSet<string> GetFileExtensionsAsSet(string fileExtensions)
        {
            if (fileExtensions == null) throw new ArgumentNullException("fileExtensions");
            var fileExtensionArray = fileExtensions.Split(new[] { ',', ';' }).Select(fileExt => fileExt.Trim().ToLower()).ToList();
            var filteredExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fileExtension in fileExtensionArray.Select(NormalizeFileExtension))
            {
                if (fileExtension == string.Empty)
                {
                    continue;
                }
                filteredExtensions.Add(fileExtension);
            }

            return filteredExtensions;
        }


        /// <summary>
        /// Gets the file extensions normalized separated by ',' ';'.
        /// </summary>
        /// <param name="fileExtensions">The file extensions separated by ',' ';'.</param>
        /// <returns>An array of file extensions.</returns>
        public static string[] GetFileExtensions(string fileExtensions)
        {
            return GetFileExtensionsAsSet(fileExtensions).ToArray();
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectories(string rootDirectory, SearchDirection direction)
        {
            if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");

            var directory = new DirectoryInfo(rootDirectory);
            if (Directory.Exists(rootDirectory))
            {
                if (direction == SearchDirection.Down)
                {
                    yield return directory;
                    foreach (var subDirectory in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
                    {
                        yield return subDirectory;
                    }
                }
                else
                {
                    do
                    {
                        yield return directory;
                        directory = directory.Parent;
                    }
                    while (directory != null);
                }
            }
        }
    }
}