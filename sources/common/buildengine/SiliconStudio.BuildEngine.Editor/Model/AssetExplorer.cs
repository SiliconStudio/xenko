using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    public class AssetExplorer : IDataExplorer
    {
        private DatabaseFileProvider fileProvider;

        public void LoadIndexMap(string assetDatabaseVfsPath)
        {
            // Create and mount database file system
            var objDatabase = new ObjectDatabase(assetDatabaseVfsPath);
            var assetIndexMap = AssetIndexMap.Load();
            fileProvider = new DatabaseFileProvider(assetIndexMap, objDatabase);
        }

        public Task<IEnumerable<string>> EnumerateDirectories(string absolutePath)
        {
            if (fileProvider == null)
                return Task.FromResult(Enumerable.Empty<string>());

            return Task.Run<IEnumerable<string>>(() => 
                {
                    var files = fileProvider.ListFiles(absolutePath, "*/*", VirtualSearchOption.AllDirectories);
                    var set  = new HashSet<string>();
                    foreach (var file in files)
                    {
                        var dir = Path.GetDirectoryName(file);
                        if (dir != null && dir.Length > absolutePath.Length)
                        {
                            string trimmedDir = dir.Substring(absolutePath.Length).Trim("/\\".ToCharArray());
                            if (trimmedDir.Contains("/") || trimmedDir.Contains("\\"))
                                trimmedDir = trimmedDir.Substring(0, trimmedDir.IndexOfAny("/\\".ToCharArray()));

                            set.Add(Path.GetFileName(trimmedDir) ?? "");
                        }
                    }
                    return set;
                });
        }

        public Task<IEnumerable<string>> EnumerateData(string absolutePath, bool recursive)
        {
            if (fileProvider == null)
                return Task.FromResult(Enumerable.Empty<string>());

            var searchOption = recursive ? VirtualSearchOption.AllDirectories : VirtualSearchOption.TopDirectoryOnly;
            return Task.Run<IEnumerable<string>>(() => fileProvider.ListFiles(absolutePath, "?*", searchOption).Select(Path.GetFileName).ToList());
        }

        public Task<bool> CreateDirectory(string absolutePath)
        {
            throw new InvalidOperationException("Cannot create a directory in an asset database.");
        }
    }
}
