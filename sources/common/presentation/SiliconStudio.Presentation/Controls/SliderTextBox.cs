// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// A specialization of the <see cref="NumericTextBox"/> class that allows to uses the mouse to drag the value in the range defined by the
    /// <see cref="NumericTextBox.Minimum"/> and the <see cref="NumericTextBox.Maximum"/> properties, just like a <see cref="Slider"/>.
    /// </summary>
    public class SliderTextBox : NumericTextBox
    {

        static SliderTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderTextBox), new FrameworkPropertyMetadata(typeof(SliderTextBox)));
        }

        /// <summary>
        /// Identifies the <see cref="DisplayRangeIndicator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayRangeIndicatorProperty = DependencyProperty.Register("DisplayRangeIndicator", typeof(bool), typeof(SliderTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="RangeIndicatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeIndicatorBrushProperty = DependencyProperty.Register("RangeIndicatorBrush", typeof(Brush), typeof(SliderTextBox), new FrameworkPropertyMetadata(Brushes.CornflowerBlue));
        
        /// <summary>
        /// Gets or sets whether to display the range indicator at the bottom of the <see cref="SliderTextBox"/>.
        /// </summary>
        public bool DisplayRangeIndicator { get { return (bool)GetValue(DisplayRangeIndicatorProperty); } set { SetValue(DisplayRangeIndicatorProperty, value); } }

        /// <summary>
        /// Gets or sets the brush to use for the range indicator.
        /// </summary>
        public Brush RangeIndicatorBrush { get { return (Brush)GetValue(RangeIndicatorBrushProperty); } set { SetValue(RangeIndicatorBrushProperty, value); } }
    }
}
