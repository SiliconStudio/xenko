using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Assets.Quantum.Commands
{
    // TODO: this command is very similar to RenameStringKeyCommand - try to factorize them
    public class MoveItemCommand : SyncNodeCommandBase
    {
        public const string CommandName = "MoveItem";

        /// <inheritdoc/>
        public override string Name => CommandName;

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.AlwaysCombine;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.ReadOnly)
                    return false;
            }

            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            return collectionDescriptor != null && collectionDescriptor.HasInsert;
        }

        protected override void ExecuteSync(IGraphNode node, Index index, object parameter)
        {
            var objectNode = (IObjectNode)node;
            var indices = (Tuple<int, int>)parameter;
            var sourceIndex = new Index(indices.Item1);
            var targetIndex = new Index(indices.Item2);
            var value = node.Retrieve(sourceIndex);
            objectNode.Remove(value, sourceIndex);
            objectNode.Add(value, targetIndex);
        }
    }
}
