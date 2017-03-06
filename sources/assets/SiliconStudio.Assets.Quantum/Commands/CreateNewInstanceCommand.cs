// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Assets.Quantum.Commands
{
    public class CreateNewInstanceCommand : ChangeValueCommand
    {
        public const string CommandName = "CreateNewInstance";

        /// <inheritdoc/>
        public override string Name => CommandName;

        /// <inheritdoc/>
        public override CombineMode CombineMode => CombineMode.CombineOnlyForAll;

        /// <inheritdoc/>
        public override bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor)
        {
            var type = typeDescriptor.GetInnerCollectionType();
            var isNullableStruct = type.IsNullable() && Nullable.GetUnderlyingType(type).IsStruct();
            var isAbstractOrClass = type.IsAbstract || type.IsClass;
            //var isCollection = (typeDescriptor is CollectionDescriptor) || (typeDescriptor is DictionaryDescriptor);

            //var result = isNullableStruct || (isAbstractOrClass && !isCollection);
            var result = isNullableStruct || isAbstractOrClass;
            return result;
        }

        protected override object ChangeValue(object currentValue, object parameter)
        {
            var entry = (AbstractNodeEntry)parameter;

            // If value is already OK, keep it
            if (entry.IsMatchingValue(currentValue))
                return currentValue;

            return entry.GenerateValue(currentValue);
        }
    }
}
