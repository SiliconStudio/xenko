// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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