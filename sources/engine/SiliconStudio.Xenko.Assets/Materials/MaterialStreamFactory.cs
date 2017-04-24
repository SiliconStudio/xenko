// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Rendering.Materials;

namespace SiliconStudio.Xenko.Assets.Materials
{
    /// <summary>
    /// <see cref="MaterialStreamDescriptor"/> factory.
    /// </summary>
    public static class MaterialStreamFactory
    {
        /// <summary>
        /// Gets the available streams.
        /// </summary>
        /// <returns>List&lt;MaterialStreamDescriptor&gt;.</returns>
        public static List<MaterialStreamDescriptor> GetAvailableStreams()
        {
            var streams = new List<MaterialStreamDescriptor>();
            foreach (var type in typeof(IMaterialStreamProvider).GetInheritedInstantiableTypes())
            {
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var provider = (IMaterialStreamProvider)Activator.CreateInstance(type);
                    streams.AddRange(provider.GetStreams());
                }
            }
            return streams;
        }
    }
}
