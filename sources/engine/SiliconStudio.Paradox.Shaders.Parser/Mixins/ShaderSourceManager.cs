// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Shaders.Parser.Mixins
{
    /// <summary>
    /// Class ShaderSourceManager
    /// </summary>
    public class ShaderSourceManager
    {
        private readonly object locker = new object();
        private readonly Dictionary<string, ShaderSourceWithHash> loadedShaderSources = new Dictionary<string, ShaderSourceWithHash>();
        private readonly Dictionary<string, string> classNameToPath = new Dictionary<string, string>();

        private const string DefaultEffectFileExtension = ".pdxsl";

        /// <summary>
        /// Gets the directory list.
        /// </summary>
        /// <value>The directory list.</value>
        public List<string> LookupDirectoryList { get; private set; }

        /// <summary>
        /// Gets or sets the URL mapping to file path.
        /// </summary>
        /// <value>The URL automatic file path.</value>
        public Dictionary<string, string> UrlToFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use file system]. (Currently used only by tests, made static)
        /// </summary>
        /// <value><c>true</c> if [use file system]; otherwise, <c>false</c>.</value>
        public static bool UseFileSystem { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderSourceManager"/> class.
        /// </summary>
        public ShaderSourceManager()
        {
            LookupDirectoryList = new List<string>();
            UrlToFilePath = new Dictionary<string, string>();
        }

        /// <summary>
        /// Deletes the shader cache for the specified shaders.
        /// </summary>
        /// <param name="modifiedShaders">The modified shaders.</param>
        public void DeleteObsoleteCache(HashSet<string> modifiedShaders)
        {
            lock (locker)
            {
                foreach (var shaderName in modifiedShaders)
                {
                    loadedShaderSources.Remove(shaderName);
                    classNameToPath.Remove(shaderName);
                }
            }
        }

        public ObjectId GetShaderSourceHash(string type)
        {
            return LoadShaderSource(type).Hash;
        }

        /// <summary>
        /// Loads the shader source with the specified type name.
        /// </summary>
        /// <param name="type">The typeName.</param>
        /// <returns>ShaderSourceWithHash.</returns>
        /// <exception cref="System.IO.FileNotFoundException">If the file was not found</exception>
        public ShaderSourceWithHash LoadShaderSource(string type)
        {
            lock (locker)
            {
                // Load file
                ShaderSourceWithHash shaderSource;
                if (!loadedShaderSources.TryGetValue(type, out shaderSource))
                {
                    var sourceUrl = FindFilePath(type);
                    if (sourceUrl != null)
                    {
                        shaderSource = new ShaderSourceWithHash();

                        // On Windows, Always try to load first from the original URL in order to get the latest version
                        if (Platform.IsWindowsDesktop)
                        {
                            // TODO: the "/path" is hardcoded, used in ImportStreamCommand and EffectSystem. Find a place to share this correctly.
                            var pathUrl = sourceUrl + "/path";
                            if (FileExists(pathUrl))
                            {
                                using (var fileStream = OpenStream(pathUrl))
                                {
                                    string shaderSourcePath;
                                    using (var sr = new StreamReader(fileStream, Encoding.UTF8))
                                        shaderSourcePath = sr.ReadToEnd();

                                    if (File.Exists(shaderSourcePath))
                                    {
                                        using (var sourceStream = File.Open(shaderSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                        {
                                            using (var sr = new StreamReader(sourceStream))
                                                shaderSource.Source = sr.ReadToEnd();
                                        }
                                    }
                                }
                            }
                        }

                        if (shaderSource.Source == null)
                        {
                            using (var fileStream = OpenStream(sourceUrl))
                            {
                                using (var sr = new StreamReader(fileStream))
                                    shaderSource.Source = sr.ReadToEnd();

                                var databaseStream = fileStream as IDatabaseStream;
                                if (databaseStream != null)
                                {
                                    shaderSource.Hash = databaseStream.ObjectId;
                                }
                            }
                        }

                        // If the file was loaded from the database, use the ObjectId returned by the database, otherwise compute it directly
                        if (shaderSource.Hash == ObjectId.Empty)
                        {
                            shaderSource.Hash = ObjectId.FromBytes(Encoding.UTF8.GetBytes(shaderSource.Source));
                        }

                        // Convert URL to absolute file path
                        // TODO can we handle path differently? Current code is just a hack
                        UrlToFilePath.TryGetValue(sourceUrl, out shaderSource.Path);

                        // If Path is null, set it to type at least to be able to have more information
                        if (shaderSource.Path == null)
                        {
                            shaderSource.Path = type;
                        }
                        loadedShaderSources[type] = shaderSource;
                    }
                    else
                    {
                        throw new FileNotFoundException(string.Format("Unable to find shader [{0}]", type), string.Format("{0}.pdxsl", type));
                    }
                }
                return shaderSource;
            }
        }

        /// <summary>
        /// Determines whether a class with the specified type name exists.
        /// </summary>
        /// <param name="typeName">The typeName.</param>
        /// <returns><c>true</c> if a class with the specified type name exists; otherwise, <c>false</c>.</returns>
        public bool IsClassExists(string typeName)
        {
            return FindFilePath(typeName) != null;
        }
        
        public string FindFilePath(string type)
        {
            lock (locker)
            {
                if (LookupDirectoryList == null)
                    return null;

                string path = null;
                if (classNameToPath.TryGetValue(type, out path))
                    return path;

                foreach (var directory in LookupDirectoryList)
                {
                    var fileName = Path.ChangeExtension(type, DefaultEffectFileExtension);
                    var testPath = Path.Combine(directory, fileName);
                    if (FileExists(testPath))
                    {
                        path = testPath;
                        break;
                    }
                }

                if (path != null)
                {
                    classNameToPath.Add(type, path);
                }

                return path;
            }
        }

        private bool FileExists(string path)
        {
            return UseFileSystem ? File.Exists(path) : AssetManager.FileProvider.FileExists(path);
        }

        private Stream OpenStream(string path)
        {
            return UseFileSystem ? File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read) : AssetManager.FileProvider.OpenStream(path, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read);
        }

        public struct ShaderSourceWithHash
        {
            public string Path;
            public string Source;
            public ObjectId Hash;
        }
    }
}