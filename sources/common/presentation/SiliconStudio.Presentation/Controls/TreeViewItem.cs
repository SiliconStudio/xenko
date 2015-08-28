using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An item of the TreeView.
    /// </summary>
    public class TreeViewItem : HeaderedItemsControl
    {
        internal double ItemTopInTreeSystem; // for virtualization purposes
        internal int HierachyLevel;// for virtualization purposes

        public static DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(true, null));

        public static DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(false, OnIsEditingChanged));

        public static DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(true));

        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(false, null));

        public static DependencyProperty TemplateEditProperty = DependencyProperty.Register("TemplateEdit", typeof(DataTemplate), typeof(TreeViewItem), new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty TemplateSelectorEditProperty = DependencyProperty.Register("TemplateSelectorEdit", typeof(DataTemplateSelector), typeof(TreeViewItem), new FrameworkPropertyMetadata(null, null));

        public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation", typeof(double), typeof(TreeViewItem), new PropertyMetadata(10.0));

        static TreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(typeof(TreeViewItem)));

            FrameworkElementFactory vPanel = new FrameworkElementFactory(typeof(VirtualizingTreePanel));
            vPanel.SetValue(Panel.IsItemsHostProperty, true);
            ItemsPanelTemplate vPanelTemplate = new ItemsPanelTemplate { VisualTree = vPanel };
            ItemsPanelProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(vPanelTemplate));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            IsTabStopProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(false));
        }

        public bool IsEditable { get { return (bool)GetValue(IsEditableProperty); } set { SetValue(IsEditableProperty, value); } }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }

        public double Indentation { get { return (double)GetValue(IndentationProperty); } set { SetValue(IndentationProperty, value); } }

        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }

        public DataTemplate TemplateEdit { get { return (DataTemplate)GetValue(TemplateEditProperty); } set { SetValue(TemplateEditProperty, value); } }

        public DataTemplateSelector TemplateSelectorEdit { get { return (DataTemplateSelector)GetValue(TemplateSelectorEditProperty); } set { SetValue(TemplateSelectorEditProperty, value); } }

        [DependsOn("Indentation")]
        public double Offset => ParentTreeViewItem?.Offset + Indentation ?? 0;

        public TreeViewItem ParentTreeViewItem => ItemsControlFromItemContainer(this) as TreeViewItem;

        public TreeView ParentTreeView { get; internal set; }

        public new bool IsVisible
        {
            get
            {
                if (Visibility != Visibility.Visible)
                    return false;
                var currentItem = ParentTreeViewItem;
                while (currentItem != null)
                {
                    if (!currentItem.IsExpanded) return false;
                    currentItem = currentItem.ParentTreeViewItem;
                }

                return true;
            }
        }

        private bool IsExpandableOnInput => IsEnabled;


        // Synchronizes the value of the child's IsVirtualizing property with that of the parent's
        internal static void IsVirtualizingPropagationHelper(DependencyObject parent, DependencyObject element)
        {
            SynchronizeValue(VirtualizingStackPanel.IsVirtualizingProperty, parent, element);
            SynchronizeValue(VirtualizingStackPanel.VirtualizationModeProperty, parent, element);
        }

        private static void SynchronizeValue(DependencyProperty dp, DependencyObject parent, DependencyObject child)
        {
            var value = parent.GetValue(dp);
            child.SetValue(dp, value);
        }

        /// <summary>
        ///     This method is invoked when the Items property changes.
        /// </summary>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    ParentTreeView?.ClearObsoleteItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (TreeViewItem)d;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                item.ParentTreeView.StartEditing(item);
            }
            else
            {
                item.ParentTreeView.StopEditing();
            }
        }

        /// <summary>
        ///     Returns true if the item is or should be its own container.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>true if its type matches the container type.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewItem;
        }

        /// <summary>
        ///     Create or identify the element used to display the given item.
        /// </summary>
        /// <returns>The container.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItem();
        }

        public override string ToString()
        {
            if (DataContext != null)
            {
                return $"{DataContext} ({base.ToString()})";
            }

            return base.ToString();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ParentTreeView?.SelectedItems != null && ParentTreeView.SelectedItems.Contains(DataContext))
            {
                IsSelected = true;
            }
        }

        internal void ForceFocus()
        {
            if (!Focus())
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => Focus()));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                Key key = e.Key;
                switch (key)
                {
                    case Key.Left:
                        if (IsExpanded)
                        {
                            IsExpanded = false;
                        }

                        e.Handled = true;
                        break;
                    case Key.Right:
                        IsExpanded = true;
                        e.Handled = true;
                        break;
                    case Key.Up:
                        ParentTreeView.SelectPreviousFromKey();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        ParentTreeView.SelectNextFromKey();
                        e.Handled = true;
                        break;
                    case Key.Add:
                        if (IsExpandableOnInput && !IsExpanded)
                        {
                            IsExpanded = true;
                        }

                        e.Handled = true;
                        break;
                    case Key.Subtract:
                        if (IsExpandableOnInput && IsExpanded)
                        {
                            IsExpanded = false;
                        }

                        e.Handled = true;
                        break;
                    case Key.F2:
                        e.Handled = StartEditing();
                        break;
                    case Key.Escape:
                    case Key.Return:
                        StopEditing();
                        e.Handled = true;
                        break;
                    case Key.Space:
                        ParentTreeView.SelectCurrentBySpace();
                        e.Handled = true;
                        break;
                    case Key.Home:
                        ParentTreeView.SelectFirst();
                        e.Handled = true;
                        break;
                    case Key.End:
                        ParentTreeView.SelectLast();
                        e.Handled = true;
                        break;
                }
            }
        }

        private bool StartEditing()
        {
            if ((TemplateEdit != null || TemplateSelectorEdit != null) && IsFocused && IsEditable)
            {
                IsEditing = true;
                return true;
            }

            return false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.Handled)
            {
                Key key = e.Key;
                switch (key)
                {
                    case Key.Left:
                    case Key.Right:
                    case Key.Up:
                    case Key.Down:
                    case Key.Add:
                    case Key.Subtract:
                    case Key.Space:
                        IEnumerable<TreeViewItem> items = TreeViewElementFinder.FindAll(ParentTreeView, false);
                        TreeViewItem focusedItem = items.FirstOrDefault(x => x.IsFocused);

                        focusedItem?.BringIntoView(new Rect(1, 1, 1, 1));

                        break;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (IsKeyboardFocused)
            {
                IsExpanded = !IsExpanded;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            //if (e.Property.Name == "IsEditing")
            //{
            //    if ((bool)e.NewValue == false)
            //    {
            //        StopEditing();
            //    }
            //    else
            //    {
            //        ParentTreeView.IsEditingManager.SetEditedObject(this);
            //    }
            //}

            if (ParentTreeView != null && e.Property.Name == "IsSelected")
            {
                if (ParentTreeView.SelectedItems.Contains(DataContext) != IsSelected)
                {
                    ParentTreeView.SelectFromProperty(this, IsSelected);
                }
            }

            base.OnPropertyChanged(e);
        }

        private void StopEditing()
        {
            IsEditing = false;
        }
    }
}