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
using System.Windows;
using System.Windows.Input;
using SiliconStudio.Presentation.Controls.PropertyGrid.Attributes;
using SiliconStudio.Presentation.Controls.PropertyGrid.Commands;
using SiliconStudio.Presentation.Controls.PropertyGrid.Editors;

using System.ComponentModel;
using System.Windows.Data;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal abstract class DescriptorPropertyDefinitionBase : DependencyObject
    {
        private string category;
        private string categoryValue;
        private string description;
        private string displayName;
        private int displayOrder;
        private bool isExpandable;
        private bool isReadOnly;
        private IList<Type> newItemTypes;
        private IEnumerable<CommandBinding> commandBindings;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DescriptorPropertyDefinitionBase), new UIPropertyMetadata(null, OnValueChanged));
       
        public object Value { get { return GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        internal abstract PropertyDescriptor PropertyDescriptor { get; }

        public string Category { get { return category; } }

        public string CategoryValue { get { return categoryValue; } }

        public IEnumerable<CommandBinding> CommandBindings { get { return commandBindings; } }

        public string DisplayName { get { return displayName; } }

        public string Description { get { return description; } }

        public int DisplayOrder { get { return displayOrder; } }

        public bool IsExpandable { get { return isExpandable; } }

        public bool IsReadOnly { get { return isReadOnly; } }

        public IList<Type> NewItemTypes { get { return newItemTypes; } }

        // A common property which is present in all selectedObjects will always have the same name.
        public string PropertyName { get { return PropertyDescriptor.Name; } }

        public Type PropertyType { get { return PropertyDescriptor.PropertyType; } }

        protected virtual string ComputeCategory()
        {
            return null;
        }

        protected virtual string ComputeCategoryValue()
        {
            return null;
        }

        protected virtual string ComputeDescription()
        {
            return null;
        }
        protected virtual int ComputeDisplayOrder()
        {
            return int.MaxValue;
        }

        protected virtual bool ComputeIsExpandable()
        {
            return false;
        }

        protected virtual IList<Type> ComputeNewItemTypes()
        {
            return null;
        }

        protected virtual bool ComputeIsReadOnly()
        {
            return false;
        }

        protected virtual bool ComputeCanResetValue()
        {
            return false;
        }

        protected virtual void ResetValue()
        {
        }

        protected abstract BindingBase CreateValueBinding();

        internal virtual ITypeEditor CreateDefaultEditor()
        {
            return null;
        }

        internal virtual ITypeEditor CreateAttributeEditor()
        {
            return null;
        }

        internal void UpdateValueFromSource()
        {
            var bindingExpressionBase = BindingOperations.GetBindingExpressionBase(this, ValueProperty);
            if (bindingExpressionBase != null)
                bindingExpressionBase.UpdateTarget();
        }


        internal object ComputeDescriptionForItem(object item)
        {
            var pd = (PropertyDescriptor)item;

            //We do not simply rely on the "Description" property of PropertyDescriptor
            //since this value is cached by PropertyDescriptor and the localized version 
            //(e.g., LocalizedDescriptionAttribute) value can dynamicaly change.
            var descriptionAtt = PropertyGridUtilities.GetAttribute<DescriptionAttribute>(pd);
            return (descriptionAtt != null)
                    ? descriptionAtt.Description
                    : pd.Description;
        }

        internal object ComputeNewItemTypesForItem(object item)
        {
            var pd = item as PropertyDescriptor;
            var attribute = PropertyGridUtilities.GetAttribute<NewItemTypesAttribute>(pd);

            return (attribute != null)
                    ? attribute.Types
                    : null;
        }

        internal object ComputeDisplayOrderForItem(object item)
        {
            var pd = item as PropertyDescriptor;
            var attribute = PropertyGridUtilities.GetAttribute<PropertyOrderAttribute>(pd);

            // Max Value. Properties with no order will be displayed last.
            return (attribute != null)
                    ? attribute.Order
                    : int.MaxValue;
        }

        private void ExecuteResetValueCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComputeCanResetValue())
                ResetValue();
        }

        private void CanExecuteResetValueCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ComputeCanResetValue();
        }

        private string ComputeDisplayName()
        {
            string name = PropertyDescriptor.DisplayName;
            var attribute = PropertyGridUtilities.GetAttribute<ParenthesizePropertyNameAttribute>(PropertyDescriptor);
            if ((attribute != null) && attribute.NeedParenthesis)
            {
                name = "(" + name + ")";
            }

            return name;
        }


        private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((DescriptorPropertyDefinitionBase)o).OnValueChanged();
        }

        private void OnValueChanged()
        {
            // Reset command also affected.
            CommandManager.InvalidateRequerySuggested();
        }

        public void InitProperties()
        {
            // Do "IsReadOnly" and PropertyName first since the others may need that value.
            isReadOnly = ComputeIsReadOnly();
            category = ComputeCategory();
            categoryValue = ComputeCategoryValue();
            description = ComputeDescription();
            displayName = ComputeDisplayName();
            displayOrder = ComputeDisplayOrder();
            isExpandable = ComputeIsExpandable();
            newItemTypes = ComputeNewItemTypes();
            commandBindings = new[] { new CommandBinding(PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand) };

            BindingBase valueBinding = CreateValueBinding();
            BindingOperations.SetBinding(this, ValueProperty, valueBinding);
        }
    }
}
