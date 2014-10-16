// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Presentation.Settings
{
    /// <summary>
    /// This class represents a set of settings that can be stored in a file. This class is public for serialization purpose only, and should not be used directly.
    /// </summary>
    [DataContract("SettingsFile")]
    public class SettingsFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFile"/> class.
        /// </summary>
        public SettingsFile()
        {
            Settings = new Dictionary<UFile, object>();
        }

        /// <summary>
        /// Gets the collection of settings to serialize.
        /// </summary>
        public Dictionary<UFile, object> Settings { get; private set; }
    }
}