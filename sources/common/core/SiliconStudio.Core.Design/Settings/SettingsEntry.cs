// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An internal object that represent a value for a settings key into a <see cref="SettingsProfile"/>.
    /// </summary>
    internal abstract class SettingsEntry
    {
        protected readonly SettingsProfile Profile;
        protected bool ShouldNotify;
        private object value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEntry"/> class.
        /// </summary>
        /// <param name="profile">The profile this <see cref="SettingsEntry"/>belongs to.</param>
        /// <param name="name">The name associated to this <see cref="SettingsEntry"/>.</param>
        protected SettingsEntry(SettingsProfile profile, UFile name)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (name == null) throw new ArgumentNullException(nameof(name));
            Profile = profile;
            Name = name;
        }

        /// <summary>
        /// Gets the name of this <see cref="SettingsEntry"/>.
        /// </summary>
        internal UFile Name { get; }

        /// <summary>
        /// Gets or sets the value of this <see cref="SettingsEntry"/>.
        /// </summary>
        internal object Value { get { return value; } set { UpdateValue(value); } }

        /// <summary>
        /// Creates a new instance of a class derived from <see cref="SettingsEntry"/> that matches the type of the given value. 
        /// </summary>
        /// <param name="profile">The profile the <see cref="SettingsEntry"/> to create belongs to.</param>
        /// <param name="name">The name associated to the <see cref="SettingsEntry"/> to create.</param>
        /// <param name="value">The value to associate to the <see cref="SettingsEntry"/> to create.</param>
        /// <returns>A new instance of a <see cref="SettingsEntry"/> class.</returns>
        internal static SettingsEntry CreateFromValue(SettingsProfile profile, UFile name, object value)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new SettingsEntryValue(profile, name, value);
        }

        /// <summary>
        /// Gets the value of this entry converted to a serializable type.
        /// </summary>
        /// <returns></returns>
        internal abstract List<ParsingEvent> GetSerializableValue(SettingsKey key);

        private void UpdateValue(object newValue)
        {
            var oldValue = value;
            bool changed = !Equals(oldValue, newValue);
            if (changed && ShouldNotify && !Profile.IsDiscarding)
            {
                using (Profile.TransactionStack.CreateTransaction())
                {
                    Profile.TransactionStack.PushOperation(new SettingsEntryChangeValueOperation(this, oldValue));
                }
                Profile.NotifyEntryChanged(Name);
            }
            value = newValue;
        }
    }
}