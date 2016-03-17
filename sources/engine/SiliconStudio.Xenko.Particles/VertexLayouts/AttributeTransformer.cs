// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public interface IAttributeTransformer<T>
    {
        void Transform(ref T attribute);
    }
}
