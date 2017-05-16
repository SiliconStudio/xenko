// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Quantum;
using IReference = SiliconStudio.Core.Serialization.Contents.IReference;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetNodeContainer : NodeContainer
    {
        public AssetNodeContainer()
        {
            NodeBuilder.RegisterPrimitiveType(typeof(IReference));
            NodeBuilder.RegisterPrimitiveType(typeof(PropertyKey));
            NodeBuilder.RegisterPrimitiveType(typeof(TimeSpan));
            NodeBuilder.RegisterPrimitiveType(typeof(Guid));
            NodeBuilder.RegisterPrimitiveType(typeof(AssetId));
            NodeBuilder.RegisterPrimitiveType(typeof(Color));
            NodeBuilder.RegisterPrimitiveType(typeof(Color3));
            NodeBuilder.RegisterPrimitiveType(typeof(Color4));
            NodeBuilder.RegisterPrimitiveType(typeof(Vector2));
            NodeBuilder.RegisterPrimitiveType(typeof(Vector3));
            NodeBuilder.RegisterPrimitiveType(typeof(Vector4));
            NodeBuilder.RegisterPrimitiveType(typeof(Int2));
            NodeBuilder.RegisterPrimitiveType(typeof(Int3));
            NodeBuilder.RegisterPrimitiveType(typeof(Int4));
            NodeBuilder.RegisterPrimitiveType(typeof(Quaternion));
            NodeBuilder.RegisterPrimitiveType(typeof(RectangleF));
            NodeBuilder.RegisterPrimitiveType(typeof(Rectangle));
            NodeBuilder.RegisterPrimitiveType(typeof(Matrix));
            NodeBuilder.RegisterPrimitiveType(typeof(UPath));
            NodeBuilder.RegisterPrimitiveType(typeof(AngleSingle));
            // Register content types as primitive so they are not processed by Quantum
            foreach (var contentType in AssetRegistry.GetContentTypes())
            {
                NodeBuilder.RegisterPrimitiveType(contentType);
            }
        }

        public new IAssetObjectNode GetOrCreateNode(object rootObject)
        {
            return (IAssetObjectNode)base.GetOrCreateNode(rootObject);
        }

        public new IAssetObjectNode GetNode(object rootObject)
        {
            return (IAssetObjectNode)base.GetNode(rootObject);
        }
    }
}
