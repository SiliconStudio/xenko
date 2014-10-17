namespace SiliconStudio.BuildEngine.Editor.Model
{
    /// <summary>
    /// A source folder used by a build script.
    /// </summary>
    public struct SourceFolder
    {
        /// <summary>
        /// Friendly name for the source folder. Used to generate a variable that can be used in source path properties of build steps.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Relative or absolute path of the source folder.
        /// </summary>
        public string Path { get; set; }
    }
}
