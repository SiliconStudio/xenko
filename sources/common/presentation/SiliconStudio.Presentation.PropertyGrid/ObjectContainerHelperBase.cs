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
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using SiliconStudio.Presentation.Controls.PropertyGrid.Core.Utilities;
using SiliconStudio.Presentation.Controls.PropertyGrid.Editors;

using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal abstract class ObjectContainerHelperBase : ContainerHelperBase
    {
        // This is needed to work around the ItemsControl behavior.
        // When ItemsControl is preparing its containers, it appears 
        // that calling Refresh() on the CollectionView bound to
        // the ItemsSource prevents the items from being displayed.
        // This patch is to avoid such a behavior.
        private bool isPreparingItemFlag;
        private readonly PropertyItemCollection propertyItemCollection;

        protected ObjectContainerHelperBase(IPropertyContainer propertyContainer)
            : base(propertyContainer)
        {
            propertyItemCollection = new PropertyItemCollection(new ObservableCollection<PropertyItem>());
            UpdateFilter();
            UpdateCategorization();

        }

        public override IList Properties { get { return propertyItemCollection; } }

        private PropertyItem DefaultProperty
        {
            get
            {
                PropertyItem defaultProperty = null;
                var defaultName = GetDefaultPropertyName();
                if (defaultName != null)
                {
                    defaultProperty = propertyItemCollection.FirstOrDefault(prop => Equals(defaultName, prop.PropertyDescriptor.Name));
                }

                return defaultProperty;
            }
        }

        protected PropertyItemCollection PropertyItems
        {
            get
            {
                return propertyItemCollection;
            }
        }

        public override PropertyItemBase ContainerFromItem(object item)
        {
            if (item == null)
                return null;
            // Exception case for ObjectContainerHelperBase. The "Item" may sometimes
            // be identified as a string representing the property name or
            // the PropertyItem itself.
            Debug.Assert(item is PropertyItem || item is string);

            var propertyItem = item as PropertyItem;
            if (propertyItem != null)
                return propertyItem;

            var propertyStr = item as string;
            return PropertyItems.FirstOrDefault(prop => propertyStr == prop.PropertyDescriptor.Name);
        }

        public override object ItemFromContainer(PropertyItemBase container)
        {
            // Since this call is only used to update the PropertyGrid.SelectedProperty property,
            // return the PropertyName.
            var propertyItem = container as PropertyItem;
            if (propertyItem == null)
                return null;

            return propertyItem.PropertyDescriptor.Name;
        }

        public override void UpdateValuesFromSource()
        {
            foreach (PropertyItem item in PropertyItems)
            {
                item.DescriptorDefinition.UpdateValueFromSource();
                item.ContainerHelper.UpdateValuesFromSource();
            }
        }

        public void GenerateProperties()
        {
            if (PropertyItems.Count == 0)
            {
                RegenerateProperties();
            }
        }

        protected override void OnFilterChanged()
        {
            UpdateFilter();
        }

        protected override void OnCategorizationChanged()
        {
            UpdateCategorization();
        }

        protected override void OnAutoGeneratePropertiesChanged()
        {
            RegenerateProperties();
        }

        protected override void OnEditorDefinitionsChanged()
        {
            RegenerateProperties();
        }

        protected override void OnPropertyDefinitionsChanged()
        {
            RegenerateProperties();
        }

        private void UpdateFilter()
        {
            FilterInfo filterInfo = PropertyContainer.FilterInfo;

            if (!string.IsNullOrEmpty(filterInfo.InputString))
            {
                // Ensure all properties are generated so that we can display all results of the filtering
                ExpandAll(PropertyItems);
            }
            PropertyItems.FilterPredicate = filterInfo.Predicate
              ?? PropertyItemCollection.CreateFilter(filterInfo.InputString);
        }

        private static void ExpandAll(IEnumerable<PropertyItem> items)
        {
            foreach (var propertyItem in items)
            {
                propertyItem.IsExpanded = true;
                ExpandAll(propertyItem.Properties.Cast<PropertyItem>());
            }

        }
        private void UpdateCategorization()
        {
            propertyItemCollection.UpdateCategorization(ComputeCategoryGroupDescription());
        }

        private GroupDescription ComputeCategoryGroupDescription()
        {
            if (!PropertyContainer.IsCategorized)
                return null;
            return new PropertyGroupDescription(PropertyItemCollection.CategoryPropertyName);
        }

        private string GetCategoryGroupingPropertyName()
        {
            var propGroup = ComputeCategoryGroupDescription() as PropertyGroupDescription;
            return (propGroup != null) ? propGroup.PropertyName : null;
        }

        private void OnChildrenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsItemOrderingProperty(e.PropertyName)
              || GetCategoryGroupingPropertyName() == e.PropertyName)
            {
                // Refreshing the view while Containers are generated will throw an exception
                if (ChildrenItemsControl.ItemContainerGenerator.Status != GeneratorStatus.GeneratingContainers
                  && !isPreparingItemFlag)
                {
                    PropertyItems.RefreshView();
                }
            }
        }

        protected abstract string GetDefaultPropertyName();

        protected abstract IReadOnlyCollection<PropertyItem> GenerateSubPropertiesCore();

        private void RegenerateProperties()
        {
            IReadOnlyCollection<PropertyItem> subProperties = GenerateSubPropertiesCore();

            foreach (var propertyItem in subProperties)
            {
                InitializePropertyItem(propertyItem);
            }

            //Remove the event callback from the previous children (if any)
            foreach (var propertyItem in PropertyItems)
            {
                propertyItem.PropertyChanged -= OnChildrenPropertyChanged;
            }


            PropertyItems.UpdateItems(subProperties);

            //Add the event callback to the new childrens
            foreach (var propertyItem in PropertyItems)
            {
                propertyItem.PropertyChanged += OnChildrenPropertyChanged;
            }

            // Update the selected property on the property grid only.
            var propertyGrid = PropertyContainer as PropertyGrid;
            if (propertyGrid != null)
            {
                propertyGrid.SelectedPropertyItem = DefaultProperty;
            }
        }

        protected static List<PropertyDescriptor> GetPropertyDescriptors(object instance)
        {
            PropertyDescriptorCollection descriptors;

            TypeConverter tc = TypeDescriptor.GetConverter(instance);
            if (!tc.GetPropertiesSupported())
            {
                var descriptor = instance as ICustomTypeDescriptor;
                descriptors = descriptor != null ? descriptor.GetProperties() : TypeDescriptor.GetProperties(instance.GetType());
            }
            else
            {
                descriptors = tc.GetProperties(instance);
            }

            return (descriptors != null)
              ? descriptors.Cast<PropertyDescriptor>().ToList()
              : null;
        }

        private void InitializePropertyItem(PropertyItem propertyItem)
        {
            DescriptorPropertyDefinitionBase pd = propertyItem.DescriptorDefinition;
            propertyItem.PropertyDescriptor = pd.PropertyDescriptor;

            propertyItem.IsReadOnly = pd.IsReadOnly;
            propertyItem.DisplayName = pd.DisplayName;
            propertyItem.Description = pd.Description;
            propertyItem.Category = pd.Category;
            propertyItem.PropertyOrder = pd.DisplayOrder;
            propertyItem.IsExpandable = pd.IsExpandable;

            //These properties can vary with the value. They need to be bound.
            SetupDefinitionBinding(propertyItem, PropertyItem.ValueProperty, pd, () => pd.Value, BindingMode.TwoWay);

            if (pd.CommandBindings != null)
            {
                foreach (CommandBinding commandBinding in pd.CommandBindings)
                {
                    propertyItem.CommandBindings.Add(commandBinding);
                }
            }
        }

        private void SetupDefinitionBinding<T>(
          PropertyItem propertyItem,
          DependencyProperty itemProperty,
          DescriptorPropertyDefinitionBase pd,
          Expression<Func<T>> definitionProperty,
          BindingMode bindingMode)
        {
            string sourceProperty = ReflectionHelper.GetPropertyOrFieldName(definitionProperty);
            var binding = new Binding(sourceProperty)
            {
                Source = pd,
                Mode = bindingMode
            };

            propertyItem.SetBinding(itemProperty, binding);
        }

        private FrameworkElement GenerateChildrenEditorElement(PropertyItem propertyItem)
        {
            FrameworkElement editorElement = null;
            DescriptorPropertyDefinitionBase pd = propertyItem.DescriptorDefinition;

            ITypeEditor editor = pd.CreateAttributeEditor();
            if (editor != null)
                editorElement = editor.ResolveEditor(propertyItem);


            if (editorElement == null)
                editorElement = GenerateCustomEditingElement(propertyItem.PropertyDescriptor.Name, propertyItem);

            if (editorElement == null)
                editorElement = GenerateCustomEditingElement(propertyItem.PropertyType, propertyItem);

            if (editorElement == null)
            {
                if (pd.IsReadOnly)
                    editor = new TextBlockEditor();

                // Fallback: Use a default type editor.
                if (editor == null)
                {
                    editor = pd.CreateDefaultEditor();
                }

                Debug.Assert(editor != null);

                editorElement = editor.ResolveEditor(propertyItem);
            }

            return editorElement;
        }

        public override void PrepareChildrenPropertyItem(PropertyItemBase propertyItem, object item)
        {
            isPreparingItemFlag = true;
            base.PrepareChildrenPropertyItem(propertyItem, item);

            if (propertyItem.Editor == null)
            {
                FrameworkElement editor = GenerateChildrenEditorElement((PropertyItem)propertyItem);
                if (editor != null)
                {
                    // Tag the editor as generated to know if we should clear it.
                    SetIsGenerated(editor, true);
                    propertyItem.Editor = editor;
                }
            }
            isPreparingItemFlag = false;
        }

        public override void ClearChildrenPropertyItem(PropertyItemBase propertyItem, object item)
        {
            if (propertyItem.Editor != null && GetIsGenerated(propertyItem.Editor))
            {
                propertyItem.Editor = null;
            }

            base.ClearChildrenPropertyItem(propertyItem, item);
        }

        public override Binding CreateChildrenDefaultBinding(PropertyItemBase propertyItem)
        {
            var binding = new Binding("Value")
                {
                    Mode = (((PropertyItem)propertyItem).IsReadOnly) ? BindingMode.OneWay : BindingMode.TwoWay
                };
            return binding;
        }

        protected static string GetDefaultPropertyName(object instance)
        {
            AttributeCollection attributes = TypeDescriptor.GetAttributes(instance);
            var defaultPropertyAttribute = (DefaultPropertyAttribute)attributes[typeof(DefaultPropertyAttribute)];
            return defaultPropertyAttribute != null ? defaultPropertyAttribute.Name : null;
        }

        private static bool IsItemOrderingProperty(string propertyName)
        {
            return string.Equals(propertyName, PropertyItemCollection.DisplayNamePropertyName)
              || string.Equals(propertyName, PropertyItemCollection.CategoryOrderPropertyName)
              || string.Equals(propertyName, PropertyItemCollection.PropertyOrderPropertyName);
        }
    }
}
