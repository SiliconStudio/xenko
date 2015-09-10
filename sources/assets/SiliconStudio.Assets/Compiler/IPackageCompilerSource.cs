using System.Collections.Generic;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Enumerate assets that <see cref="PackageCompiler"/> will process.
    /// </summary>
    public interface IPackageCompilerSource
    {
        /// <summary>
        /// Enumerates assets.
        /// </summary>
        IEnumerable<AssetItem> GetAssets(AssetCompilerResult assetCompilerResult);
    }
}