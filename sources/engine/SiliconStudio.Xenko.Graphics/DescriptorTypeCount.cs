// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes how many descriptor of a specific type will need to be allocated in a <see cref="DescriptorPool"/>.
    /// </summary>
    public struct DescriptorTypeCount
    {
        public EffectParameterClass Type;
        public int Count;

        public DescriptorTypeCount(EffectParameterClass type, int count)
        {
            Type = type;
            Count = count;
        }
    }
}
