// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Yaml
{
    /// <summary>
    /// An interface representing a container used to transfer metadata between the asset and the YAML serializer.
    /// </summary>
    internal interface IYamlAssetMetadata : IEnumerable
    {
        /// <summary>
        /// Notifies that this metadata has been attached and cannot be modified anymore.
        /// </summary>
        void Attach();

        /// <summary>
        /// Attaches the given metadata value to the given YAML path.
        /// </summary>
        /// <param name="path">The path at which to attach metadata.</param>
        /// <param name="value">The metadata to attach.</param>
        void Set([NotNull] YamlAssetPath path, object value);

        /// <summary>
        /// Removes attached metadata from the given YAML path.
        /// </summary>
        /// <param name="path">The path at which to remove metadata.</param>
        void Remove([NotNull] YamlAssetPath path);

        /// <summary>
        /// Tries to retrieve the metadata for the given path.
        /// </summary>
        /// <param name="path">The path at which to retrieve metadata.</param>
        /// <returns>The metadata attached to the given path, or the default value of <typeparamref name="T"/> if no metadata is attached at the given path.</returns>
        object TryGet([NotNull] YamlAssetPath path);
    }
}
