// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
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
            Settings = new SettingsDictionary();
        }

        /// <summary>
        /// Gets the collection of settings to serialize.
        /// </summary>
        [DataMemberCustomSerializer]
        public SettingsDictionary Settings { get; private set; }
    }

    public class SettingsDictionary : Dictionary<UFile, List<ParsingEvent>>
    {
        // Used for temporary internal storage
        [DataMemberIgnore]
        internal object Tags;
    }
}