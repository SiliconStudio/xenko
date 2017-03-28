using System;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Base interface for compiler registries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompilerRegistry<out T> where T : class, IAssetCompiler
    {
        /// <summary>
        /// Gets the compiler associated to an <see cref="Asset"/> type.
        /// </summary>
        /// <param name="type">The type of the <see cref="Asset"/></param>
        /// <param name="compilationContext">The context in which this list will compile the assets (Asset, Preview, thumbnail etc)</param>
        /// <returns>The compiler associated the provided asset type or null if no compiler exists for that type.</returns>
        T GetCompiler(Type type, Type compilationContext);
    }
}