// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml;
using SharpYaml.Events;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;

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
            YamlSerializer.Serialize(new ParsingEventListEmitter(parsingEvents), Value, key.Type);

            return parsingEvents;
        }

        class ParsingEventListEmitter : IEmitter
        {
            private readonly List<ParsingEvent> parsingEvents;

            public ParsingEventListEmitter(List<ParsingEvent> parsingEvents)
            {
                this.parsingEvents = parsingEvents;
            }

            public void Emit(ParsingEvent @event)
            {
                // Ignore some events
                if (@event is StreamStart || @event is StreamEnd
                    || @event is DocumentStart || @event is DocumentEnd)
                    return;

                parsingEvents.Add(@event);
            }
        }

    }
}