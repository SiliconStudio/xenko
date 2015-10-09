// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Rendering.Materials;

namespace SiliconStudio.Paradox.Assets.Materials
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