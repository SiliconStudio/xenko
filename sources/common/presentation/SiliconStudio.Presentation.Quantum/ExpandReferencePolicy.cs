namespace SiliconStudio.Presentation.Quantum
{
    public enum ExpandReferencePolicy
    {
        /// <summary>
        /// No children will be generated for this node.
        /// </summary>
        None,

        /// <summary>
        /// Children will be generated for this node, but some filtering might be done by <see cref="IPropertiesProviderViewModel.ShouldConstructMember"/>.
        /// </summary>
        Partial,

        /// <summary>
        /// Children will be generated for this node and all needed children should be generated.
        /// </summary>
        Full,
    }
}
