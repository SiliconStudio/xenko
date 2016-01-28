using System;
using System.Collections.Generic;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Helper class to build a <see cref="DescriptorSetLayout"/>.
    /// </summary>
    public class DescriptorSetLayoutBuilder
    {
        internal int ElementCount;
        internal List<DescriptorSetLayout.Entry> Entries = new List<DescriptorSetLayout.Entry>();

        private ObjectIdBuilder hashBuilder = new ObjectIdBuilder();

        /// <summary>
        /// Returns hash describing current state of DescriptorSet (to know if they can be shared)
        /// </summary>
        public ObjectId Hash => hashBuilder.ComputeHash();

        /// <summary>
        /// Gets (or creates) an entry to the DescriptorSetLayout and gets its index.
        /// </summary>
        /// <returns>The future entry index.</returns>
        public void AddBinding(ParameterKey key, EffectParameterClass @class, int arraySize = 1)
        {
            hashBuilder.Write(key.Name);
            hashBuilder.Write(@class);
            hashBuilder.Write(arraySize);

            ElementCount += arraySize;
            Entries.Add(new DescriptorSetLayout.Entry { Key = key, Class = @class, ArraySize = arraySize });
        }
    }
}