using System.Collections.Generic;
using SharpYaml.Serialization;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Use this if your asset uses another asset at compile time (but not runtime).
    /// </summary>
    public interface IAssetCompileTimeDependencies
    {
        /// <summary>
        /// Enumerates the compile time dependencies.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IContentReference> EnumerateCompileTimeDependencies();
    }
}