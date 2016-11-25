// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal object that represent a single value for a settings key into a <see cref="SettingsProfile"/>.
    /// </summary>
    internal class SettingsEntryValue : SettingsEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEntryValue"/> class.
        /// </summary>
        /// <param name="profile">The profile this <see cref="SettingsEntryValue"/>belongs to.</param>
        /// <param name="name">The name associated to this <see cref="SettingsEntryValue"/>.</param>
        /// <param name="value">The value to associate to this <see cref="SettingsEntryValue"/>.</param>
        internal SettingsEntryValue(SettingsProfile profile, UFile name, object value)
            : base(profile, name)
        {
            Value = value;
            ShouldNotify = true;
        }

        /// <inheritdoc/>
        internal override List<ParsingEvent> GetSerializableValue(SettingsKey key)
        {
            // Value might have been kept as a parsing event list (if key didn't exist)
            var parsingEvents = Value as List<ParsingEvent>;
            if (parsingEvents != null)
                return parsingEvents;

            if (key == null)
                throw new InvalidOperationException();

            parsingEvents = new List<ParsingEvent>();
            SettingsYamlSerializer.Default.Serialize(new ParsingEventListEmitter(parsingEvents), Value, key.Type);

            return parsingEvents;
        }
    }
}
