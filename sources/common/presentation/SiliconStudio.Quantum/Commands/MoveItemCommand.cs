using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public class MoveItemCommand : ActionItemNodeCommand
    {
        public const string StaticName = "MoveItem";

        private class MoveItemActionItem : SimpleNodeCommandActionItem
        {
            private readonly int sourceIndex;
            private readonly int targetIndex;

            public MoveItemActionItem(string name, IContent content, object index, Tuple<int, int> indices, IEnumerable<IDirtiable> dirtiables)
                : base(name, content, index, dirtiables)
            {
                sourceIndex = indices.Item1;
                targetIndex = indices.Item2;
            }

            public override bool Do()
            {
                var value = Content.Retrieve(Index);
                var removedObject = RemoveItemCommand.RemoveItem(value, sourceIndex);
                RemoveItemCommand.InsertItem(value, targetIndex, removedObject);
                Content.Update(value, Index);
                return true;
            }

            protected override void UndoAction()
            {
                var value = Content.Retrieve(Index);
                var removedObject = RemoveItemCommand.RemoveItem(value, targetIndex);
                RemoveItemCommand.InsertItem(value, sourceIndex, removedObject);
                Content.Update(value, Index);
            }
        }

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

        protected override NodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var indices = (Tuple<int, int>)parameter;
            return new MoveItemActionItem(Name, content, index, indices, dirtiables);
        }
    }
}