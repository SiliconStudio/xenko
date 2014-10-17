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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System;
using System.Collections.Specialized;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Controls.PropertyGrid.Core.Utilities;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    public class PropertyItemCollection : ReadOnlyObservableCollection<PropertyItem>
    {
        internal static readonly string CategoryPropertyName;
        internal static readonly string CategoryOrderPropertyName;
        internal static readonly string PropertyOrderPropertyName;
        internal static readonly string DisplayNamePropertyName;

        private bool preventNotification;

        static PropertyItemCollection()
        {
            PropertyItem p = null;
            CategoryPropertyName = ReflectionHelper.GetPropertyOrFieldName(() => p.Category);
            CategoryOrderPropertyName = ReflectionHelper.GetPropertyOrFieldName(() => p.CategoryOrder);
            PropertyOrderPropertyName = ReflectionHelper.GetPropertyOrFieldName(() => p.PropertyOrder);
            DisplayNamePropertyName = ReflectionHelper.GetPropertyOrFieldName(() => p.DisplayName);
        }

        public PropertyItemCollection(ObservableCollection<PropertyItem> editableCollection)
            : base(editableCollection)
        {
            EditableCollection = editableCollection;
        }

        internal Predicate<object> FilterPredicate
        {
            get { return GetDefaultView().Filter; }
            set { GetDefaultView().Filter = value; }
        }

        public ObservableCollection<PropertyItem> EditableCollection { get; private set; }

        private ICollectionView GetDefaultView()
        {
            return CollectionViewSource.GetDefaultView(this);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (preventNotification)
                return;

            base.OnCollectionChanged(args);
        }

        internal void UpdateItems(IEnumerable<PropertyItem> newItems)
        {
            if (newItems == null)
                throw new ArgumentNullException("newItems");

            preventNotification = true;
            using (GetDefaultView().DeferRefresh())
            {
                EditableCollection.Clear();
                foreach (var item in newItems)
                {
                    EditableCollection.Add(item);
                }
            }
            preventNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void UpdateCategorization(GroupDescription groupDescription)
        {
            // Clear view values
            ICollectionView view = GetDefaultView();
            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();

                // Update view values
                if (groupDescription != null)
                {
                    view.GroupDescriptions.Add(groupDescription);
                }
            }
        }

        internal void RefreshView()
        {
            GetDefaultView().Refresh();
        }

        internal static Predicate<object> CreateFilter(string text)
        {
            Predicate<object> filter = null;

            if (!string.IsNullOrEmpty(text))
            {
                filter = item =>
                {
                    var property = (PropertyItem)item;
                    if (property.DisplayName != null && property.DisplayName.ToLower().Contains(text.ToLower()))
                        return true;

                    return property.Properties.Cast<PropertyItem>().SelectDeep(x => x.Properties.Cast<PropertyItem>()).Any(subProperty => subProperty.DisplayName != null && subProperty.DisplayName.ToLower().Contains(text.ToLower()));
                };
            }

            return filter;
        }
    }
}
