using System;
using SiliconStudio.Assets.Quantum.Commands;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;
using IReference = SiliconStudio.Core.Serialization.Contents.IReference;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetNodeContainer : NodeContainer
    {
        public AssetNodeContainer()
        {
            NodeBuilder.AvailableCommands.Add(new AddNewItemCommand());
            NodeBuilder.AvailableCommands.Add(new AddPrimitiveKeyCommand());
            NodeBuilder.AvailableCommands.Add(new CreateNewInstanceCommand());
            NodeBuilder.AvailableCommands.Add(new RemoveItemCommand());
            NodeBuilder.AvailableCommands.Add(new MoveItemCommand());
            NodeBuilder.AvailableCommands.Add(new RenameStringKeyCommand());
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
    }

    public class AssetNodeFactory : IContentFactory
    {
        /// <inheritdoc/>
        public IGraphNode CreateObjectContent(INodeBuilder nodeBuilder, Guid guid, object obj, ITypeDescriptor descriptor, bool isPrimitive)
        {
            var reference = nodeBuilder.CreateReferenceForNode(descriptor.Type, obj, false) as ReferenceEnumerable;
            return new AssetObjectNode(nodeBuilder, obj, guid, descriptor, isPrimitive, reference);
        }

        /// <inheritdoc/>
        public IGraphNode CreateBoxedContent(INodeBuilder nodeBuilder, Guid guid, object structure, ITypeDescriptor descriptor, bool isPrimitive)
        {
            return new AssetBoxedNode(nodeBuilder, structure, guid, descriptor, isPrimitive);
        }

        /// <inheritdoc/>
        public IGraphNode CreateMemberContent(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor member, bool isPrimitive, object value)
        {
            var reference = nodeBuilder.CreateReferenceForNode(member.Type, value, true);
            return new AssetMemberNode(nodeBuilder, guid, parent, member, isPrimitive, reference);
        }
    }
}
