// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class SubPropertyInfoViewModelContent : UnaryViewModelContentBase
    {
        private readonly List<PropertyInfo> propertyInfoChain = new List<PropertyInfo>();

        public SubPropertyInfoViewModelContent(IContent operand, Type type, params PropertyInfo[] propertyInfoChain)
            : base(type, operand)
        {
            if (propertyInfoChain == null)
                throw new ArgumentException("propertyInfoChain cannot be empty", "propertyInfoChain");

            this.propertyInfoChain.AddRange(propertyInfoChain);
        }

        public SubPropertyInfoViewModelContent(IContent operand, Type type, IEnumerable<PropertyInfo> propertyInfoChain)
            : this(operand, type)
        {
            this.propertyInfoChain.AddRange(propertyInfoChain);

            if (this.propertyInfoChain.Count == 0)
                throw new ArgumentException("propertyInfoChain cannot be empty", "propertyInfoChain");
        }

        private object SetRecursively(object currentObject, object value, int currentIndex = 0)
        {
            PropertyInfo propertyInfo = propertyInfoChain[currentIndex];

            if (currentIndex == propertyInfoChain.Count - 1)
            {
                propertyInfo.SetValue(currentObject, value, null);
            }
            else
            {
                object childValue = propertyInfo.GetValue(currentObject, null);
                childValue = SetRecursively(childValue, value, currentIndex + 1);

                if (propertyInfo.PropertyType.GetTypeInfo().IsValueType)
                {
                    propertyInfo.SetValue(currentObject, childValue, null);
                }
            }

            return currentObject;
        }

        public override object Value
        {
            get
            {
                object currentValue = Operand.Value;
                foreach (PropertyInfo propertyInfo in propertyInfoChain)
                {
                    if (currentValue.GetType().GetTypeInfo().GetProperties().All(x => x != propertyInfo))
                        return null;

                    currentValue = propertyInfo.GetValue(currentValue, null);

                    if (currentValue == null)
                        return null;
                }

                return currentValue;
            }
            set
            {
                SetRecursively(Operand.Value, value);
            }
        }
    }
}
