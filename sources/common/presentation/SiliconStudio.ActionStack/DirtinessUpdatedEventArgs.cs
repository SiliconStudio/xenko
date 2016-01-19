// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.ActionStack
{
    public class DirtinessUpdatedEventArgs : EventArgs
    {
        public DirtinessUpdatedEventArgs(bool oldValue, bool newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public bool OldValue { get; }

        public bool NewValue { get; }

        public bool HasChanged => OldValue != NewValue;
    }
}