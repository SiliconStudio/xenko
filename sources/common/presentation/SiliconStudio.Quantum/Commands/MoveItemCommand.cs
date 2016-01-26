using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    // TODO: this command is very similar to RenameStringKeyCommand - try to factorize them
    public class MoveItemCommand : SyncNodeCommand
    {
        public const string StaticName = "MoveItem";

        /// <inheritdoc/>
        public override string Name => StaticName;

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

        protected override IActionItem ExecuteSync(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var indices = (Tuple<int, int>)parameter;
            var sourceIndex = indices.Item1;
            var targetIndex = indices.Item2;
            content.Remove(sourceIndex);
            content.Add(targetIndex);
            return null;
        }
    }
}