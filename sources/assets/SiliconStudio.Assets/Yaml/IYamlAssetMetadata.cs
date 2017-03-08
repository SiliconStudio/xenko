namespace SiliconStudio.Assets.Yaml
{
    /// <summary>
    /// An interface representing a container used to transfer metadata between the asset and the YAML serializer.
    /// </summary>
    internal interface IYamlAssetMetadata
    {
        /// <summary>
        /// Notifies that this metadata has been attached and cannot be modified anymore.
        /// </summary>
        void Attach();
    }
}
