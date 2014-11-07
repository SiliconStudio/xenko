// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Physics;

namespace SiliconStudio.Paradox.Engine
{
    public static class ColliderExtensions
    {
        public static Entity GetEntity(this Collider collider)
        {
            return (Entity)collider.EntityObject;
        }
    }
}