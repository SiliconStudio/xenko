// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<PhysicsColliderShape>))]
    [DataSerializerGlobal(typeof(CloneSerializer<PhysicsColliderShape>), Profile = "Clone")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<PhysicsColliderShape>), Profile = "Asset")]
    public class PhysicsColliderShape
    {
        private List<IColliderShapeDesc> descriptions;

        /// <summary>
        /// Used to serialize one or more collider shapes into one single shape
        /// Reading this value will automatically parse the Shape property into its description
        /// Writing this value will automatically compose, create and populate the Shape property
        /// </summary>
        public List<IColliderShapeDesc> Descriptions
        {
            get { return descriptions; }
            set
            {
                descriptions = value;
                if (descriptions != null)
                {
                    Shape = Compose(descriptions);
                }
            }
        }

        [DataMemberIgnore]
        public ColliderShape Shape { get; private set; }

        public static PhysicsColliderShape New(params IColliderShapeDesc[] descriptions)
        {
            return new PhysicsColliderShape { Descriptions = descriptions.ToList() };
        }
        
        private static ColliderShape Compose(IReadOnlyList<IColliderShapeDesc> descs)
        {
            ColliderShape res = null;

            try
            {
                if (descs.Count == 1) //single shape case
                {
                    res = CreateShape(descs[0]);
                }
                else if (descs.Count > 1) //need a compound shape in this case
                {
                    var compound = new CompoundColliderShape();
                    foreach (var desc in descs)
                    {
                        compound.AddChildShape(CreateShape(desc));
                    }
                    res = compound;
                }
            }
            catch (DllNotFoundException)
            {
                //during pre process and build process we often don't have the physics native engine running.
            }

            return res;
        }

        private static ColliderShape CreateShape(IColliderShapeDesc desc)
        {
            ColliderShape shape = null;

            var type = desc.GetType();
            if (type == typeof(Box2DColliderShapeDesc))
            {
                var boxDesc = (Box2DColliderShapeDesc)desc;
                shape = new Box2DColliderShape(boxDesc.HalfExtent) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
            }
            else if (type == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                shape = new BoxColliderShape(boxDesc.HalfExtents) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
            }
            else if (type == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Height, capsuleDesc.UpAxis) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = capsuleDesc.LocalRotation };
            }
            else if (type == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                shape = new CylinderColliderShape(cylinderDesc.HalfExtents, cylinderDesc.UpAxis) { LocalOffset = cylinderDesc.LocalOffset, LocalRotation = cylinderDesc.LocalRotation };
            }
            else if (type == typeof(SphereColliderShapeDesc))
            {
                var sphereDesc = (SphereColliderShapeDesc)desc;
                shape = new SphereColliderShape(sphereDesc.Is2D, sphereDesc.Radius) { LocalOffset = sphereDesc.LocalOffset };
            }
            else if (type == typeof(StaticPlaneColliderShapeDesc))
            {
                var planeDesc = (StaticPlaneColliderShapeDesc)desc;
                shape = new StaticPlaneColliderShape(planeDesc.Normal, planeDesc.Offset);
            }
            else if (type == typeof(ConvexHullColliderShapeDesc))
            {
                var convexDesc = (ConvexHullColliderShapeDesc)desc;

                //Optimize performance and focus on less shapes creation since this shape could be nested

                if (convexDesc.ConvexHulls.Count == 1)
                {
                    if (convexDesc.ConvexHulls[0].Count == 1)
                    {
                        shape = new ConvexHullColliderShape(convexDesc.ConvexHulls[0][0], convexDesc.ConvexHullsIndices[0][0], convexDesc.Scaling)
                        {
                            NeedsCustomCollisionCallback = true
                        };

                        shape.UpdateLocalTransformations();
                        shape.Description = desc;

                        return shape;
                    }

                    if (convexDesc.ConvexHulls[0].Count <= 1) return null;

                    var subCompound = new CompoundColliderShape
                    {
                        NeedsCustomCollisionCallback = true
                    };

                    for (var i = 0; i < convexDesc.ConvexHulls[0].Count; i++)
                    {
                        var verts = convexDesc.ConvexHulls[0][i];
                        var indices = convexDesc.ConvexHullsIndices[0][i];

                        var subHull = new ConvexHullColliderShape(verts, indices, convexDesc.Scaling);
                        subHull.UpdateLocalTransformations();
                        subCompound.AddChildShape(subHull);
                    }

                    subCompound.UpdateLocalTransformations();
                    subCompound.Description = desc;

                    return subCompound;
                }

                if (convexDesc.ConvexHulls.Count <= 1) return null;

                var compound = new CompoundColliderShape
                {
                    NeedsCustomCollisionCallback = true
                };

                for (var i = 0; i < convexDesc.ConvexHulls.Count; i++)
                {
                    var verts = convexDesc.ConvexHulls[i];
                    var indices = convexDesc.ConvexHullsIndices[i];

                    if (verts.Count == 1)
                    {
                        var subHull = new ConvexHullColliderShape(verts[0], indices[0], convexDesc.Scaling);
                        subHull.UpdateLocalTransformations();
                        compound.AddChildShape(subHull);
                    }
                    else if (verts.Count > 1)
                    {
                        var subCompound = new CompoundColliderShape();

                        for (var b = 0; b < verts.Count; b++)
                        {
                            var subVerts = verts[b];
                            var subIndex = indices[b];

                            var subHull = new ConvexHullColliderShape(subVerts, subIndex, convexDesc.Scaling);
                            subHull.UpdateLocalTransformations();
                            subCompound.AddChildShape(subHull);
                        }

                        subCompound.UpdateLocalTransformations();

                        compound.AddChildShape(subCompound);
                    }
                }

                compound.UpdateLocalTransformations();
                compound.Description = desc;

                return compound;
            }

            if (shape != null)
            {
                shape.UpdateLocalTransformations();
                shape.Description = desc;
            }

            return shape;
        }
    }
}