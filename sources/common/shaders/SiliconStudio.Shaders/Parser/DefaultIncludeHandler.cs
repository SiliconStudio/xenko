// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;

namespace SiliconStudio.Shaders.Parser
{
    /// <summary>
    /// Default <see cref="IncludeHandler"/> implementation loading files from a set of predefined directories.
    /// </summary>
    public class DefaultIncludeHandler : IncludeHandler
    {
        private Stack<string> CurrentDirectory;
        private List<string> includeDirectories;
        private HashSet<FileStream> files;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIncludeHandler"/> class.
        /// </summary>
        public DefaultIncludeHandler()
        {
            CurrentDirectory = new Stack<string>();
            includeDirectories = new List<string>();
            files = new HashSet<FileStream>();
        }


        /// <summary>
        /// Gets the include directories used by this handler.
        /// </summary>
        public string[] IncludeDirectories
        {
            get
            {
                return includeDirectories.ToArray();
            }
        }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public void AddDirectory(string directory)
        {
            if (includeDirectories.Count > 0)
            {
                if (string.Compare(includeDirectories[0], directory, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return;
            }

            includeDirectories.Add(directory);
        }

        /// <summary>
        /// Adds the directories.
        /// </summary>
        public void AddDirectories(IEnumerable<string> directories)
        {
            foreach (var directory in directories)
                AddDirectory(directory);
       }

        /// <summary>
        /// Finds the full path for filename from the set of <see cref="IncludeDirectories"/> defined.
        /// </summary>
        /// <param name="name">The filename.</param>
        /// <returns>The full filepath or null if file was not found</returns>
        public string FindFullPathForFilename(string name)
        {
            var directories = new List<string>();
            directories.AddRange(includeDirectories);
            if (CurrentDirectory.Count > 0)
                directories.Add(CurrentDirectory.Peek());

            for (int i = directories.Count - 1; i >= 0; i--)
            {
                var includeDirectory = directories[i];
                string filePath = Path.Combine(includeDirectory, name);
                if (File.Exists(filePath))
                    return filePath;
            }
            return null;
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string filePath = FindFullPathForFilename(fileName);
            if (filePath != null)
            {
                var dirPath = Path.GetDirectoryName(filePath);
                CurrentDirectory.Push(dirPath);
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            Console.WriteLine("Warning, unable to find include file [{0}] from directories [{1}]", fileName, string.Join(" ; ", IncludeDirectories));
            return null;
        }

        public void Close(Stream stream)
        {
            stream.Close();
            CurrentDirectory.Pop();
        }
    }
}