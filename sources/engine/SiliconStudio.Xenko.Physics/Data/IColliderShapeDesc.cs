// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Physics
{
    public interface IColliderShapeDesc
    {
        bool Match(object obj);
    }

    public interface IAssetColliderShapeDesc : IColliderShapeDesc
    {
    }

    public interface IInlineColliderShapeDesc : IAssetColliderShapeDesc
    {
    }
}
