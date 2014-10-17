using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    public class FileExplorer : IDataExplorer
    {
        readonly Dictionary<string, string> rootPaths = new Dictionary<string, string>();

        public Task<IEnumerable<string>> EnumerateDirectories(string absolutePath)
        {
            try
            {
                return absolutePath == null ? Task.FromResult<IEnumerable<string>>(rootPaths.Keys) : Task.FromResult(Directory.Exists(absolutePath) ? Directory.EnumerateDirectories(absolutePath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName) : Enumerable.Empty<string>());
            }
            catch (Exception)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }
        }

        public Task<IEnumerable<string>> EnumerateData(string absolutePath, bool recursive)
        {
            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                if (absolutePath != null && Directory.Exists(absolutePath))
                    return Task.FromResult(Directory.EnumerateFiles(absolutePath, "*", searchOption).Select(Path.GetFileName));

                return Task.FromResult(Enumerable.Empty<string>());
            }
            catch (Exception)
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }
        }

        public Task<bool> CreateDirectory(string absolutePath)
        {
            if (PathExt.IsValidPath(absolutePath))
            {
                return Task.FromResult(false);
            }
            try
            {
                Directory.CreateDirectory(absolutePath);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
