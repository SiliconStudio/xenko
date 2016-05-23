// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Physics
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<PhysicsColliderShape>))]
    [DataSerializerGlobal(typeof(CloneSerializer<PhysicsColliderShape>), Profile = "Clone")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<PhysicsColliderShape>), Profile = "Content")]
    public class PhysicsColliderShape : IDisposable
    {
        /// <summary>
        /// Used to serialize one or more collider shapes into one single shape
        /// Reading this value will automatically parse the Shape property into its description
        /// Writing this value will automatically compose, create and populate the Shape property
        /// </summary>
        public List<IAssetColliderShapeDesc> Descriptions { get; set; }

        [DataMemberIgnore]
        public ColliderShape Shape { get; internal set; }

        public static PhysicsColliderShape New(params IAssetColliderShapeDesc[] descriptions)
        {
            return new PhysicsColliderShape { Descriptions = descriptions.ToList() };
        }
        
        internal static ColliderShape Compose(IReadOnlyList<IAssetColliderShapeDesc> descs)
        {
            if (descs == null)
            {
                return null;
            }

            ColliderShape res = null;

            if (descs.Count == 1) //single shape case
            {
                res = CreateShape(descs[0]);
                if (res == null) return null;
                res.IsPartOfAsset = true;
            }
            else if (descs.Count > 1) //need a compound shape in this case
            {
                var compound = new CompoundColliderShape();
                foreach (var desc in descs)
                {
                    var subShape = CreateShape(desc);
                    if(subShape == null) continue;
                    compound.AddChildShape(subShape);
                }
                res = compound;
                res.IsPartOfAsset = true;
            }

            return res;
        }

        internal static ColliderShape CreateShape(IColliderShapeDesc desc)
        {
            ColliderShape shape = null;

            var type = desc.GetType();
            if (type == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                if (boxDesc.Is2D)
                {
                    shape = new Box2DColliderShape(new Vector2(boxDesc.Size.X, boxDesc.Size.Y)) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
                }
                else
                {
                    shape = new BoxColliderShape(boxDesc.Size) { LocalOffset = boxDesc.LocalOffset, LocalRotation = boxDesc.LocalRotation };
                }
            }
            else if (type == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Length, capsuleDesc.Orientation) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = capsuleDesc.LocalRotation };
            }
            else if (type == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                shape = new CylinderColliderShape(cylinderDesc.Height, cylinderDesc.Radius, cylinderDesc.Orientation) { LocalOffset = cylinderDesc.LocalOffset, LocalRotation = cylinderDesc.LocalRotation };
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

                if (convexDesc.ConvexHulls == null) return null;

                //Optimize performance and focus on less shapes creation since this shape could be nested

                if (convexDesc.ConvexHulls.Count == 1)
                {
                    if (convexDesc.ConvexHulls[0].Count == 1 && convexDesc.ConvexHullsIndices[0][0].Count > 0)
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

                        if(indices.Count == 0) continue;

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
                        if(indices[0].Count == 0) continue;

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

                            if (subIndex.Count == 0) continue;

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
            else if (type == typeof(ColliderShapeAssetDesc))
            {
                var assetDesc = (ColliderShapeAssetDesc)desc;

                if (assetDesc.Shape == null)
                {
                    return null;
                }

                if (assetDesc.Shape.Shape == null)
                {
                    assetDesc.Shape.Shape = Compose(assetDesc.Shape.Descriptions);
                }

                shape = assetDesc.Shape.Shape;
            }

            if (shape == null) return shape;

            shape.UpdateLocalTransformations();
            shape.Description = desc;

            return shape;
        }

        public void Dispose()
        {
            if (Shape == null) return;

            var compound = Shape.Parent;
            compound?.RemoveChildShape(Shape);

            Shape.Dispose();
            Shape = null;
        }
    }
}
