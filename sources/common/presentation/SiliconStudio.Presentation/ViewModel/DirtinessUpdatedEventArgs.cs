// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.ViewModel
{
    public class DirtinessUpdatedEventArgs : EventArgs
    {
        public DirtinessUpdatedEventArgs(bool oldValue, bool newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public bool OldValue { get; private set; }

        public bool NewValue { get; private set; }

        public bool HasChanged { get { return OldValue != NewValue; } }
    }
}