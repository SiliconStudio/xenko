// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines the TreeViewEx type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows.Core;
using SiliconStudio.Presentation.Collections;

namespace System.Windows.Controls
{
    #region

    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Automation.Peers;
    using System.Windows.Input;
    using System.Windows.Media;

    #endregion

    public class TreeViewEx : ItemsControl
    {
        #region Constants and Fields
        // the space where items will be realized if virtualization is enabled. This is set by virtualizingtreepanel.
        internal VerticalArea realizationSpace = new VerticalArea();
        internal SizesCache cachedSizes = new SizesCache();
        private bool updatingSelection;

        public static readonly DependencyProperty DragTemplateProperty = DependencyProperty.Register(
           "DragTemplate", typeof(DataTemplate), typeof(TreeViewEx), new PropertyMetadata(null));

        public static readonly DependencyProperty InsertTemplateProperty = DependencyProperty.Register(
           "InsertTemplate", typeof(DataTemplate), typeof(TreeViewEx), new PropertyMetadata(null));

        public static DependencyProperty InsertionMarkerBrushProperty =
           DependencyProperty.Register(
              "InsertionMarkerBrush",
              typeof(Brush),
              typeof(TreeViewEx),
              new FrameworkPropertyMetadata(Brushes.Black, null));

        public static DependencyProperty BackgroundSelectionRectangleProperty =
           DependencyProperty.Register(
              "BackgroundSelectionRectangle",
              typeof(Brush),
              typeof(TreeViewEx),
              new FrameworkPropertyMetadata(Brushes.LightBlue, null));

        public static DependencyProperty BorderBrushSelectionRectangleProperty =
           DependencyProperty.Register(
              "BorderBrushSelectionRectangle",
              typeof(Brush),
              typeof(TreeViewEx),
              new FrameworkPropertyMetadata(Brushes.Blue, null));

        public static DependencyProperty SelectedItemProperty =
           DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewEx), new FrameworkPropertyMetadata(null, OnSelectedItemPropertyChanged));

        public static DependencyPropertyKey SelectedItemsProperty = DependencyProperty.RegisterReadOnly(
           "SelectedItems",
           typeof(IList),
           typeof(TreeViewEx),
           new FrameworkPropertyMetadata(
              null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsPropertyChanged));

        public static DependencyProperty SelectionModeProperty = DependencyProperty.Register(
           "SelectionMode",
           typeof(SelectionMode),
           typeof(TreeViewEx),
           new FrameworkPropertyMetadata(
              SelectionMode.Extended, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionModeChanged));

        public static readonly DependencyProperty IsVirtualizingProperty =
            DependencyProperty.Register("IsVirtualizing", typeof(bool), typeof(TreeViewEx), new PropertyMetadata(false));
        
        private InputEventRouter inputEventRouter;

        private bool isInitialized;

        private ScrollViewer scroller;

        #endregion

        #region Constructors and Destructors

        static TreeViewEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(typeof(TreeViewEx)));

            FrameworkElementFactory vPanel = new FrameworkElementFactory(typeof(VirtualizingTreePanel));
            vPanel.SetValue(VirtualizingTreePanel.IsItemsHostProperty, true);
            ItemsPanelTemplate vPanelTemplate = new ItemsPanelTemplate();
            vPanelTemplate.VisualTree = vPanel;
            ItemsPanelProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(vPanelTemplate));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        public TreeViewEx()
        {
            SelectedItems = new NonGenericObservableListWrapper<object>(new ObservableList<object>());
        }

        #endregion

        #region Public Events

        public event EventHandler<SelectionChangedCancelEventArgs> OnSelecting;

        #endregion

        #region Properties

        public bool IsVirtualizing
        {
            get { return (bool)GetValue(IsVirtualizingProperty); }
            set { SetValue(IsVirtualizingProperty, value); }
        }

        public Brush BackgroundSelectionRectangle
        {
            get
            {
                return (Brush)GetValue(BackgroundSelectionRectangleProperty);
            }

            set
            {
                SetValue(BackgroundSelectionRectangleProperty, value);
            }
        }

        public Brush BorderBrushSelectionRectangle
        {
            get
            {
                return (Brush)GetValue(BorderBrushSelectionRectangleProperty);
            }

            set
            {
                SetValue(BorderBrushSelectionRectangleProperty, value);
            }
        }

        public Brush InsertionMarkerBrush
        {
            get
            {
                return (Brush)GetValue(InsertionMarkerBrushProperty);
            }

            set
            {
                SetValue(InsertionMarkerBrushProperty, value);
            }
        }

        public DataTemplate DragTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DragTemplateProperty);
            }

            set
            {
                SetValue(DragTemplateProperty, value);
            }
        }

        public DataTemplate InsertTemplate
        {
            get
            {
                return (DataTemplate)GetValue(InsertTemplateProperty);
            }

            set
            {
                SetValue(InsertTemplateProperty, value);
            }
        }

        /// <summary>
        ///   Gets the last selected item.
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }

            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        internal IsEditingManager IsEditingManager { get; set; }

        /// <summary>
        ///   Gets or sets a list of selected items and can be bound to another list. If the source list implements <see
        ///    cref="INotifyPropertyChanged" /> the changes are automatically taken over.
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                return (IList)GetValue(SelectedItemsProperty.DependencyProperty);
            }

            private set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }

        public SelectionMode SelectionMode
        {
            get
            {
                return (SelectionMode)GetValue(SelectionModeProperty);
            }
            set
            {
                SetValue(SelectionModeProperty, value);
            }
        }

        internal ScrollViewer ScrollViewer
        {
            get
            {
                if (scroller == null)
                {
                    scroller = (ScrollViewer)Template.FindName("scroller", this);
                }

                return scroller;
            }
        }

        internal ISelectionStrategy Selection { get; private set; }

        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!isInitialized)
            {
                Loaded += OnLoaded;
                Unloaded += OnUnLoaded;
                OnLoaded(this, new RoutedEventArgs(LoadedEvent));
            }
        }

        // TODO: This method has been implemented with a lot of fail and retry, and should be cleaned.
        // TODO: Also, it is probably close to work with virtualization, but it needs some testing
        public bool BringItemToView(object item, Func<object, object> getParent)
        {
            // Useful link: https://msdn.microsoft.com/en-us/library/ff407130%28v=vs.110%29.aspx
            if (item == null) throw new ArgumentNullException("item");
            if (getParent == null) throw new ArgumentNullException("getParent");
            if (IsVirtualizing)
                throw new InvalidOperationException("BringItemToView cannot be used when the tree view is virtualizing.");

            TreeViewExItem container = null;

            var path = new List<object> { item };
            var parent = getParent(item);
            while (parent != null)
            {
                path.Add(parent);
                parent = getParent(parent);
            }

            for (int i = path.Count - 1; i >= 0; --i)
            {
                if (container != null)
                    container = (TreeViewExItem)container.ItemContainerGenerator.ContainerFromItem(path[i]);
                else
                    container = (TreeViewExItem)ItemContainerGenerator.ContainerFromItem(path[i]);

                if (container == null)
                    return false;

                container.IsExpanded = true;
                container.ApplyTemplate();
                var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter == null)
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = TreeHelper.FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = TreeHelper.FindVisualChild<ItemsPresenter>(container);
                    }
                }
                if (itemsPresenter == null)
                    return false;

                itemsPresenter.ApplyTemplate();
                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);
                itemsHostPanel.UpdateLayout();
                itemsHostPanel.ApplyTemplate();

                // Ensure that the generator for this panel has been created.
                // ReSharper disable once UnusedVariable
                UIElementCollection children = itemsHostPanel.Children;
                container.BringIntoView();
            }
            return true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure everything is unloaded before reloading!
            OnUnLoaded(sender, e);

            if (SelectionMode == SelectionMode.Single)
            {
                var selectionSingle = new SelectionSingle(this);
                Selection = selectionSingle;
            }
            else
            {
                var selectionMultiple = new SelectionMultiple(this);
                Selection = selectionMultiple;
            }
            IsEditingManager = new IsEditingManager(this);

            inputEventRouter = new InputEventRouter(this);
            inputEventRouter.Add(IsEditingManager);
            inputEventRouter.Add((InputSubscriberBase)Selection);
            isInitialized = true;
        }

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            if (inputEventRouter != null)
            {
                inputEventRouter.Dispose();
                inputEventRouter = null;
            }

            Selection = null;
            scroller = null;
        }

        internal bool CheckSelectionAllowed(object item, bool isItemAdded)
        {
            if (isItemAdded)
            {
                return CheckSelectionAllowed(new List<object> { item }, new List<object>());
            }

            return CheckSelectionAllowed(new List<object>(), new List<object> { item });
        }

        internal bool CheckSelectionAllowed(object itemAdded, object itemRemoved)
        {
            var added = itemAdded != null ? new List<object> { itemAdded } : new List<object>(0);
            var removed = itemRemoved != null ? new List<object> { itemRemoved } : new List<object>(0);
            return CheckSelectionAllowed(added, removed);
        }

        internal bool CheckSelectionAllowed(IEnumerable<object> itemsToSelect, IEnumerable<object> itemsToUnselect)
        {
            if (OnSelecting != null)
            {
                var e = new SelectionChangedCancelEventArgs(itemsToSelect, itemsToUnselect);
                foreach (var method in OnSelecting.GetInvocationList())
                {
                    method.Method.Invoke(method.Target, new object[] { this, e });

                    // stop iteration if one subscriber wants to cancel
                    if (e.Cancel)
                    {
                        return false;
                    }
                }

                return true;
            }

            return true;
        }

        internal IEnumerable<TreeViewExItem> GetChildren(TreeViewExItem item)
        {
            if (item == null) yield break;
            for (int i = 0; i < item.Items.Count; i++)
			{
                TreeViewExItem child = item.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewExItem;
                if (child != null) yield return child;
			}
        }

        internal TreeViewExItem GetNextItem(TreeViewExItem item, List<TreeViewExItem> items)
        {
            int indexOfCurrent = items.IndexOf(item);

            for (int i = indexOfCurrent + 1; i < items.Count; i++)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        internal IEnumerable<TreeViewExItem> GetNodesToSelectBetween(TreeViewExItem firstNode, TreeViewExItem lastNode)
        {
            var allNodes = TreeViewElementFinder.FindAll(this, false).ToList();
            var firstIndex = allNodes.IndexOf(firstNode);
            var lastIndex = allNodes.IndexOf(lastNode);

            if (firstIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "First node index " + firstIndex + "greater or equal than count " + allNodes.Count + ".");
            }

            if (lastIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "Last node index " + lastIndex + " greater or equal than count " + allNodes.Count + ".");
            }

            var nodesToSelect = new List<TreeViewExItem>();

            if (lastIndex == firstIndex)
            {
                return new List<TreeViewExItem> { firstNode };
            }

            if (lastIndex > firstIndex)
            {
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }
            else
            {
                for (int i = firstIndex; i >= lastIndex; i--)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }

            return nodesToSelect;
        }

        /// <summary>
        /// Send down the IsVirtualizing property if it's set on this element.
        /// </summary>
        /// <param name="element">
        /// <param name="item">
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            TreeViewExItem.IsVirtualizingPropagationHelper(this, element);
        }

        internal TreeViewExItem GetPreviousItem(TreeViewExItem item, List<TreeViewExItem> items)
        {
            int indexOfCurrent = items.IndexOf(item);
            for (int i = indexOfCurrent - 1; i >= 0; i--)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        public TreeViewExItem GetTreeViewItemFor(object item)
        {
            foreach (var treeViewExItem in TreeViewElementFinder.FindAll(this, false))
            {
                if (item == treeViewExItem.DataContext)
                {
                    return treeViewExItem;
                }
            }

            return null;
        }

        internal IEnumerable<TreeViewExItem> GetTreeViewItemsFor(IEnumerable objects)
        {
            if (objects == null)
            {
                yield break;
            }
            var items = objects.Cast<object>().ToList();
            foreach (var treeViewExItem in TreeViewElementFinder.FindAll(this, false))
            {
                if (items.Contains(treeViewExItem.DataContext))
                {
                    yield return treeViewExItem;
                }
            }

        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewExItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewExItem;
        }
        
        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeViewEx)d;
            if (!treeView.updatingSelection)
            {
                if (treeView.SelectedItems.Count == 1 && treeView.SelectedItems[0] == e.NewValue)
                    return;

                treeView.updatingSelection = true;
                if (treeView.SelectedItems.Count > 0)
                {
                    foreach (var oldItem in treeView.SelectedItems.Cast<object>().ToList())
                    {
                        var item = treeView.GetTreeViewItemFor(oldItem);
                        if (item != null)
                            item.IsSelected = false;
                    }
                    treeView.SelectedItems.Clear();
                }
                if (e.NewValue != null)
                {
                    var item = treeView.GetTreeViewItemFor(e.NewValue);
                    if (item != null)
                        item.IsSelected = true;
                    treeView.SelectedItems.Add(e.NewValue);
                }
                treeView.updatingSelection = false;
            }
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewEx treeView = (TreeViewEx)d;
            if (e.OldValue != null)
            {
                INotifyCollectionChanged collection = e.OldValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged -= treeView.OnSelectedItemsChanged;
                }
            }

            if (e.NewValue != null)
            {
                INotifyCollectionChanged collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged += treeView.OnSelectedItemsChanged;
                }
            }
        }

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeViewEx)d;
            var newValue = (SelectionMode)e.NewValue;
            if (newValue == SelectionMode.Multiple)
                throw new NotSupportedException("SelectionMode.Multiple is not yet supported. Please use SelectionMode.Single or SelectionMode.Multiple.Extended.");

            if (newValue != SelectionMode.Single)
            {
                var selectedItem = treeView.SelectedItem;
                treeView.updatingSelection = true;
                for (int i = treeView.SelectedItems.Count - 1; i >= 0; --i)
                {
                    if (treeView.SelectedItems[i] != selectedItem)
                    {
                        var item = treeView.GetTreeViewItemFor(treeView.SelectedItems[i]);
                        if (item != null)
                            item.IsSelected = false;
                        treeView.SelectedItems.RemoveAt(i);
                    }
                }
                treeView.updatingSelection = false;
            }
            if (treeView.inputEventRouter != null)
                treeView.inputEventRouter.Remove((InputSubscriberBase)treeView.Selection);

            if (newValue == SelectionMode.Single)
            {
                var selectionSingle = new SelectionSingle(treeView);
                treeView.Selection = selectionSingle;
            }
            else
            {
                var selectionMultiple = new SelectionMultiple(treeView);
                treeView.Selection = selectionMultiple;
            }
            if (treeView.inputEventRouter != null)
                treeView.inputEventRouter.Add((InputSubscriberBase)treeView.Selection);
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (updatingSelection)
                return;

            if (SelectionMode == SelectionMode.Single)
                throw new InvalidOperationException("Can only change SelectedItems collection in multiple selection modes. Use SelectedItem in single select modes.");

            updatingSelection = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    object last = null;
                    foreach (var item in GetTreeViewItemsFor(e.NewItems))
                    {
                        item.IsSelected = true;

                        last = item.DataContext;
                    }

                    SelectedItem = last;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in GetTreeViewItemsFor(e.OldItems))
                    {
                        item.IsSelected = false;
                        if (item.DataContext == SelectedItem)
                        {
                            if (SelectedItems.Count > 0)
                            {
                                SelectedItem = SelectedItems[SelectedItems.Count - 1];
                            }
                            else
                            {
                                SelectedItem = null;
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in TreeViewElementFinder.FindAll(this, false))
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                    }

                    SelectedItem = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            updatingSelection = false;
        }

        /// <summary>
        ///     This method is invoked when the Items property changes.
        /// </summary>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    if (Selection != null) // happens during unload
                        Selection.ClearObsoleteItems(e.OldItems.Cast<object>());
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
        #endregion
    }
}