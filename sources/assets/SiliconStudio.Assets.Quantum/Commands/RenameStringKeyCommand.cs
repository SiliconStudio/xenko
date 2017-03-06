// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Assets.Quantum.Commands
{
    public class RenameStringKeyCommand : SyncNodeCommandBase
    {
        /// <inheritdoc/>
        public override string Name => "RenameStringKey";

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.AlwaysCombine;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor descriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<MemberCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.ReadOnly)
                    return false;
            }

            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            return dictionaryDescriptor != null && dictionaryDescriptor.KeyType == typeof(string);
        }

        protected override void ExecuteSync(IGraphNode node, Index index, object parameter)
        {
            var objectNode = (IObjectNode)node;
            var oldName = index;
            var renamedObject = node.Retrieve(oldName);
            objectNode.Remove(renamedObject, oldName);
            var newName = AddPrimitiveKeyCommand.GenerateStringKey(node.Retrieve(), node.Descriptor, (string)parameter);
            objectNode.Add(renamedObject, newName);
        }
    }
}
