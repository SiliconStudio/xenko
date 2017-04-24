// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Engine
{
    public static class ColliderExtensions
    {
        public static Entity GetEntity(this Collider collider)
        {
            return (Entity)collider.EntityObject;
        }
    }
}
