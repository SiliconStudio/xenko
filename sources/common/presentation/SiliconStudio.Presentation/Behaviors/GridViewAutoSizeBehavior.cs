// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SiliconStudio.Presentation.Behaviors
{
    [Obsolete]
    public class GridViewAutoSizeBehavior : DeferredBehaviorBase<ListView>
    {
        /// <summary>
        /// Gets or sets whether the columns are initialized with auto sizing or not.
        /// Note: This parameter is used at initialization time only.
        /// </summary>
        public bool IsAutoSizedByDefault { get; set; }

        protected override void OnAttachedOverride()
        {
            var observableCollection = AssociatedObject.Items.SourceCollection as INotifyCollectionChanged;
            if (observableCollection != null)
                observableCollection.CollectionChanged += SourceCollectionChanged;

            var gridView = AssociatedObject.View as GridView;
            if (gridView != null)
            {
                foreach (var column in gridView.Columns)
                    column.Width = IsAutoSizedByDefault ? double.NaN : column.ActualWidth;
            }
        }

        protected override void OnDetachingOverride()
        {
            var observableCollection = AssociatedObject.Items.SourceCollection as INotifyCollectionChanged;
            if (observableCollection != null)
                observableCollection.CollectionChanged -= SourceCollectionChanged;
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var gridView = AssociatedObject.View as GridView;
            if (gridView != null)
                UpdateAllColumns(gridView);
        }

        private void UpdateAllColumns(GridView gridView)
        {
            foreach (var column in gridView.Columns)
                UpdateColumn(column);
        }

        private void UpdateColumn(GridViewColumn column)
        {
            // auto sizing
            if (double.IsNaN(column.Width))
            {
                // reset size with actual size
                column.Width = column.ActualWidth;
                // set it back to auto sizing
                column.Width = double.NaN;
            }
        }
    }
}
