// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Yaml
{
    /// <summary>
    /// A container class to transfer metadata between the asset and the YAML serializer.
    /// </summary>
    /// <typeparam name="T">The type of metadata.</typeparam>
    public class YamlAssetMetadata<T> : IYamlAssetMetadata, IEnumerable<KeyValuePair<YamlAssetPath, T>>
    {
        private readonly Dictionary<YamlAssetPath, T> metadata = new Dictionary<YamlAssetPath, T>();
        private bool isAttached;

        /// <summary>
        /// Attaches the given metadata value to the given YAML path.
        /// </summary>
        /// <param name="path">The path at which to attach metadata.</param>
        /// <param name="value">The metadata to attach.</param>
        public void Set([NotNull] YamlAssetPath path, T value)
        {
            if (isAttached) throw new InvalidOperationException("Cannot modify a YamlAssetMetadata after it has been attached.");
            metadata[path] = value;
        }

        /// <summary>
        /// Removes attached metadata from the given YAML path.
        /// </summary>
        /// <param name="path">The path at which to remove metadata.</param>
        public void Remove([NotNull] YamlAssetPath path)
        {
            if (isAttached) throw new InvalidOperationException("Cannot modify a YamlAssetMetadata after it has been attached.");
            metadata.Remove(path);
        }

        /// <summary>
        /// Tries to retrieve the metadata for the given path.
        /// </summary>
        /// <param name="path">The path at which to retrieve metadata.</param>
        /// <returns>The metadata attached to the given path, or the default value of <typeparamref name="T"/> if no metadata is attached at the given path.</returns>
        public T TryGet([NotNull] YamlAssetPath path)
        {
            T value;
            metadata.TryGetValue(path, out value);
            return value;
        }

        /// <inheritdoc/>
        void IYamlAssetMetadata.Attach()
        {
            isAttached = true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<YamlAssetPath, T>> GetEnumerator() => metadata.GetEnumerator();
    }
}
