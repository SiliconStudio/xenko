// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
