// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class PropertyInfoViewModelContent : UnaryViewModelContentBase
    {
        private readonly PropertyInfo propertyInfo;
        public readonly object[] Index;

        public PropertyInfoViewModelContent(IContent operand, PropertyInfo propertyInfo, params object[] index)
            : base(propertyInfo.PropertyType, operand)
        {
            this.propertyInfo = propertyInfo;
            this.Index = index;
        }

        /// <inheritdoc/>
        public override object Value
        {
            get
            {
                return propertyInfo.GetValue(Operand.Value, Index);
            }
            set
            {
                var parentValue = Operand.Value;
                propertyInfo.SetValue(parentValue, value, Index);

                // If container type is a structure, propagate up the updated structure (since copy only works by value)
                if (Operand.Type.GetTypeInfo().IsValueType)
                    Operand.Value = parentValue;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return propertyInfo.GetSetMethod(false) == null;
            }
        }
    }
}