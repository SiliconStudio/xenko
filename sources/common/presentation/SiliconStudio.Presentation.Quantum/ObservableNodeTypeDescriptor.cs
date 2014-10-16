// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Presentation.Quantum.ComponentModel;

namespace SiliconStudio.Presentation.Quantum
{
    public partial class ObservableNode : ICustomTypeDescriptor
    {
        private IObservableNodePropertyProvider propertyProvider;

        public AttributeCollection GetAttributes()
        {
            return new AttributeCollection();
        }

        public string GetClassName()
        {
            return typeof(ObservableNode).FullName;
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            return new TypeConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return propertyProvider != null ? propertyProvider.GetProperties() : new PropertyDescriptorCollection(new PropertyDescriptor[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        private void InitializeTypeDescriptor(ObservableViewModelService service)
        {
            if (service.NodePropertyProviderFactory != null)
            {
                propertyProvider = service.NodePropertyProviderFactory(this);
            }
        }
    }
}