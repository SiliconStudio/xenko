using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Physics
{
    public class PhysicsColliderShapeDataConverter : DataConverter<PhysicsColliderShapeData, PhysicsColliderShape>
    {
        public override void ConstructFromData(ConverterContext converterContext, PhysicsColliderShapeData data, ref PhysicsColliderShape obj)
        {
            throw new NotImplementedException();
        }

        private static ColliderShape CreateShape(IColliderShapeDesc desc)
        {
            ColliderShape shape = null;

            var type = desc.GetType();
            if (type == typeof(Box2DColliderShapeDesc))
            {
                var boxDesc = (Box2DColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //todo
                shape = new Box2DColliderShape(boxDesc.HalfExtent) { LocalOffset = boxDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //todo
                shape = new BoxColliderShape(boxDesc.HalfExtents) { LocalOffset = boxDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //todo
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Height, capsuleDesc.UpAxis) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //todo
                shape = new CylinderColliderShape(cylinderDesc.HalfExtents, cylinderDesc.UpAxis) { LocalOffset = cylinderDesc.LocalOffset, LocalRotation = rotation };
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
                        shape = new ConvexHullColliderShape(convexDesc.ConvexHulls[0][0], convexDesc.ConvexHullsIndices[0][0])
                        {
                            NeedsCustomCollisionCallback = true
                        };
                        
                        shape.UpdateLocalTransformations();

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
                        
                        var subHull = new ConvexHullColliderShape(verts, indices);                        
                        subHull.UpdateLocalTransformations();
                        subCompound.AddChildShape(subHull);
                    }

                    subCompound.UpdateLocalTransformations();

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
                        var subHull = new ConvexHullColliderShape(verts[0], indices[0]);
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

                            var subHull = new ConvexHullColliderShape(subVerts, subIndex);
                            subHull.UpdateLocalTransformations();
                            subCompound.AddChildShape(subHull);
                        }

                        subCompound.UpdateLocalTransformations();

                        compound.AddChildShape(subCompound);
                    }  
                }

                compound.UpdateLocalTransformations();

                return compound;
            }

            if (shape != null) shape.UpdateLocalTransformations();

            return shape;
        }

        public override void ConvertFromData(ConverterContext converterContext, PhysicsColliderShapeData data, ref PhysicsColliderShape obj)
        {           
            if (data.ColliderShapes.Count == 1) //single shape case
            {
                obj = new PhysicsColliderShape(CreateShape(data.ColliderShapes[0]));
            }
            else if (data.ColliderShapes.Count > 1) //need a compound shape in this case
            {
                obj = new PhysicsColliderShape(new CompoundColliderShape());
                var compound = (CompoundColliderShape)obj.Shape;
                foreach (var desc in data.ColliderShapes)
                {
                    compound.AddChildShape(CreateShape(desc));   
                }
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref PhysicsColliderShapeData data, PhysicsColliderShape obj)
        {
            throw new NotImplementedException();
        }
    }
}