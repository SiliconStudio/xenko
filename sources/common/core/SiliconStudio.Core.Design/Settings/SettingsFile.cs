// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal class that represents a set of settings that can be stored in a file.
    /// </summary>
    [DataContract("SettingsFile")]
    internal class SettingsFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFile"/> class.
        /// </summary>
        public SettingsFile(SettingsProfile profile)
        {
            Settings = profile;
        }

        /// <summary>
        /// Gets the settings profile to serialize.
        /// </summary>
        [DataMemberCustomSerializer]
        public SettingsProfile Settings { get; private set; }
    }
}
