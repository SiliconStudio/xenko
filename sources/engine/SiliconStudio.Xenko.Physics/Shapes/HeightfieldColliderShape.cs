// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Physics.Shapes
{
    enum BulletPhyScalarType
    {
        PhyFloat,
        PhyDouble,
        PhyInteger,
        PhyShort,
        PhyFixedpoint88,
        PhyUchar
    }

    public class HeightfieldColliderShape : ColliderShape
    {
        public HeightfieldColliderShape(int heightStickWidth, int heightStickLength, UnmanagedArray<short> dynamicFieldData, float heightScale, float minHeight, float maxHeight, bool flipQuadEdges)
        {
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyShort, flipQuadEdges)
            {
                LocalScaling = Vector3.One
            };
            ShortArray = dynamicFieldData;
        }

        public HeightfieldColliderShape(int heightStickWidth, int heightStickLength, UnmanagedArray<byte> dynamicFieldData, float heightScale, float minHeight, float maxHeight, bool flipQuadEdges)
        {
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyUchar, flipQuadEdges)
            {
                LocalScaling = Vector3.One
            };
            ByteArray = dynamicFieldData;
        }

        public HeightfieldColliderShape(int heightStickWidth, int heightStickLength, UnmanagedArray<float> dynamicFieldData, float heightScale, float minHeight, float maxHeight, bool flipQuadEdges)
        {
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyFloat, flipQuadEdges)
            {
                LocalScaling = Vector3.One
            };
            FloatArray = dynamicFieldData;
        }

        public bool UseDiamondSubdivision
        {
            set { ((BulletSharp.HeightfieldShape)InternalShape).SetUseDiamondSubdivision(value); }
        }

        public bool UseZigzagSubdivision
        {
            set { ((BulletSharp.HeightfieldShape)InternalShape).SetUseZigzagSubdivision(value); }
        }

        public UnmanagedArray<short> ShortArray { get; private set; }

        public UnmanagedArray<byte> ByteArray { get; private set; }

        public UnmanagedArray<float> FloatArray { get; private set; }

        public override void Dispose()
        {
            base.Dispose();

            ShortArray?.Dispose();
            ShortArray = null;
            ByteArray?.Dispose();
            ByteArray = null;
            FloatArray?.Dispose();
            FloatArray = null;
        }
    }
}
