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
            CachedScaling = Vector3.One;
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyShort, flipQuadEdges)
            {
                LocalScaling = CachedScaling
            };
            ShortArray = dynamicFieldData;
        }

        public HeightfieldColliderShape(int heightStickWidth, int heightStickLength, UnmanagedArray<byte> dynamicFieldData, float heightScale, float minHeight, float maxHeight, bool flipQuadEdges)
        {
            CachedScaling = Vector3.One;
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyUchar, flipQuadEdges)
            {
                LocalScaling = CachedScaling
            };
            ByteArray = dynamicFieldData;
        }

        public HeightfieldColliderShape(int heightStickWidth, int heightStickLength, UnmanagedArray<float> dynamicFieldData, float heightScale, float minHeight, float maxHeight, bool flipQuadEdges)
        {
            CachedScaling = Vector3.One;
            InternalShape = new BulletSharp.HeightfieldShape(heightStickWidth, heightStickLength, dynamicFieldData.Pointer, heightScale, minHeight, maxHeight, 1, (int)BulletPhyScalarType.PhyFloat, flipQuadEdges)
            {
                LocalScaling = CachedScaling
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
