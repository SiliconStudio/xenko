// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.


namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public interface IAttributeTransformer<T, U>
    {
        void Transform(ref T attribute, ref U transformer);
    }
}
