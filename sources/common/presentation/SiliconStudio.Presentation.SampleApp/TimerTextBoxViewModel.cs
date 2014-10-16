// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.SampleApp
{
    public class TimerTextBoxViewModel : TextBoxViewModel
    {
        private bool isTimedValidationEnabled = true;
        private int validationDelay = 500;

        public bool IsTimedValidationEnabled { get { return isTimedValidationEnabled; } set { SetValue(ref isTimedValidationEnabled, value); } }

        public int ValidationDelay { get { return validationDelay; } set { SetValue(ref validationDelay, value); } }
    }
}