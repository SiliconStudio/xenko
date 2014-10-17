// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using SiliconStudio.Core.Mathematics;

using Point = System.Windows.Point;

namespace SiliconStudio.Presentation.Legacy
{
    public class EditableTextBlock : TextBlock
    {
        private EditableTextBlockAdorner adorner;

        public EditableTextBlock()
        {
            Loaded += OnLoaded;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            KeyDown += OnKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                if (adorner != null)
                    adornerLayer.Remove(adorner);

                adorner = new EditableTextBlockAdorner(this);
                adornerLayer.Add(adorner);
            }

            if (adorner != null)
                adorner.SetEditingState(IsEditing);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EditOnDoubleClick)
            {
                if (e.ClickCount < 2)
                    return;

                IsEditing = true;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && EditWithF2)
            {
                IsEditing = true;
            }
        }

        // === IsEditing property ===================================================================================

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(false, OnIsEditingPropertyChanged));

        private static void OnIsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((EditableTextBlock)sender).OnIsEditingChanged((bool)e.NewValue);
        }

        private void OnIsEditingChanged(bool isEditing)
        {
            if (adorner != null)
                adorner.SetEditingState(isEditing);
        }

        // === EditWithF2 property ===============================================================================

        public bool EditWithF2
        {
            get { return (bool)GetValue(EditWithF2Property); }
            set { SetValue(EditWithF2Property, value); }
        }

        public static readonly DependencyProperty EditWithF2Property = DependencyProperty.Register(
            "EditWithF2",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(true));

        // === EditOnDoubleClick property ===========================================================================

        public bool EditOnDoubleClick
        {
            get { return (bool)GetValue(EditOnDoubleClickProperty); }
            set { SetValue(EditOnDoubleClickProperty, value); }
        }

        public static readonly DependencyProperty EditOnDoubleClickProperty = DependencyProperty.Register(
            "EditOnDoubleClick",
            typeof(bool),
            typeof(EditableTextBlock),
            new PropertyMetadata(true));
        
        // === EditableTextBlockAdorner class =======================================================================

        private class EditableTextBlockAdorner : Adorner
        {
            public EditableTextBlockAdorner(EditableTextBlock adornedElement)
                : base(adornedElement)
            {
                KeyDown += OnKeyDown;
            }

            private void OnKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    SetEditingState(false);
                }
            }

            private TextBox textBox;

            public void SetEditingState(bool isEditing)
            {
                if (isEditing == false)
                {
                    if (textBox != null)
                    {
                        var adorned = (EditableTextBlock)AdornedElement;
                        adorned.SetCurrentValue(TextProperty, textBox.Text);
                        textBox.LostFocus -= OnTextBoxLostFocus;

                        RemoveVisualChild(textBox);
                        RemoveLogicalChild(textBox);

                        textBox = null;
                    }
                }
                else
                {
                    if (textBox == null)
                    {
                        textBox = new TextBox();

                        var adorned = (EditableTextBlock)AdornedElement;
                        textBox.MinWidth = 30.0;
                        textBox.Text = adorned.Text;
                        textBox.LostFocus += OnTextBoxLostFocus;
                        textBox.Loaded += OnTextBoxLoaded;

                        AddLogicalChild(textBox);
                        AddVisualChild(textBox);

                        InvalidateMeasure();
                    }
                }
            }

            private void OnTextBoxLoaded(object sender, RoutedEventArgs e)
            {
                var control = (TextBox)sender;
                control.Focus();
                control.Loaded -= OnTextBoxLoaded;
            }

            private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
            {
                SetEditingState(false);
                AdornedElement.SetCurrentValue(IsEditingProperty, false);
            }

            protected override Size MeasureOverride(Size constraint)
            {
                if (textBox != null)
                {
                    constraint = new Size(
                        MathUtil.Clamp(AdornedElement.DesiredSize.Width, textBox.MinWidth, textBox.MaxWidth),
                        MathUtil.Clamp(AdornedElement.DesiredSize.Height, textBox.MinHeight, textBox.MaxHeight));
                    textBox.Measure(constraint);
                }

                return constraint;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                if (textBox != null)
                    textBox.Arrange(new Rect(new Point(-2.0, -2.0), finalSize)); // TODO: Make offset a DependencyProperty for finer tweaking

                return finalSize;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            protected override Visual GetVisualChild(int index)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException("index");

                return textBox;
            }

            protected override System.Collections.IEnumerator LogicalChildren
            {
                get
                {
                    yield return textBox;
                }
            }
        }
    }
}
