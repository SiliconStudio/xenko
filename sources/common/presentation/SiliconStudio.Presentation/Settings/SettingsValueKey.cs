// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.IO;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Settings
{
    /// <summary>
    /// This class represents a <see cref="SettingsKey"/> containing a single value of the specified type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of value contained in this settings key.</typeparam>
    public class SettingsValueKey<T> : SettingsKey
    {       
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsValueKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the settings key. Must be unique amongst an application.</param>
        /// <param name="defaultValue">The default value for this settings key.</param>
        public SettingsValueKey(UFile name, T defaultValue)
            : base(name, defaultValue)
        {
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(T); } }
        
        /// <summary>
        /// Gets the default value of this settings key.
        /// </summary>
        public T DefaultValue { get { return (T)DefaultObjectValue; } }

        /// <summary>
        /// Gets or sets a function that returns an enumation of acceptable values for this <see cref="SettingsValueKey{T}"/>.
        /// </summary>
        public Func<IEnumerable<T>> GetAcceptableValues { get; set; }

        /// <summary>
        /// Gets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns>The value of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public T GetValue(bool searchInParentProfile = true, SettingsProfile profile = null, bool createInCurrentProfile = false)
        {
            object value;
            profile = profile ?? SettingsService.CurrentProfile;
            if (profile.GetValue(Name, out value, searchInParentProfile, createInCurrentProfile))
            {
                return (T)value;
            }
            throw new KeyNotFoundException("Settings key not found");
        }

        /// <summary>
        /// Sets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        /// <param name="profile">The profile in which to set the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        public void SetValue(T value, SettingsProfile profile = null)
        {
            profile = profile ?? SettingsService.CurrentProfile;
            profile.SetValue(Name, value);
        }

        /// <summary>
        /// Tries to gets the value of this settings key in the given profile, if it exists.
        /// </summary>
        /// <param name="value">The resulting value, if found</param>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        /// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(out T value, bool searchInParentProfile = true, SettingsProfile profile = null)
        {
            object obj;
            profile = profile ?? SettingsService.CurrentProfile;
            if (profile.GetValue(Name, out obj, searchInParentProfile, false))
            {
                try
                {
                    value = (T)obj;
                    return true;
                }
                catch (Exception e)
                {
                    // The cast exception will fallback to the value unfound result.
                    e.Ignore();
                }
            }
            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        internal override object ConvertValue(object value)
        {
            return ConvertObject(value, DefaultValue);
        }
    }
}