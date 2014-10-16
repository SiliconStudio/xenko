// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.SampleApp
{
    public class NumericTextBoxViewModel : TextBoxViewModel
    {
        private double value;
        private double decimalPlaces = 2;
        private double minimum = -500;
        private double maximum = 500;
        private double valueRatio;
        private double largeChange = 10.0;
        private double smallChange = 1.0;
        private bool displayUpDownButtons = true;

        public double Value { get { return value; } set { SetValue(ref this.value, value); } }

        public double DecimalPlaces { get { return decimalPlaces; } set { SetValue(ref decimalPlaces, value); } }

        public double Minimum { get { return minimum; } set { SetValue(ref minimum, value); } }
        
        public double Maximum { get { return maximum; } set { SetValue(ref maximum, value); } }

        public double ValueRatio { get { return valueRatio; } set { SetValue(ref valueRatio, value); } }

        public double LargeChange { get { return largeChange; } set { SetValue(ref largeChange, value); } }

        public double SmallChange { get { return smallChange; } set { SetValue(ref smallChange, value); } }

        public bool DisplayUpDownButtons { get { return displayUpDownButtons; } set { SetValue(ref displayUpDownButtons, value); } }
    }
}