// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// Arguments of the <see cref="SettingsKey.ChangesValidated"/> event.
    /// </summary>
    public class ChangesValidatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangesValidatedEventArgs"/> class.
        /// </summary>
        /// <param name="profile">The profile in which changes have been validated.</param>
        public ChangesValidatedEventArgs(SettingsProfile profile)
        {
            Profile = profile;
        }

        /// <summary>
        /// Gets the <see cref="SettingsProfile"/> in which changes have been validated.
        /// </summary>
        public SettingsProfile Profile { get; private set; }
    }
}
