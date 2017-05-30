// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    public struct PermutationParameter<T>
    {
        internal readonly int BindingSlot;
        internal readonly int Count;

        internal PermutationParameter(int bindingSlot, int count)
        {
            this.BindingSlot = bindingSlot;
            this.Count = count;
        }
    }
}
