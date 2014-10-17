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

using System.Collections;
using System;
using System.Windows;
using System.ComponentModel;

using SiliconStudio.Presentation.Controls.PropertyGrid.Core.Utilities;

using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal abstract class ContainerHelperBase
    {
        protected readonly IPropertyContainer PropertyContainer;

        internal static readonly DependencyProperty IsGeneratedProperty = DependencyProperty.RegisterAttached("IsGenerated", typeof(bool), typeof(ContainerHelperBase), new PropertyMetadata(false));

        protected ContainerHelperBase(IPropertyContainer propertyContainer)
        {
            if (propertyContainer == null)
                throw new ArgumentNullException("propertyContainer");

            PropertyContainer = propertyContainer;

            var propChange = propertyContainer as INotifyPropertyChanged;
            if (propChange != null)
            {
                propChange.PropertyChanged += OnPropertyContainerPropertyChanged;
            }
        }

        public abstract IList Properties { get; }

        internal ItemsControl ChildrenItemsControl { get; set; }

        internal static bool GetIsGenerated(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsGeneratedProperty);
        }

        internal static void SetIsGenerated(DependencyObject obj, bool value)
        {
            obj.SetValue(IsGeneratedProperty, value);
        }

        public virtual void ClearHelper()
        {
            var propChange = PropertyContainer as INotifyPropertyChanged;
            if (propChange != null)
            {
                propChange.PropertyChanged -= OnPropertyContainerPropertyChanged;
            }

            // Calling RemoveAll() will force the ItemsContol displaying the
            // properties to clear all the current container (i.e., ClearContainerForItem).
            // This will make the call at "ClearChildrenPropertyItem" for every prepared
            // container. Fortunately, the ItemsContainer will not re-prepare the items yet
            // (i.e., probably made on next measure pass), allowing us to set up the new
            // parent helper.
            if (ChildrenItemsControl != null)
            {
                ((IItemContainerGenerator)ChildrenItemsControl.ItemContainerGenerator).RemoveAll();
            }
        }

        public virtual void PrepareChildrenPropertyItem(PropertyItemBase propertyItem, object item)
        {
            // Initialize the parent node
            propertyItem.ParentNode = PropertyContainer;

            PropertyGrid.RaisePreparePropertyItemEvent((UIElement)PropertyContainer, propertyItem, item);
        }

        public virtual void ClearChildrenPropertyItem(PropertyItemBase propertyItem, object item)
        {

            propertyItem.ParentNode = null;

            PropertyGrid.RaiseClearPropertyItemEvent((UIElement)PropertyContainer, propertyItem, item);
        }

        public virtual void OnEndInit() { }

        public abstract PropertyItemBase ContainerFromItem(object item);

        public abstract object ItemFromContainer(PropertyItemBase container);

        public abstract Binding CreateChildrenDefaultBinding(PropertyItemBase propertyItem);

        public virtual void NotifyEditorDefinitionsCollectionChanged() { }

        public virtual void NotifyPropertyDefinitionsCollectionChanged() { }

        public abstract void UpdateValuesFromSource();

        protected FrameworkElement GenerateCustomEditingElement(Type definitionKey, PropertyItemBase propertyItem)
        {
            FrameworkElement editor = null;

            if (PropertyContainer.EditorDefinitions != null)
            {
                // If no editor for the current type, fall back on base type editor recursively.
                while (editor == null && definitionKey != null)
                {
                    editor = CreateCustomEditor(PropertyContainer.EditorDefinitions[definitionKey], propertyItem);
                    definitionKey = definitionKey.BaseType;
                }
            }

            return editor;
        }

        protected FrameworkElement GenerateCustomEditingElement(object definitionKey, PropertyItemBase propertyItem)
        {
            return (PropertyContainer.EditorDefinitions != null)
              ? CreateCustomEditor(PropertyContainer.EditorDefinitions[definitionKey], propertyItem)
              : null;
        }

        protected FrameworkElement CreateCustomEditor(EditorDefinitionBase customEditor, PropertyItemBase propertyItem)
        {
            return (customEditor != null)
              ? customEditor.GenerateEditingElementInternal(propertyItem)
              : null;
        }

        protected virtual void OnPropertyContainerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propertyName = e.PropertyName;
            IPropertyContainer ps = null;
            if (propertyName == ReflectionHelper.GetPropertyOrFieldName(() => ps.FilterInfo))
            {
                OnFilterChanged();
            }
            else if (propertyName == ReflectionHelper.GetPropertyOrFieldName(() => ps.IsCategorized))
            {
                OnCategorizationChanged();
            }
            else if (propertyName == ReflectionHelper.GetPropertyOrFieldName(() => ps.AutoGenerateProperties))
            {
                OnAutoGeneratePropertiesChanged();
            }
            else if (propertyName == ReflectionHelper.GetPropertyOrFieldName(() => ps.EditorDefinitions))
            {
                OnEditorDefinitionsChanged();
            }
            else if (propertyName == ReflectionHelper.GetPropertyOrFieldName(() => ps.PropertyDefinitions))
            {
                OnPropertyDefinitionsChanged();
            }
        }

        protected virtual void OnCategorizationChanged() { }

        protected virtual void OnFilterChanged() { }

        protected virtual void OnAutoGeneratePropertiesChanged() { }

        protected virtual void OnEditorDefinitionsChanged() { }

        protected virtual void OnPropertyDefinitionsChanged() { }
    }
}
