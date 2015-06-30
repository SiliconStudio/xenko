using System.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Represents an assembly that is loaded at runtime by the package.
    /// </summary>
    public class PackageLoadedAssembly
    {
        /// <summary>
        /// Gets the project reference for this assembly.
        /// </summary>
        /// <value>
        /// The project reference.
        /// </value>
        public ProjectReference ProjectReference { get; private set; }

        /// <summary>
        /// Gets the path of the assembly.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets the loaded assembly. Could be null if not properly loaded.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public Assembly Assembly { get; set; }

        public PackageLoadedAssembly(ProjectReference projectReference, string path)
        {
            ProjectReference = projectReference;
            Path = path;
        }
    }
}