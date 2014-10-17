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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    [TemplatePart(Name = "content", Type = typeof(ContentControl))]
    public class PropertyItem : PropertyItemBase
    {
        private int categoryOrder;

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(PropertyItem), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(PropertyItem), new UIPropertyMetadata(null, OnValueChanged, OnCoerceValueChanged));

        /// <summary>
        /// Identifies the <see cref="IsReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyItem), new UIPropertyMetadata(false));

        public static readonly DependencyProperty PropertyOrderProperty = DependencyProperty.Register("PropertyOrder", typeof(int), typeof(PropertyItem), new UIPropertyMetadata(0));

        public static RoutedCommand ToggleNestedPropertiesCommand { get; private set; }

        static PropertyItem()
        {
            // Since the NumericTextBox is not focusable itself, we have to bind the commands to the inner text box of the control.
            // The handlers will then find the parent that is a NumericTextBox and process the command on this control if it is found.
            ToggleNestedPropertiesCommand = new RoutedCommand("ToggleNestedPropertiesCommand", typeof(PropertyItem));
            CommandManager.RegisterClassCommandBinding(typeof(PropertyItem), new CommandBinding(ToggleNestedPropertiesCommand, OnToggleNestedProperties));
        }

        internal PropertyItem(DescriptorPropertyDefinitionBase definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            DescriptorDefinition = definition;
        }

        public int CategoryOrder
        {
            get
            {
                return categoryOrder;
            }
            internal set
            {
                if (categoryOrder != value)
                {
                    categoryOrder = value;
                    // Notify the parent helper since this property may affect ordering.
                    RaisePropertyChanged(() => CategoryOrder);
                }
            }
        }

        public string Category { get { return (string)GetValue(CategoryProperty); } set { SetValue(CategoryProperty, value); } }

        public object Value { get { return GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        public bool IsReadOnly { get { return (bool)GetValue(IsReadOnlyProperty); } set { SetValue(IsReadOnlyProperty, value); } }

        public int PropertyOrder { get { return (int)GetValue(PropertyOrderProperty); } set { SetValue(PropertyOrderProperty, value); } }

        public PropertyDescriptor PropertyDescriptor { get; internal set; }

        public Type PropertyType { get { return (PropertyDescriptor != null) ? PropertyDescriptor.PropertyType : null; } }

        internal DescriptorPropertyDefinitionBase DescriptorDefinition { get; private set; }

        public object Instance { get; internal set; }

        protected override void OnIsExpandedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                // This withholds the generation of all PropertyItem instances (recursively)
                // until the PropertyItem is expanded.
                var objectContainerHelper = ContainerHelper as ObjectContainerHelperBase;
                if (objectContainerHelper != null)
                {
                    objectContainerHelper.GenerateProperties();
                }
            }
        }

        protected override void OnEditorChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
            if (oldValue != null)
                oldValue.DataContext = null;

            if (newValue != null)
                newValue.DataContext = this;
        }

        protected object OnCoerceValueChanged(object baseValue)
        {
            // Propagate error from DescriptorPropertyDefinitionBase to PropertyItem.Value
            // to see the red error rectangle in the propertyGrid.
            BindingExpression be = GetBindingExpression(ValueProperty);
            if ((be != null) && be.DataItem is DescriptorPropertyDefinitionBase)
            {
                var descriptor = be.DataItem as DescriptorPropertyDefinitionBase;
                if (Validation.GetHasError(descriptor))
                {
                    ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors(descriptor);
                    Validation.MarkInvalid(be, errors[0]);
                }
            }
            return baseValue;
        }

        protected void OnValueChanged(object oldValue, object newValue)
        {
            if (IsInitialized)
            {
                RaiseEvent(new PropertyValueChangedEventArgs(PropertyGrid.PropertyValueChangedEvent, this, oldValue, newValue));
            }

            // Update the ObjectContainerHelper this depends on 
            var helper = new ObjectContainerHelper(this, newValue);
            ContainerHelper = helper;
            if (IsExpanded)
            {
                helper.GenerateProperties();
            }
        }

        private static void OnToggleNestedProperties(object sender, ExecutedRoutedEventArgs e)
        {
            var propertyItem = (PropertyItem)sender;
            bool currentValue = propertyItem.Properties.Cast<PropertyItem>().All(x => x.IsExpanded);
            foreach (var nestedProperty in propertyItem.Properties.Cast<PropertyItem>())
            {
                nestedProperty.IsExpanded = !currentValue;
            }
        }

        private static object OnCoerceValueChanged(DependencyObject o, object baseValue)
        {
            var prop = o as PropertyItem;
            if (prop != null)
                return prop.OnCoerceValueChanged(baseValue);

            return baseValue;
        }

        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyItem = o as PropertyItem;
            if (propertyItem != null)
            {
                propertyItem.OnValueChanged(e.OldValue, e.NewValue);
            }
        }
    }
}
