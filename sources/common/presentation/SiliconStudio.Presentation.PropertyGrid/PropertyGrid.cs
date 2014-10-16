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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using SiliconStudio.Presentation.Controls.PropertyGrid.Commands;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Collections;

using SiliconStudio.Presentation.Controls.PropertyGrid.Core.Utilities;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    [TemplatePart(Name = PART_DragThumb, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_PropertyItemsControl, Type = typeof(PropertyItemsControl))]
    [StyleTypedProperty(Property = "PropertyContainerStyle", StyleTargetType = typeof(PropertyItemBase))]
    public class PropertyGrid : Control, IPropertyContainer, INotifyPropertyChanged
    {
        private const string PART_DragThumb = "PART_DragThumb";
        internal const string PART_PropertyItemsControl = "PART_PropertyItemsControl";
        private readonly WeakEventListener<NotifyCollectionChangedEventArgs> propertyDefinitionsListener;
        private readonly WeakEventListener<NotifyCollectionChangedEventArgs> editorDefinitionsListener;

        private Thumb dragThumb;
        private bool hasPendingSelectedObjectChanged;
        private int initializationCount;
        private ContainerHelperBase containerHelper;
        private PropertyDefinitionCollection propertyDefinitions;
        private EditorDefinitionCollection editorDefinitions;

        public static readonly DependencyProperty AutoGeneratePropertiesProperty = DependencyProperty.Register("AutoGenerateProperties", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));

        public static readonly DependencyProperty ShowSummaryProperty = DependencyProperty.Register("ShowSummary", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(null, OnFilterChanged));

        public static readonly DependencyProperty FilterWatermarkProperty = DependencyProperty.Register("FilterWatermark", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata("Search"));

        public static readonly DependencyProperty IsCategorizedProperty = DependencyProperty.Register("IsCategorized", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true, OnIsCategorizedChanged));

        public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register("NameColumnWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(150.0, OnNameColumnWidthChanged));

        /// <summary>
        /// Identifies the PropertyContainerStyle dependency property
        /// </summary>
        public static readonly DependencyProperty PropertyContainerStyleProperty = DependencyProperty.Register("PropertyContainerStyle", typeof(Style), typeof(PropertyGrid), new UIPropertyMetadata(null, OnPropertyContainerStyleChanged));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(false));

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(object), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedObjectChanged));

        public static readonly DependencyProperty SelectedObjectNameProperty = DependencyProperty.Register("SelectedObjectName", typeof(string), typeof(PropertyGrid), new UIPropertyMetadata(string.Empty, OnSelectedObjectNameChanged, OnCoerceSelectedObjectName));

        private static readonly DependencyPropertyKey SelectedPropertyItemPropertyKey = DependencyProperty.RegisterReadOnly("SelectedPropertyItem", typeof(PropertyItemBase), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedPropertyItemChanged));

        public static readonly DependencyProperty SelectedPropertyItemProperty = SelectedPropertyItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the SelectedProperty dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedPropertyProperty = DependencyProperty.Register("SelectedProperty", typeof(object), typeof(PropertyGrid), new UIPropertyMetadata(null, OnSelectedPropertyChanged));

        public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register("ShowSearchBox", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));

        public static readonly DependencyProperty ShowSortOptionsProperty = DependencyProperty.Register("ShowSortOptions", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));

        public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.Register("ShowTitle", typeof(bool), typeof(PropertyGrid), new UIPropertyMetadata(true));

        public static readonly DependencyProperty TitleContainerTemplateProperty = DependencyProperty.Register("TitleContainerTemplate", typeof(DataTemplate), typeof(PropertyGrid), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ToolContainerTemplateProperty = DependencyProperty.Register("ToolContainerTemplate", typeof(DataTemplate), typeof(PropertyGrid), new UIPropertyMetadata(null));

        public static readonly RoutedEvent PropertyValueChangedEvent = EventManager.RegisterRoutedEvent("PropertyValueChanged", RoutingStrategy.Bubble, typeof(PropertyValueChangedEventHandler), typeof(PropertyGrid));

        public static readonly RoutedEvent SelectedPropertyItemChangedEvent = EventManager.RegisterRoutedEvent("SelectedPropertyItemChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<PropertyItemBase>), typeof(PropertyGrid));

        public static readonly RoutedEvent SelectedObjectChangedEvent = EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(PropertyGrid));

        /// <summary>
        /// Identifies the PreparePropertyItem event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a
        /// PropertyItemBase containing sub-items.
        /// </summary>
        public static readonly RoutedEvent PreparePropertyItemEvent = EventManager.RegisterRoutedEvent("PreparePropertyItem", RoutingStrategy.Bubble, typeof(PropertyItemEventHandler), typeof(PropertyGrid));

        /// <summary>
        /// Identifies the ClearPropertyItem event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a
        /// PropertyItemBase containing sub items.
        /// </summary>
        public static readonly RoutedEvent ClearPropertyItemEvent = EventManager.RegisterRoutedEvent("ClearPropertyItem", RoutingStrategy.Bubble, typeof(PropertyItemEventHandler), typeof(PropertyGrid));

        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        public PropertyGrid()
        {
            propertyDefinitionsListener = new WeakEventListener<NotifyCollectionChangedEventArgs>(OnPropertyDefinitionsCollectionChanged);
            editorDefinitionsListener = new WeakEventListener<NotifyCollectionChangedEventArgs>(OnEditorDefinitionsCollectionChanged);
            UpdateContainerHelper();
            EditorDefinitions = new EditorDefinitionCollection();
            PropertyDefinitions = new PropertyDefinitionCollection();

            AddHandler(PropertyItemBase.ItemSelectionChangedEvent, new RoutedEventHandler(OnItemSelectionChanged));
            AddHandler(PropertyItemsControl.PreparePropertyItemEvent, new PropertyItemEventHandler(OnPreparePropertyItemInternal));
            AddHandler(PropertyItemsControl.ClearPropertyItemEvent, new PropertyItemEventHandler(OnClearPropertyItemInternal));
            CommandBindings.Add(new CommandBinding(PropertyGridCommands.ClearFilter, ClearFilter, CanClearFilter));
        }

        public bool AutoGenerateProperties { get { return (bool)GetValue(AutoGeneratePropertiesProperty); } set { SetValue(AutoGeneratePropertiesProperty, value); } }

        public bool ShowSummary { get { return (bool)GetValue(ShowSummaryProperty); } set { SetValue(ShowSummaryProperty, value); } }

        public string Filter { get { return (string)GetValue(FilterProperty); } set { SetValue(FilterProperty, value); } }

        public string FilterWatermark { get { return (string)GetValue(FilterWatermarkProperty); } set { SetValue(FilterWatermarkProperty, value); } }

        public bool IsCategorized { get { return (bool)GetValue(IsCategorizedProperty); } set { SetValue(IsCategorizedProperty, value); } }

        public double NameColumnWidth { get { return (double)GetValue(NameColumnWidthProperty); } set { SetValue(NameColumnWidthProperty, value); } }

        public IList Properties { get { return containerHelper.Properties; } }

        public bool IsReadOnly { get { return (bool)GetValue(IsReadOnlyProperty); } set { SetValue(IsReadOnlyProperty, value); } }

        public object SelectedObject { get { return GetValue(SelectedObjectProperty); } set { SetValue(SelectedObjectProperty, value); } }

        public string SelectedObjectName { get { return (string)GetValue(SelectedObjectNameProperty); } set { SetValue(SelectedObjectNameProperty, value); } }

        public PropertyItemBase SelectedPropertyItem { get { return (PropertyItemBase)GetValue(SelectedPropertyItemProperty); } internal set { SetValue(SelectedPropertyItemPropertyKey, value); } }

        /// <summary>
        /// Gets or sets the selected property or returns null if the selection is empty.
        /// </summary>
        public object SelectedProperty { get { return GetValue(SelectedPropertyProperty); } set { SetValue(SelectedPropertyProperty, value); } }

        public bool ShowSearchBox { get { return (bool)GetValue(ShowSearchBoxProperty); } set { SetValue(ShowSearchBoxProperty, value); } }

        public bool ShowSortOptions { get { return (bool)GetValue(ShowSortOptionsProperty); } set { SetValue(ShowSortOptionsProperty, value); } }

        public bool ShowTitle { get { return (bool)GetValue(ShowTitleProperty); } set { SetValue(ShowTitleProperty, value); } }

        public DataTemplate TitleContainerTemplate { get { return (DataTemplate)GetValue(TitleContainerTemplateProperty); } set { SetValue(TitleContainerTemplateProperty, value); } }

        public DataTemplate ToolContainerTemplate { get { return (DataTemplate)GetValue(ToolContainerTemplateProperty); } set { SetValue(ToolContainerTemplateProperty, value); } }
      
        /// <summary>
        /// Gets or sets the style that will be applied to all PropertyItemBase instances displayed in the property grid.
        /// </summary>
        public Style PropertyContainerStyle
        {
            get { return (Style)GetValue(PropertyContainerStyleProperty); }
            set { SetValue(PropertyContainerStyleProperty, value); }
        }

        public PropertyDefinitionCollection PropertyDefinitions
        {
            get
            {
                return propertyDefinitions;
            }
            set
            {
                if (propertyDefinitions != value)
                {
                    PropertyDefinitionCollection oldValue = propertyDefinitions;
                    propertyDefinitions = value;
                    OnPropertyDefinitionsChanged(oldValue, value);
                }
            }
        }

        public EditorDefinitionCollection EditorDefinitions
        {
            get
            {
                return editorDefinitions;
            }
            set
            {
                if (editorDefinitions != value)
                {
                    EditorDefinitionCollection oldValue = editorDefinitions;
                    editorDefinitions = value;
                    OnEditorDefinitionsChanged(oldValue, value);
                }
            }
        }

        FilterInfo IPropertyContainer.FilterInfo { get { return new FilterInfo { Predicate = CreateFilter(Filter), InputString = Filter }; } }

        ContainerHelperBase IPropertyContainer.ContainerHelper { get { return containerHelper; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyValueChangedEventHandler PropertyValueChanged { add { AddHandler(PropertyValueChangedEvent, value); } remove { RemoveHandler(PropertyValueChangedEvent, value); } }

        public event RoutedPropertyChangedEventHandler<PropertyItemBase> SelectedPropertyItemChanged { add { AddHandler(SelectedPropertyItemChangedEvent, value); } remove { RemoveHandler(SelectedPropertyItemChangedEvent, value); } }

        public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged { add { AddHandler(SelectedObjectChangedEvent, value); } remove { RemoveHandler(SelectedObjectChangedEvent, value); } }

        /// <summary>
        /// This event is raised when a property item is about to be displayed in the PropertyGrid.
        /// This allow the user to customize the property item just before it is displayed.
        /// </summary>
        public event PropertyItemEventHandler PreparePropertyItem { add { AddHandler(PreparePropertyItemEvent, value); } remove { RemoveHandler(PreparePropertyItemEvent, value); } }

        /// <summary>
        /// This event is raised when an property item is about to be remove from the display in the PropertyGrid
        /// This allow the user to remove any attached handler in the PreparePropertyItem event.
        /// </summary>
        public event PropertyItemEventHandler ClearPropertyItem { add { AddHandler(ClearPropertyItemEvent, value); } remove { RemoveHandler(ClearPropertyItemEvent, value); } }

        protected virtual void OnEditorDefinitionsChanged(EditorDefinitionCollection oldValue, EditorDefinitionCollection newValue)
        {
            if (oldValue != null)
                CollectionChangedEventManager.RemoveListener(oldValue, editorDefinitionsListener);

            if (newValue != null)
                CollectionChangedEventManager.AddListener(newValue, editorDefinitionsListener);

            this.Notify(PropertyChanged, () => EditorDefinitions);
        }

        private void OnEditorDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            containerHelper.NotifyEditorDefinitionsCollectionChanged();
        }

        private static void OnFilterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnFilterChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnFilterChanged(string oldValue, string newValue)
        {
            // The Filter property affects the resulting FilterInfo of IPropertyContainer. Raise an event corresponding
            // to this property.
            this.Notify(PropertyChanged, () => ((IPropertyContainer)this).FilterInfo);
        }

        private static void OnIsCategorizedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnIsCategorizedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsCategorizedChanged(bool oldValue, bool newValue)
        {
            UpdateThumb();
        }

        private static void OnNameColumnWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnNameColumnWidthChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnNameColumnWidthChanged(double oldValue, double newValue)
        {
            if (dragThumb != null)
                ((TranslateTransform)dragThumb.RenderTransform).X = newValue;
        }

        private static void OnPropertyContainerStyleChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var owner = o as PropertyGrid;
            if (owner != null)
                owner.OnPropertyContainerStyleChanged((Style)e.OldValue, (Style)e.NewValue);
        }

        protected virtual void OnPropertyContainerStyleChanged(Style oldValue, Style newValue)
        {
        }

        protected virtual void OnPropertyDefinitionsChanged(PropertyDefinitionCollection oldValue, PropertyDefinitionCollection newValue)
        {
            if (oldValue != null)
                CollectionChangedEventManager.RemoveListener(oldValue, propertyDefinitionsListener);

            if (newValue != null)
                CollectionChangedEventManager.AddListener(newValue, propertyDefinitionsListener);

            this.Notify(PropertyChanged, () => PropertyDefinitions);
        }

        private void OnPropertyDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            containerHelper.NotifyPropertyDefinitionsCollectionChanged();
        }

        private static void OnSelectedObjectChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyInspector = o as PropertyGrid;
            if (propertyInspector != null)
                propertyInspector.OnSelectedObjectChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
        {
            // We do not want to process the change now if the grid is initializing (ie. BeginInit/EndInit).
            if (initializationCount != 0)
            {
                hasPendingSelectedObjectChanged = true;
                return;
            }

            UpdateContainerHelper();

            RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, SelectedObjectChangedEvent));
        }

        private static void OnSelectedObjectTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedObjectTypeChanged((Type)e.OldValue, (Type)e.NewValue);
        }

        protected virtual void OnSelectedObjectTypeChanged(Type oldValue, Type newValue)
        {
        }

        private static object OnCoerceSelectedObjectName(DependencyObject o, object baseValue)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
            {
                if ((propertyGrid.SelectedObject is FrameworkElement) && (String.IsNullOrEmpty((String)baseValue)))
                    return "<no name>";
            }

            return baseValue;
        }

        private static void OnSelectedObjectNameChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.SelectedObjectNameChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void SelectedObjectNameChanged(string oldValue, string newValue)
        {
        }

        private static void OnSelectedPropertyItemChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var propertyGrid = o as PropertyGrid;
            if (propertyGrid != null)
                propertyGrid.OnSelectedPropertyItemChanged((PropertyItemBase)e.OldValue, (PropertyItemBase)e.NewValue);
        }

        protected virtual void OnSelectedPropertyItemChanged(PropertyItemBase oldValue, PropertyItemBase newValue)
        {
            if (oldValue != null)
                oldValue.IsSelected = false;

            if (newValue != null)
                newValue.IsSelected = true;

            SelectedProperty = (newValue != null) ? containerHelper.ItemFromContainer(newValue) : null;

            RaiseEvent(new RoutedPropertyChangedEventArgs<PropertyItemBase>(oldValue, newValue, SelectedPropertyItemChangedEvent));
        }

        private static void OnSelectedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var propertyGrid = sender as PropertyGrid;
            if (propertyGrid != null)
            {
                propertyGrid.OnSelectedPropertyChanged(args.NewValue);
            }
        }

        private void OnSelectedPropertyChanged(object newValue)
        {
            // Do not update the SelectedPropertyItem if the Current SelectedPropertyItem
            // item is the same as the new SelectedProperty. There may be 
            // duplicate items and the result could be to change the selection to the wrong item.
            object currentSelectedProperty = containerHelper.ItemFromContainer(SelectedPropertyItem);
            if (!Equals(currentSelectedProperty, newValue))
            {
                SelectedPropertyItem = containerHelper.ContainerFromItem(newValue);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (dragThumb != null)
                dragThumb.DragDelta -= DragThumbDragDelta;
            dragThumb = GetTemplateChild(PART_DragThumb) as Thumb;
            if (dragThumb != null)
                dragThumb.DragDelta += DragThumbDragDelta;

            containerHelper.ChildrenItemsControl = GetTemplateChild(PART_PropertyItemsControl) as PropertyItemsControl;

            //Update TranslateTransform in code-behind instead of XAML to remove the
            //output window error.
            //When we use FindAncesstor in custom control template for binding internal elements property 
            //into its ancestor element, Visual Studio displays data warning messages in output window when 
            //binding engine meets unmatched target type during visual tree traversal though it does the proper 
            //binding when it receives expected target type during visual tree traversal
            //ref : http://www.codeproject.com/Tips/124556/How-to-suppress-the-System-Windows-Data-Error-warn
            var moveTransform = new TranslateTransform { X = NameColumnWidth };
            if (dragThumb != null)
                dragThumb.RenderTransform = moveTransform;

            UpdateThumb();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            //hitting enter on textbox will update value of underlying source
            if (SelectedPropertyItem != null && e.Key == Key.Enter && e.OriginalSource is TextBox)
            {
                if (!(e.OriginalSource as TextBox).AcceptsReturn)
                {
                    BindingExpression be = ((TextBox)e.OriginalSource).GetBindingExpression(TextBox.TextProperty);
                    if (be != null)
                        be.UpdateSource();
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // First check that the raised property is actually a real CLR property.
            // This could be something else like a Attached DP.
            if (ReflectionHelper.IsPublicInstanceProperty(GetType(), e.Property.Name))
            {
                this.Notify(PropertyChanged, e.Property.Name);
            }
        }

        private void OnItemSelectionChanged(object sender, RoutedEventArgs args)
        {
            var item = (PropertyItemBase)args.OriginalSource;
            if (item.IsSelected)
            {
                SelectedPropertyItem = item;
            }
            else
            {
                if (ReferenceEquals(item, SelectedPropertyItem))
                {
                    SelectedPropertyItem = null;
                }
            }
        }

        private void OnPreparePropertyItemInternal(object sender, PropertyItemEventArgs args)
        {
            containerHelper.PrepareChildrenPropertyItem(args.PropertyItem, args.Item);
            args.Handled = true;
        }

        private void OnClearPropertyItemInternal(object sender, PropertyItemEventArgs args)
        {
            containerHelper.ClearChildrenPropertyItem(args.PropertyItem, args.Item);
            args.Handled = true;
        }

        private void DragThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            NameColumnWidth = Math.Max(0, NameColumnWidth + e.HorizontalChange);
        }

        private void ClearFilter(object sender, ExecutedRoutedEventArgs e)
        {
            Filter = String.Empty;
        }

        private void CanClearFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrEmpty(Filter);
        }

        private void UpdateContainerHelper()
        {
            // Keep a backup of the template element and initialize the
            // new helper with it.
            ItemsControl childrenItemsControl = null;
            if (containerHelper != null)
            {
                childrenItemsControl = containerHelper.ChildrenItemsControl;
                containerHelper.ClearHelper();
            }

            containerHelper = new ObjectContainerHelper(this, SelectedObject);
            ((ObjectContainerHelper)containerHelper).GenerateProperties();


            containerHelper.ChildrenItemsControl = childrenItemsControl;
            // Since the template will bind on this property and this property
            // will be different when the property parent is updated.
            this.Notify(PropertyChanged, () => Properties);
        }

        private void UpdateThumb()
        {
            if (dragThumb != null)
            {
                dragThumb.Margin = IsCategorized ? new Thickness(6, 0, 0, 0) : new Thickness(-1, 0, 0, 0);
            }
        }

        /// <summary>
        /// Override this call to control the filter applied based on the
        /// text input.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual Predicate<object> CreateFilter(string filter)
        {
            return null;
        }

        /// <summary>
        /// Updates all property values in the PropertyGrid with the data from the SelectedObject
        /// </summary>
        public void Update()
        {
            containerHelper.UpdateValuesFromSource();
        }

        /// <summary>
        /// Adds a handler for the PreparePropertyItem attached event
        /// </summary>
        /// <param name="element">the element to attach the handler</param>
        /// <param name="handler">the handler for the event</param>
        public static void AddPreparePropertyItemHandler(UIElement element, PropertyItemEventHandler handler)
        {
            element.AddHandler(PreparePropertyItemEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the PreparePropertyItem attached event
        /// </summary>
        /// <param name="element">the element to attach the handler</param>
        /// <param name="handler">the handler for the event</param>
        public static void RemovePreparePropertyItemHandler(UIElement element, PropertyItemEventHandler handler)
        {
            element.RemoveHandler(PreparePropertyItemEvent, handler);
        }

        internal static void RaisePreparePropertyItemEvent(UIElement source, PropertyItemBase propertyItem, object item)
        {
            source.RaiseEvent(new PropertyItemEventArgs(PreparePropertyItemEvent, source, propertyItem, item));
        }

        /// <summary>
        /// Adds a handler for the ClearPropertyItem attached event
        /// </summary>
        /// <param name="element">the element to attach the handler</param>
        /// <param name="handler">the handler for the event</param>
        public static void AddClearPropertyItemHandler(UIElement element, PropertyItemEventHandler handler)
        {
            element.AddHandler(ClearPropertyItemEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the ClearPropertyItem attached event
        /// </summary>
        /// <param name="element">the element to attach the handler</param>
        /// <param name="handler">the handler for the event</param>
        public static void RemoveClearPropertyItemHandler(UIElement element, PropertyItemEventHandler handler)
        {
            element.RemoveHandler(ClearPropertyItemEvent, handler);
        }

        internal static void RaiseClearPropertyItemEvent(UIElement source, PropertyItemBase propertyItem, object item)
        {
            source.RaiseEvent(new PropertyItemEventArgs(ClearPropertyItemEvent, source, propertyItem, item));
        }
        public override void BeginInit()
        {
            base.BeginInit();
            initializationCount++;
        }

        public override void EndInit()
        {
            base.EndInit();
            if (--initializationCount == 0)
            {
                if (hasPendingSelectedObjectChanged)
                {
                    //This will update SelectedObject, Type, Name based on the actual config.
                    UpdateContainerHelper();
                    hasPendingSelectedObjectChanged = false;
                }
                containerHelper.OnEndInit();
            }
        }
    }

    public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);

    public class PropertyValueChangedEventArgs : RoutedEventArgs
    {
        public object NewValue { get; set; }

        public object OldValue { get; set; }

        public PropertyValueChangedEventArgs(RoutedEvent routedEvent, object source, object oldValue, object newValue)
            : base(routedEvent, source)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    public delegate void PropertyItemEventHandler(object sender, PropertyItemEventArgs e);

    public class PropertyItemEventArgs : RoutedEventArgs
    {
        public PropertyItemBase PropertyItem { get; private set; }

        public object Item { get; private set; }

        public PropertyItemEventArgs(RoutedEvent routedEvent, object source, PropertyItemBase propertyItem, object item)
            : base(routedEvent, source)
        {
            PropertyItem = propertyItem;
            Item = item;
        }
    }
}
