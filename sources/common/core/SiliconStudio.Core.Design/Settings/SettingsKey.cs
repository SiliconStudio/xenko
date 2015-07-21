// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using SharpYaml.Events;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// This class represents property to store in the settings that is identified by a key.
    /// </summary>
    public abstract class SettingsKey
    {
        /// <summary>
        /// The default value of the settings key.
        /// </summary>
        protected readonly object DefaultObjectValue;

        /// <summary>
        /// The default value of the settings key.
        /// </summary>
        protected readonly Func<object> DefaultObjectValueCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey"/> class.
        /// </summary>
        /// <param name="name">The name of this settings key. Must be unique amongst the application.</param>
        /// <param name="group">The <see cref="SettingsGroup"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValue">The default value associated to this settings key.</param>
        protected SettingsKey(UFile name, SettingsGroup group, object defaultValue)
        {
            Name = name;
            DisplayName = name;
            DefaultObjectValue = defaultValue;
            Group = group;
            Group.RegisterSettingsKey(name, defaultValue, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey"/> class.
        /// </summary>
        /// <param name="name">The name of this settings key. Must be unique amongst the application.</param>
        /// <param name="group">The <see cref="SettingsGroup"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValueCallback">A function that returns the default value associated to this settings key.</param>
        protected SettingsKey(UFile name, SettingsGroup group, Func<object> defaultValueCallback)
        {
            Name = name;
            DisplayName = name;
            DefaultObjectValueCallback = defaultValueCallback;
            Group = group;
            Group.RegisterSettingsKey(name, defaultValueCallback(), this);
        }

        /// <summary>
        /// Gets the name of this <see cref="SettingsKey"/>.
        /// </summary>
        public UFile Name { get; private set; }

        /// <summary>
        /// Gets the type of this <see cref="SettingsKey"/>.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Gets the <see cref="SettingsGroup"/> containing this <see cref="SettingsKey"/>.
        /// </summary>
        public SettingsGroup Group { get; private set; }

        /// <summary>
        /// Gets or sets the display name of the <see cref="SettingsKey"/>.
        /// </summary>
        /// <remarks>The default value is the name parameter given to the constructor of this class.</remarks>
        public UFile DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of this <see cref="SettingsKey"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets an enumeration of acceptable values for this <see cref="SettingsKey"/>.
        /// </summary>
        public abstract IEnumerable<object> AcceptableValues { get; }

        /// <summary>
        /// Raised when the value of the settings key has been modified and the method <see cref="SettingsProfile.ValidateSettingsChanges"/> has been invoked.
        /// </summary>
        public event EventHandler<ChangesValidatedEventArgs> ChangesValidated;

        /// <summary>
        /// Converts a value of a different type to the type associated with this <see cref="SettingsKey"/>. If the conversion is not possible,
        /// this method will return the default value of the SettingsKey.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value if the conversion is possible, the default value otherwise.</returns>
        internal abstract object ConvertValue(List<ParsingEvent> value);

        /// <summary>
        /// Notifes that the changes have been validated by <see cref="SettingsProfile.ValidateSettingsChanges"/>.
        /// </summary>
        /// <param name="profile">The profile in which the change has been validated.</param>
        internal void NotifyChangesValidated(SettingsProfile profile)
        {
            var handler = ChangesValidated;
            if (handler != null)
                handler(this, new ChangesValidatedEventArgs(profile));
        }

        /// <summary>
        /// Gets the default value of this settings key.
        /// </summary>
        public abstract object DefaultValueObject { get; }

        /// <summary>
        /// Gets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsGroup.CurrentProfile"/>.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns>The value of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public virtual object GetValue(bool searchInParentProfile = true, SettingsProfile profile = null, bool createInCurrentProfile = false)
        {
            object value;
            profile = profile ?? Group.CurrentProfile;
            if (profile.GetValue(Name, out value, searchInParentProfile, createInCurrentProfile))
            {
                return value;
            }
            throw new KeyNotFoundException("Settings key not found");
        }

        /// <summary>
        /// Sets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        /// <param name="profile">The profile in which to set the value. If <c>null</c>, it will look in the <see cref="SettingsGroup.CurrentProfile"/>.</param>
        public void SetValue(object value, SettingsProfile profile = null)
        {
            profile = profile ?? Group.CurrentProfile;
            profile.SetValue(Name, value);
        }

        /// <summary>
        /// Tries to gets the value of this settings key in the given profile, if it exists.
        /// </summary>
        /// <param name="value">The resulting value, if found</param>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsGroup.CurrentProfile"/>.</param>
        /// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(out object value, bool searchInParentProfile = true, SettingsProfile profile = null)
        {
            profile = profile ?? Group.CurrentProfile;
            if (profile.GetValue(Name, out value, searchInParentProfile, false))
            {
                return true;
            }
            value = DefaultValueObject;
            return false;
        }

        /// <summary>
        /// Determines whether the specified profile contains key (without checking parent profiles).
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns></returns>
        public bool ContainsKey(SettingsProfile profile)
        {
            object value;
            return profile.GetValue(Name, out value, false, false);
        }
    }
}