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

using System.Windows;
using System.Windows.Data;

namespace SiliconStudio.Presentation.Controls.PropertyGrid.Editors
{
    public abstract class TypeEditor<T> : ITypeEditor where T : FrameworkElement, new()
    {
        protected T Editor { get; set; }
        protected DependencyProperty ValueProperty { get; set; }

        public virtual FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Editor = new T();
            SetValueDependencyProperty();
            SetControlProperties();
            ResolveValueBinding(propertyItem);
            return Editor;
        }

        protected virtual IValueConverter CreateValueConverter()
        {
            return null;
        }

        protected virtual void ResolveValueBinding(PropertyItem propertyItem)
        {
            var binding = new Binding("Value")
            {
                Source = propertyItem,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                Converter = CreateValueConverter()
            };
            BindingOperations.SetBinding(Editor, ValueProperty, binding);
        }

        protected virtual void SetControlProperties()
        {
            //TODO: implement in derived class
        }

        protected abstract void SetValueDependencyProperty();
    }
}
