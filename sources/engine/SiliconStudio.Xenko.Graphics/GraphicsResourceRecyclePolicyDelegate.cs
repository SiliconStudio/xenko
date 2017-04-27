// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A recycle policy to check whether the specified resource must be disposed from a <see cref="GraphicsResourceAllocator"/>.
    /// </summary>
    /// <param name="resourceLink">The resource link.</param>
    /// <returns><c>true</c> if the specified resource must be disposed and remove from the , <c>false</c> otherwise.</returns>
    public delegate bool GraphicsResourceRecyclePolicyDelegate(GraphicsResourceLink resourceLink);
}
