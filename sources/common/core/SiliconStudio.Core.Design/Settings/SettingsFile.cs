// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
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

    /// <summary>
    /// An internal dictionary class used to serialize a <see cref="SettingsProfile"/>.
    /// </summary>
    internal class SettingsDictionary : Dictionary<UFile, List<ParsingEvent>>
    {
        // Used for temporary internal storage
        [DataMemberIgnore]
        internal SettingsProfile Profile;
    }
}