// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.SampleApp
{
    public class TextBoxViewModel : ViewModelBase
    {
        private string text;
        private int validationCount;
        private int cancellationCount;
        private bool validateWithEnter = true;
        private bool validateOnTextChange;
        private bool validateOnLostFocus = true;
        private bool cancelWithEscape = true;
        private bool cancelOnLostFocus;

        public TextBoxViewModel()
        {
            ValidateCommand = new AnonymousCommand(null, () => ++ValidationCount);
            CancelCommand = new AnonymousCommand(null, () => ++CancellationCount);
        }

        public string Text { get { return text; } set { SetValue(ref text, value); } }

        public bool ValidateWithEnter { get { return validateWithEnter; } set { SetValue(ref validateWithEnter, value); } }

        public bool ValidateOnTextChange { get { return validateOnTextChange; } set { SetValue(ref validateOnTextChange, value); } }

        public bool ValidateOnLostFocus { get { return validateOnLostFocus; } set { SetValue(ref validateOnLostFocus, value); } }

        public bool CancelWithEscape { get { return cancelWithEscape; } set { SetValue(ref cancelWithEscape, value); } }

        public bool CancelOnLostFocus { get { return cancelOnLostFocus; } set { SetValue(ref cancelOnLostFocus, value); } }

        public int ValidationCount { get { return validationCount; } set { SetValue(ref validationCount, value); } }

        public int CancellationCount { get { return cancellationCount; } set { SetValue(ref cancellationCount, value); } }

        public ICommandBase ValidateCommand { get; private set; }

        public ICommandBase CancelCommand { get; private set; }
    }
}
