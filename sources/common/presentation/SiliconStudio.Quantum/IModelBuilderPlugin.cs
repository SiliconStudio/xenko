using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Provides a way to customize <see cref="DefaultContentFactory" />.
    /// </summary>
    public interface IModelBuilderPlugin
    {
        /// <summary>
        /// Processes the specified <see cref="ModelNode"/>.
        /// </summary>
        /// <param name="nodeBuilder">The node builder.</param>
        /// <param name="modelNode">The model node.</param>
        void Process(INodeBuilder nodeBuilder, ModelNode modelNode);
    }
}