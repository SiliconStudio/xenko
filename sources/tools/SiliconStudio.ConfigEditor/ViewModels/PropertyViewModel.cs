// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Xml;

using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Xenko.ConfigEditor.ViewModels
{
    public class PropertyViewModel : ViewModelBase
    {
        public SectionViewModel Parent { get; private set; }
        public PropertyInfo Property { get; private set; }
        public ConfigurationPropertyAttribute Attribute { get; private set; }

        public PropertyViewModel(SectionViewModel parent, PropertyInfo property, ConfigurationPropertyAttribute attribute)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (property == null)
                throw new ArgumentNullException("property");
            if (attribute == null)
                throw new ArgumentNullException("attribute");

            Parent = parent;

            Property = property;
            Attribute = attribute;

            DefaultValue = Attribute.DefaultValue;
            Value = DefaultValue;
        }

        private bool isUsed;
        public bool IsUsed
        {
            get { return isUsed; }
            set { SetValue(ref isUsed, value, "IsUsed"); }
        }

        public string PropertyName { get { return Property.Name; } }
        public string PropertyTypeName { get { return Property.PropertyType.FullName; } }

        public object DefaultValue { get; private set; }

        private object value;
        public object Value
        {
            get { return value; }
            set { SetValue(ref this.value, value, "Value"); }
        }

        public bool IsRequired { get { return Attribute.IsRequired; } }
    }
}
