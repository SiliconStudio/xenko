// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Physics;

namespace SiliconStudio.Paradox.Engine.Data
{
    public interface IColliderShapeDesc
    {
    }

    [ContentSerializer(typeof(DataContentSerializer<Box2DColliderShapeDesc>))]
    [DataContract("Box2DColliderShapeDesc")]
    public class Box2DColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector2 HalfExtent;
    }

    [ContentSerializer(typeof(DataContentSerializer<BoxColliderShapeDesc>))]
    [DataContract("BoxColliderShapeDesc")]
    public class BoxColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector3 HalfExtents;
    }

    [ContentSerializer(typeof(DataContentSerializer<CapsuleColliderShapeDesc>))]
    [DataContract("CapsuleColliderShapeDesc")]
    public class CapsuleColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public bool Is2D;

        [DataMember(20)]
        public Vector3 LocalOffset;

        [DataMember(30)]
        public float Radius;

        [DataMember(40)]
        public float Height;

        [DataMember(50)]
        public Vector3 UpAxis;
    }

    [ContentSerializer(typeof(DataContentSerializer<CylinderColliderShapeDesc>))]
    [DataContract("CylinderColliderShapeDesc")]
    public class CylinderColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 LocalOffset;

        [DataMember(20)]
        public Vector3 HalfExtents;

        [DataMember(30)]
        public Vector3 UpAxis;
    }

    [ContentSerializer(typeof(DataContentSerializer<SphereColliderShapeDesc>))]
    [DataContract("SphereColliderShapeDesc")]
    public class SphereColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public bool Is2D;

        [DataMember(20)]
        public Vector3 LocalOffset;

        [DataMember(30)]
        public float Radius;
    }

    [ContentSerializer(typeof(DataContentSerializer<StaticPlaneColliderShapeDesc>))]
    [DataContract("StaticPlaneColliderShapeDesc")]
    public class StaticPlaneColliderShapeDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        public Vector3 Normal;

        [DataMember(20)]
        public float Offset;
    }

    //[ContentSerializer(typeof(DataContentSerializer<ColliderShapeAssetDesc>))]
    //[DataContract("ColliderShapeAssetDesc")]
    //public class ColliderShapeAssetDesc : IColliderShapeDesc
    //{
    //    [DataMember(10)]
    //    public PhysicsColliderShape Asset;
    //}

    public partial class PhysicsColliderShapeData
    {
        public PhysicsColliderShapeData()
        {
            ColliderShapes = new List<IColliderShapeDesc>();
        }

        [DataMember(10)]
        public List<IColliderShapeDesc> ColliderShapes { get; set; }
    }

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
                var rotation = Quaternion.Identity; //Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(boxDesc.Yaw), MathUtil.DegreesToRadians(boxDesc.Pitch), MathUtil.DegreesToRadians(boxDesc.Roll));
                shape = new Box2DColliderShape(boxDesc.HalfExtent) { LocalOffset = boxDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(BoxColliderShapeDesc))
            {
                var boxDesc = (BoxColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(boxDesc.Yaw), MathUtil.DegreesToRadians(boxDesc.Pitch), MathUtil.DegreesToRadians(boxDesc.Roll));
                shape = new BoxColliderShape(boxDesc.HalfExtents) { LocalOffset = boxDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(CapsuleColliderShapeDesc))
            {
                var capsuleDesc = (CapsuleColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(capsuleDesc.Yaw), MathUtil.DegreesToRadians(capsuleDesc.Pitch), MathUtil.DegreesToRadians(capsuleDesc.Roll));
                shape = new CapsuleColliderShape(capsuleDesc.Is2D, capsuleDesc.Radius, capsuleDesc.Height, capsuleDesc.UpAxis) { LocalOffset = capsuleDesc.LocalOffset, LocalRotation = rotation };
            }
            else if (type == typeof(CylinderColliderShapeDesc))
            {
                var cylinderDesc = (CylinderColliderShapeDesc)desc;
                var rotation = Quaternion.Identity; //Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(cylinderDesc.Yaw), MathUtil.DegreesToRadians(cylinderDesc.Pitch), MathUtil.DegreesToRadians(cylinderDesc.Roll));
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
