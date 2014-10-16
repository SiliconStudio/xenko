/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using SiliconStudio.Presentation.Controls.PropertyGrid.Attributes;
using SiliconStudio.Presentation.Controls.PropertyGrid.Editors;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal class DescriptorPropertyDefinition : DescriptorPropertyDefinitionBase
    {
        private readonly object selectedObject;
        private readonly PropertyDescriptor propertyDescriptor;

        internal DescriptorPropertyDefinition(PropertyDescriptor propertyDescriptor, object selectedObject)
        {
            if (propertyDescriptor == null)
                throw new ArgumentNullException("propertyDescriptor");

            if (selectedObject == null)
                throw new ArgumentNullException("selectedObject");

            this.propertyDescriptor = propertyDescriptor;
            this.selectedObject = selectedObject;
        }

        internal override PropertyDescriptor PropertyDescriptor { get { return propertyDescriptor; } }

        private object SelectedObject { get { return selectedObject; } }

        protected override BindingBase CreateValueBinding()
        {
            //Bind the value property with the source object.
            var binding = new Binding(PropertyDescriptor.Name)
            {
                Source = SelectedObject,
                Mode = PropertyDescriptor.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true
            };

            return binding;
        }

        protected override bool ComputeIsReadOnly()
        {
            return PropertyDescriptor.IsReadOnly;
        }

        internal override ITypeEditor CreateDefaultEditor()
        {
            return PropertyGridUtilities.CreateDefaultEditor(PropertyDescriptor.PropertyType, PropertyDescriptor.Converter);
        }

        protected override bool ComputeCanResetValue()
        {
            return PropertyDescriptor.CanResetValue(SelectedObject)
              && !PropertyDescriptor.IsReadOnly;
        }

        protected override string ComputeCategory()
        {
            return PropertyDescriptor.Category;
        }

        protected override string ComputeCategoryValue()
        {
            return PropertyDescriptor.Category;
        }

        protected override bool ComputeIsExpandable()
        {
            bool isExpandable = false;
            var attribute = GetAttribute<ExpandableObjectAttribute>();
            if (attribute != null)
            {
                isExpandable = true;
            }

            return isExpandable;
        }

        protected override IList<Type> ComputeNewItemTypes()
        {
            return (IList<Type>)ComputeNewItemTypesForItem(PropertyDescriptor);
        }
        protected override string ComputeDescription()
        {
            return (string)ComputeDescriptionForItem(PropertyDescriptor);
        }

        protected override int ComputeDisplayOrder()
        {
            return (int)ComputeDisplayOrderForItem(PropertyDescriptor);
        }

        protected override void ResetValue()
        {
            PropertyDescriptor.ResetValue(SelectedObject);
        }

        internal override ITypeEditor CreateAttributeEditor()
        {
            var editorAttribute = GetAttribute<EditorAttribute>();
            if (editorAttribute != null)
            {
                Type type = Type.GetType(editorAttribute.EditorTypeName);

                // If the editor does not have any public parameterless constructor, forget it.
                if (type != null && (typeof(ITypeEditor).IsAssignableFrom(type) && (type.GetConstructor(new Type[0]) != null)))
                {
                    var instance = Activator.CreateInstance(type) as ITypeEditor;
                    Debug.Assert(instance != null, "Type was expected to be ITypeEditor with public constructor.");
                    return instance;
                }
            }

            return null;
        }

        private T GetAttribute<T>() where T : Attribute
        {
            return PropertyGridUtilities.GetAttribute<T>(PropertyDescriptor);
        }
    }
}
