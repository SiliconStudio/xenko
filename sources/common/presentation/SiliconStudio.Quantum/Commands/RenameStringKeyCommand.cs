// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Attributes;

namespace SiliconStudio.Quantum.Commands
{
    public class RenameStringKeyCommand : NodeCommand
    {
        private struct UndoTokenData
        {
            private readonly object previousIndex;

            private readonly object newIndex;

            public UndoTokenData(object previousIndex, object newIndex)
            {
                this.previousIndex = previousIndex;
                this.newIndex = newIndex;
            }

            public object PreviousIndex { get { return previousIndex; } }

            public object NewIndex { get { return newIndex; } }
        }

        /// <inheritdoc/>
        public override string Name { get { return "RenameStringKey"; } }

        /// <inheritdoc/>
        public override CombineMode CombineMode { get { return CombineMode.AlwaysCombine; } }


        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor descriptor, MemberDescriptorBase memberDescriptor)
        {
            if (memberDescriptor != null)
            {
                var attrib = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<SealedCollectionAttribute>(memberDescriptor.MemberInfo);
                if (attrib != null && attrib.CollectionSealed)
                    return false;
            }

            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            return dictionaryDescriptor != null && dictionaryDescriptor.KeyType == typeof(string);
        }

        /// <inheritdoc/>
        public override object Invoke(object currentValue, object parameter, out UndoToken undoToken)
        {
            var dictionaryDescriptor = TypeDescriptorFactory.Default.Find(currentValue.GetType()) as DictionaryDescriptor;
            var tuple = parameter as Tuple<object, object>;
            if (dictionaryDescriptor == null || tuple == null)
                throw new InvalidOperationException("This command cannot be executed on the given object.");

            var removedObject = dictionaryDescriptor.GetValue(currentValue, tuple.Item1);
            undoToken = new UndoToken(true, new UndoTokenData(tuple.Item1, tuple.Item2));
            dictionaryDescriptor.Remove(currentValue, tuple.Item1);
            dictionaryDescriptor.SetValue(currentValue, tuple.Item2, removedObject);

            return currentValue;
        }

        /// <inheritdoc/>
        public override object Undo(object currentValue, UndoToken undoToken)
        {
            var dictionaryDescriptor = TypeDescriptorFactory.Default.Find(currentValue.GetType()) as DictionaryDescriptor;
            var undoData = (UndoTokenData)undoToken.TokenValue;
            if (dictionaryDescriptor == null)
                throw new InvalidOperationException("This command cannot be cancelled on the given object.");

            if (dictionaryDescriptor.ContainsKey(currentValue, undoData.PreviousIndex))
                throw new InvalidOperationException("Unable to undo remove: the dictionary contains the key to re-add.");

            var removedObject = dictionaryDescriptor.GetValue(currentValue, undoData.NewIndex);
            dictionaryDescriptor.Remove(currentValue, undoData.NewIndex);
            dictionaryDescriptor.SetValue(currentValue, undoData.PreviousIndex, removedObject);

            return currentValue;
        }
    }
}