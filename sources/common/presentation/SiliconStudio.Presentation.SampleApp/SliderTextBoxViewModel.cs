// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows.Media;

using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.Presentation.SampleApp
{
    public class SliderTextBoxViewModel : NumericTextBoxViewModel
    {
        private bool displayRangeIndicator = true;
        private Brush rangeIndicatorBrush = Brushes.DarkTurquoise;
        private bool isMouseChangeEnabled = true;
        private MouseValidationTrigger mouseValidationTrigger;

        public bool DisplayRangeIndicator { get { return displayRangeIndicator; } set { SetValue(ref displayRangeIndicator, value); } }

        public Brush RangeIndicatorBrush { get { return rangeIndicatorBrush; } set { SetValue(ref rangeIndicatorBrush, value); } }

        public bool IsMouseChangeEnabled { get { return isMouseChangeEnabled; } set { SetValue(ref isMouseChangeEnabled, value); } }

        public MouseValidationTrigger MouseValidationTrigger { get { return mouseValidationTrigger; } set { SetValue(ref mouseValidationTrigger, value); } }

    }
}