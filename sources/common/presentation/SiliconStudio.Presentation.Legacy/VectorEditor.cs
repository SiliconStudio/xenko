// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

using Point = System.Windows.Point;

namespace SiliconStudio.Presentation.Legacy
{
    [TemplatePart(Name = "PART_Elements", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_AllSelector", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_VerticalSelector", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_HorizontalSelector", Type = typeof(ItemsControl))]
    [ContentProperty("VectorSource")]
    public class VectorEditor : Control
    {
        private ItemsControl elements;
        private ContentControl allSelector;
        private ItemsControl verticalSelector;
        private ItemsControl horizontalSelector;

        static VectorEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VectorEditor), new FrameworkPropertyMetadata(typeof(VectorEditor)));

            InitializeVectorLayout();
        }

        #region Vector Layout Description management

        private static HashSet<VectorLayoutDescriptor> vectorLayoutDescriptors;

        private static void InitializeVectorLayout()
        {
            var equalityComparer = new AnonymousEqualityComparer<VectorLayoutDescriptor>(
                (a, b) => a.VectorType == b.VectorType,
                a => a.VectorType.GetHashCode());
            vectorLayoutDescriptors = new HashSet<VectorLayoutDescriptor>(equalityComparer);

            InitializeDefaultVectorLayouts();
        }

        private static void InitializeDefaultVectorLayouts()
        {
            RegisterVectorLayoutDescriptor(typeof(Thickness), new SizeI(2, 2), new ThicknessElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Size), new SizeI(2, 1), new SizeElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(SizeI), new SizeI(2, 1), new SizeElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Point), new SizeI(2, 1), new Vector3ElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Vector2), new SizeI(2, 1), new Vector3ElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Vector3), new SizeI(3, 1), new Vector3ElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Vector4), new SizeI(4, 1), new Vector4ElementsMapper());
            RegisterVectorLayoutDescriptor(typeof(Matrix), new SizeI(4, 4), new MatrixElementsMapper());
        }

        public static bool RegisterVectorLayoutDescriptor(Type vectorType, SizeI mapSize, IVectorElementsMapper vectorElementsMapper)
        {
            return vectorLayoutDescriptors.Add(new VectorLayoutDescriptor(vectorType, mapSize, vectorElementsMapper));
        }

        #endregion

        private List<CheckablePart> elementParts = new List<CheckablePart>();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            elements = FrameworkElementExtensions.CheckTemplatePart<ItemsControl>(GetTemplateChild("PART_Elements"));
            allSelector = FrameworkElementExtensions.CheckTemplatePart<ContentControl>(GetTemplateChild("PART_AllSelector"));
            verticalSelector = FrameworkElementExtensions.CheckTemplatePart<ItemsControl>(GetTemplateChild("PART_VerticalSelector"));
            horizontalSelector = FrameworkElementExtensions.CheckTemplatePart<ItemsControl>(GetTemplateChild("PART_HorizontalSelector"));

            CreateAllLayoutParts();
        }

        private void CleanupAllLayoutsParts()
        {
            Rows = 0;
            Columns = 0;
            IsUnknownType = true;

            foreach (CheckablePart part in elementParts)
                part.PropertyChanged -= OnElementPartPropertyChanged;
            elementParts.Clear();

            if (allSelector != null)
                allSelector.Content = null;

            if (verticalSelector != null)
                verticalSelector.ItemsSource = null;

            if (horizontalSelector != null)
                horizontalSelector.ItemsSource = null;

            if (elements != null)
                elements.ItemsSource = null;

            InitializeSelectorsVisibility();
        }

        private void CreateAllLayoutParts()
        {
            CleanupAllLayoutsParts();

            if (VectorSource != null)
            {
                VectorLayoutDescriptor vectorLayoutDescriptor = vectorLayoutDescriptors
                    .FirstOrDefault(vld => vld.VectorType == VectorSource.GetType());
                if (vectorLayoutDescriptor != null)
                {
                    IsUnknownType = false;
                    Columns = vectorLayoutDescriptor.LayoutSize.Width;
                    Rows = vectorLayoutDescriptor.LayoutSize.Height;

                    CreateSelectorLayoutParts();
                    CreateValuedLayoutParts(vectorLayoutDescriptor);

                    return;
                }
            }

            IsUnknownType = true;
            Columns = 0;
            Rows = 0;
        }

        private void CreateSelectorLayoutParts()
        {
            if (elements != null)
            {
                var part = new CheckablePart(new SizeI(-1, -1));
                part.PropertyChanged += OnElementPartPropertyChanged;
                allSelector.Content = part;
                elementParts.Add(part);
            }

            if (verticalSelector != null)
            {
                var vs = new List<CheckablePart>();
                for (int i = 0; i < Columns; i++)
                {
                    var part = new CheckablePart(new SizeI(i, -1));
                    part.PropertyChanged += OnElementPartPropertyChanged;
                    vs.Add(part);
                    elementParts.Add(part);
                }
                verticalSelector.ItemsSource = vs.ToArray();
            }

            if (horizontalSelector != null)
            {
                var vs = new List<CheckablePart>();
                for (int i = 0; i < Rows; i++)
                {
                    var part = new CheckablePart(new SizeI(-1, i));
                    part.PropertyChanged += OnElementPartPropertyChanged;
                    vs.Add(part);
                    elementParts.Add(part);
                }
                horizontalSelector.ItemsSource = vs.ToArray();
            }

            InitializeSelectorsVisibility();
        }

        private object localValue;

        private void CreateValuedLayoutParts(VectorLayoutDescriptor vectorLayoutDescriptor = null)
        {
            if (elements == null)
                return;

            if (vectorLayoutDescriptor == null)
                vectorLayoutDescriptor = vectorLayoutDescriptors.FirstOrDefault(vld => vld.VectorType == VectorSource.GetType());

            CheckablePart[] valuedParts = null;
            ValuedCheckablePart[] previousValuedParts = elementParts.OfType<ValuedCheckablePart>().ToArray();

            if (isValueType || ForceUpdateVectorSource)
            {
                if (localValue == null)
                    localValue = VectorSource.MemberwiseClone();
                else
                    localValue = VectorSource;

                valuedParts = CreateValuedParts(vectorLayoutDescriptor, localValue).ToArray();
            }
            else
                valuedParts = CreateValuedParts(vectorLayoutDescriptor, VectorSource).ToArray();

            if (previousValuedParts.Length > 0)
            {
                for (int i = 0; i < previousValuedParts.Length; i++)
                {
                    previousValuedParts[i].PropertyChanged -= OnElementPartPropertyChanged;
                    valuedParts[i].IsChecked = previousValuedParts[i].IsChecked;
                }
            }

            elementParts.RemoveAll(p => p is ValuedCheckablePart);
            elementParts.AddRange(valuedParts);

            elements.ItemsSource = null;
            elements.ItemsSource = valuedParts;
        }

        private IEnumerable<CheckablePart> CreateValuedParts(VectorLayoutDescriptor vectorLayoutDescriptor, object value)
        {
            if (vectorLayoutDescriptor == null)
                throw new ArgumentNullException("vectorLayoutDescriptor");

            int rows = vectorLayoutDescriptor.LayoutSize.Height;
            int columns = vectorLayoutDescriptor.LayoutSize.Width;

            IVectorElementsMapper mapper = vectorLayoutDescriptor.ElementsMapper;
            string[] names = mapper.GetVectorElementNames().ToArray();

            if (vectorLayoutDescriptor.ElementsMapper is IFullVectorElementsMapper)
            {
                var fullMapper = vectorLayoutDescriptor.ElementsMapper as IFullVectorElementsMapper;

                int i = 0;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        var part = new DirectValuedCheckablePart(new SizeI(x, y), value, names[i++], fullMapper);
                        part.PropertyChanged += OnElementPartPropertyChanged;
                        yield return part;
                    }
                }
            }
            else
            {
                int i = 0;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        var part = new AutomaticValuedCheckablePart(new SizeI(x, y), value, names[i++]);
                        part.PropertyChanged += OnElementPartPropertyChanged;
                        yield return part;
                    }
                }
            }
        }

        private void UpdateValuedParts(object newValue)
        {
            selfSet = true;
            foreach (ValuedCheckablePart p in elementParts.OfType<ValuedCheckablePart>())
            {
                p.UpdateInstance(newValue);
            }
            selfSet = false;

            //OnVectorSourceValueChanged(newValue);
        }

        private void InitializeSelectorsVisibility()
        {
            if (allSelector != null && allSelector.Content != null)
                allSelector.Visibility = Visibility.Visible;

            if (horizontalSelector != null && horizontalSelector.ItemsSource != null)
                horizontalSelector.Visibility = Visibility.Visible;

            if (verticalSelector != null && verticalSelector.ItemsSource != null)
                verticalSelector.Visibility = Visibility.Visible;

            if (Rows <= 1 && Columns <= 1)
            {
                if (allSelector != null)
                    allSelector.Visibility = Visibility.Collapsed;
                if (horizontalSelector != null)
                    horizontalSelector.Visibility = Visibility.Collapsed;
                if (verticalSelector != null)
                    verticalSelector.Visibility = Visibility.Collapsed;
            }
            else if (Rows == 1)
            {
                if (allSelector != null)
                    allSelector.Visibility = Visibility.Collapsed;
                if (verticalSelector != null)
                    verticalSelector.Visibility = Visibility.Collapsed;
            }
            else if (Columns == 1)
            {
                if (allSelector != null)
                    allSelector.Visibility = Visibility.Collapsed;
                if (horizontalSelector != null)
                    horizontalSelector.Visibility = Visibility.Collapsed;
            }
        }

        private bool valueElementSelfSet;
        private bool deepValueSelfSet;

        private void OnElementPartPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (valueElementSelfSet)
                return;

            if (e.PropertyName == "IsChecked")
            {
                valueElementSelfSet = true;
                UpdateDependentCheck(sender as CheckablePart);
                valueElementSelfSet = false;
            }
            else if (e.PropertyName == "Value")
            {
                if (deepValueSelfSet)
                    return;

                var part = sender as ValuedCheckablePart;
                if (part != null)
                {
                    if (part.IsChecked)
                    {
                        deepValueSelfSet = true;

                        elementParts
                            .OfType<ValuedCheckablePart>()
                            .Where(elem => elem.IsChecked)
                            .ForEach(elem => elem.Value = part.Value);

                        deepValueSelfSet = false;
                    }
                }

                if (isValueType || ForceUpdateVectorSource)
                    OnVectorSourceValueChanged(part.instance.MemberwiseClone());
            }
        }

        private void UpdateDependentCheck(CheckablePart part)
        {
            valueElementSelfSet = true;

            if (part.Coordinate.X == -1 && part.Coordinate.Y == -1)
            {
                elementParts.ForEach(ve => ve.IsChecked = part.IsChecked);
            }
            else
            {
                if (part.Coordinate.X != -1 && part.Coordinate.Y != -1)
                {
                    UpdateDependentCheckX(part.Coordinate.X);
                    UpdateDependentCheckY(part.Coordinate.Y);
                    UpdateDependentCheckAll();
                }
                else
                {
                    if (part.Coordinate.X == -1)
                    {
                        elementParts
                            .Where(ve => ve.Coordinate.Y == part.Coordinate.Y)
                            .ForEach(ve => { ve.IsChecked = part.IsChecked; UpdateDependentCheckX(ve.Coordinate.X); });
                        UpdateDependentCheckAll();
                    }
                    if (part.Coordinate.Y == -1)
                    {
                        elementParts
                            .Where(ve => ve.Coordinate.X == part.Coordinate.X)
                            .ForEach(ve => { ve.IsChecked = part.IsChecked; UpdateDependentCheckY(ve.Coordinate.Y); });
                        UpdateDependentCheckAll();
                    }
                }
            }

            valueElementSelfSet = false;
        }

        private void UpdateDependentCheckX(int x)
        {
            elementParts
                .First(ve => ve.Coordinate.X == x && ve.Coordinate.Y == -1).IsChecked = elementParts
                .Where(ve => ve.Coordinate.X == x && ve.Coordinate.Y != -1)
                .All(ve => ve.IsChecked);
        }

        private void UpdateDependentCheckY(int y)
        {
            elementParts
                .First(ve => ve.Coordinate.X == -1 && ve.Coordinate.Y == y).IsChecked = elementParts
                .Where(ve => ve.Coordinate.X != -1 && ve.Coordinate.Y == y)
                .All(ve => ve.IsChecked);
        }

        private void UpdateDependentCheckAll()
        {
            elementParts
                .First(ve => ve.Coordinate.X == -1 && ve.Coordinate.Y == -1).IsChecked = elementParts
                .Where(ve => ve.Coordinate.X != -1 && ve.Coordinate.Y != -1)
                .All(ve => ve.IsChecked);
        }

        // === ForceUpdateVectorSource ===============================================================================================

        public bool ForceUpdateVectorSource
        {
            get { return (bool)GetValue(ForceUpdateVectorSourceProperty); }
            set { SetValue(ForceUpdateVectorSourceProperty, value); }
        }

        public static readonly DependencyProperty ForceUpdateVectorSourceProperty = DependencyProperty.Register(
            "ForceUpdateVectorSource",
            typeof(bool),
            typeof(VectorEditor),
            new PropertyMetadata(true));

        // === VectorSource ===============================================================================================

        private bool isValueType;
        public object VectorSource
        {
            get { return GetValue(VectorSourceProperty); }
            set { SetValue(VectorSourceProperty, value); }
        }

        public static readonly DependencyProperty VectorSourceProperty = DependencyProperty.Register(
            "VectorSource",
            typeof(object),
            typeof(VectorEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorSourcePropertyChanged));

        private static void OnVectorSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((VectorEditor)sender).OnVectorSourceChanged(e.OldValue, e.NewValue);
        }

        private bool selfSet;

        protected virtual void OnVectorSourceChanged(object oldValue, object newValue)
        {
            if (selfSet)
                return;

            isValueType = newValue is ValueType;
            SetupVectorLayout(oldValue, newValue);

            RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, VectorSourceChangedEvent));
        }

        private void SetupVectorLayout(object oldValue, object newValue)
        {
            if (newValue == null)
            {
                CleanupAllLayoutsParts();
                return;
            }

            bool needRelayout = oldValue == null // at initialization
                || oldValue.GetType() != newValue.GetType(); // the type of bound vector changed

            if (needRelayout)
                CreateAllLayoutParts();
            else if (object.Equals(oldValue, newValue) == false)
                UpdateValuedParts(newValue);
        }

        private void OnVectorSourceValueChanged(object newValue)
        {
            selfSet = true;
            SetCurrentValue(VectorSourceProperty, newValue);
            selfSet = false;
        }

        public static readonly RoutedEvent VectorSourceChangedEvent = EventManager.RegisterRoutedEvent("VectorSourceChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(VectorEditor));

        public event RoutedPropertyChangedEventHandler<object> VectorSourceChanged
        {
            add { AddHandler(VectorSourceChangedEvent, value); }
            remove { RemoveHandler(VectorSourceChangedEvent, value); }
        }

        // === ElementTemplate ===============================================================================================

        public DataTemplate ElementTemplate
        {
            get { return (DataTemplate)GetValue(ElementTemplateProperty); }
            set { SetValue(ElementTemplateProperty, value); }
        }

        public static readonly DependencyProperty ElementTemplateProperty = DependencyProperty.Register(
            "ElementTemplate",
            typeof(DataTemplate),
            typeof(VectorEditor));

        // === SelectorTemplate ===============================================================================================

        public DataTemplate SelectorTemplate
        {
            get { return (DataTemplate)GetValue(SelectorTemplateProperty); }
            set { SetValue(SelectorTemplateProperty, value); }
        }

        public static readonly DependencyProperty SelectorTemplateProperty = DependencyProperty.Register(
            "SelectorTemplate",
            typeof(DataTemplate),
            typeof(VectorEditor));

        // === MultiSelectorWidth ===============================================================================================

        public double MultiSelectorWidth
        {
            get { return (double)GetValue(MultiSelectorWidthProperty); }
            set { SetValue(MultiSelectorWidthProperty, value); }
        }

        public static readonly DependencyProperty MultiSelectorWidthProperty = DependencyProperty.Register(
            "MultiSelectorWidth",
            typeof(double),
            typeof(VectorEditor),
            new PropertyMetadata(16.0));

        // === MultiSelectorHeight ===============================================================================================

        public double MultiSelectorHeight
        {
            get { return (double)GetValue(MultiSelectorHeightProperty); }
            set { SetValue(MultiSelectorHeightProperty, value); }
        }

        public static readonly DependencyProperty MultiSelectorHeightProperty = DependencyProperty.Register(
            "MultiSelectorHeight",
            typeof(double),
            typeof(VectorEditor),
            new PropertyMetadata(16.0));

        // === IsUnknownType ===============================================================================================

        public bool IsUnknownType
        {
            get { return (bool)GetValue(IsUnknownTypeProperty); }
            private set { SetValue(IsUnknownTypePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsUnknownTypePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsUnknownType",
            typeof(bool),
            typeof(VectorEditor),
            new PropertyMetadata(true));
        public static readonly DependencyProperty IsUnknownTypeProperty = IsUnknownTypePropertyKey.DependencyProperty;

        // === Rows ===============================================================================================

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            private set { SetValue(RowsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey RowsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Rows",
            typeof(int),
            typeof(VectorEditor),
            new PropertyMetadata(0));
        public static readonly DependencyProperty RowsProperty = RowsPropertyKey.DependencyProperty;

        // === Columns ===============================================================================================

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            private set { SetValue(ColumnsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ColumnsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Columns",
            typeof(int),
            typeof(VectorEditor),
            new PropertyMetadata(0));
        public static readonly DependencyProperty ColumnsProperty = ColumnsPropertyKey.DependencyProperty;
    }
}
