// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SharpYaml;
using SharpYaml.Events;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;

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
        /// <param name="container">The <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValue">The default value associated to this settings key.</param>
        protected SettingsKey(UFile name, SettingsContainer container, object defaultValue)
        {
            Name = name;
            DisplayName = name;
            DefaultObjectValue = defaultValue;
            Container = container;
            Container.RegisterSettingsKey(name, defaultValue, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey"/> class.
        /// </summary>
        /// <param name="name">The name of this settings key. Must be unique amongst the application.</param>
        /// <param name="container">The <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValueCallback">A function that returns the default value associated to this settings key.</param>
        protected SettingsKey(UFile name, SettingsContainer container, Func<object> defaultValueCallback)
        {
            Name = name;
            DisplayName = name;
            DefaultObjectValueCallback = defaultValueCallback;
            Container = container;
            Container.RegisterSettingsKey(name, defaultValueCallback(), this);
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
        /// Gets the <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.
        /// </summary>
        public SettingsContainer Container { get; private set; }

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
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsContainer.CurrentProfile"/>.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns>The value of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public object GetValue(bool searchInParentProfile = true, SettingsProfile profile = null, bool createInCurrentProfile = false)
        {
            object value;
            profile = ResolveProfile(profile);
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
        /// <param name="profile">The profile in which to set the value. If <c>null</c>, it will look in the <see cref="SettingsContainer.CurrentProfile"/>.</param>
        public void SetValue(object value, SettingsProfile profile = null)
        {
            profile = ResolveProfile(profile);
            profile.SetValue(Name, value);
        }

        /// <summary>
        /// Tries to gets the value of this settings key in the given profile, if it exists, and without looking into
        /// the parent profiles.
        /// </summary>
        /// <param name="value">The resulting value, if found</param>
        /// <param name="profile">The profile in which to look for the value.</param>
        /// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(out object value, SettingsProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            profile = ResolveProfile(profile);
            if (profile.GetValue(Name, out value, false, false))
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

        /// <summary>
        /// Determines whether the specified profile contains key (without checking parent profiles).
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns></returns>
        public bool Remove(SettingsProfile profile)
        {
            return profile.Remove(Name);
        }

        protected SettingsProfile ResolveProfile(SettingsProfile profile = null)
        {
            profile = profile ?? Container.CurrentProfile;
            if (profile.Container != Container)
                throw new ArgumentException("This settings key has a different container that the given settings profile.");
            return profile;
        }
    }

    /// <summary>
    /// This class represents a <see cref="SettingsKey"/> containing a value of the specified type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of value contained in this settings key.</typeparam>
    public class SettingsKey<T> : SettingsKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the settings key. Must be unique amongst an application.</param>
        /// <param name="container">The <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.</param>
        public SettingsKey(UFile name, SettingsContainer container)
            : this(name, container, default(T))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the settings key. Must be unique amongst an application.</param>
        /// <param name="container">The <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValue">The default value for this settings key.</param>
        public SettingsKey(UFile name, SettingsContainer container, T defaultValue)
            : base(name, container, defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name of this settings key. Must be unique amongst the application.</param>
        /// <param name="container">The <see cref="SettingsContainer"/> containing this <see cref="SettingsKey"/>.</param>
        /// <param name="defaultValueCallback">A function that returns the default value associated to this settings key.</param>
        public SettingsKey(UFile name, SettingsContainer container, Func<object> defaultValueCallback)
            : base(name, container, defaultValueCallback)
        {
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(T); } }

        /// <summary>
        /// Gets the default value of this settings key.
        /// </summary>
        public T DefaultValue { get { return DefaultObjectValueCallback != null ? (T)DefaultObjectValueCallback() : (T)DefaultObjectValue; } }

        /// <inheritdoc/>
        public override object DefaultValueObject { get { return DefaultValue; } }

        /// <summary>
        /// Gets or sets a function that returns an enumation of acceptable values for this <see cref="SettingsKey{T}"/>.
        /// </summary>
        public Func<IEnumerable<T>> GetAcceptableValues { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<object> AcceptableValues { get { return GetAcceptableValues != null ? (IEnumerable<object>)GetAcceptableValues() : Enumerable.Empty<object>(); } }

        /// <summary>
        /// Gets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsContainer.CurrentProfile"/>.</param>
        /// <returns>The value of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public T GetValue(bool searchInParentProfile, SettingsProfile profile)
        {
            object value;
            profile = ResolveProfile(profile);
            if (profile.GetValue(Name, out value, searchInParentProfile, false))
            {
                return (T)value;
            }
            throw new KeyNotFoundException("Settings key not found");
        }

        /// <summary>
        /// Gets the value of this settings key in the current profile.
        /// </summary>
        /// <returns>The value of this settings key.</returns>
        public T GetValue()
        {
            object value;
            var profile = ResolveProfile();
            if (profile.GetValue(Name, out value, true, false))
            {
                return (T)value;
            }
            // This should never happen
            throw new KeyNotFoundException("Settings key not found");
        }

        /// <summary>
        /// Sets the value of this settings key in the given profile.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        /// <param name="profile">The profile in which to set the value. Must be a non-null that uses the same <see cref="SettingsContainer"/> that this <see cref="SettingsKey"/>.</param>
        public void SetValue(T value, SettingsProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            profile = ResolveProfile(profile);
            profile.SetValue(Name, value);
        }

        /// <summary>
        /// Sets the value of this settings key in the current profile.
        /// </summary>
        /// <param name="value">The new value to set.</param>
        public void SetValue(T value)
        {
            var profile = ResolveProfile();
            profile.SetValue(Name, value);
        }

        /// <summary>
        /// Tries to gets the value of this settings key in the given profile, if it exists, and without looking into
        /// the parent profiles.
        /// </summary>
        /// <param name="value">The resulting value, if found</param>
        /// <param name="profile">The profile in which to look for the value.</param>
        /// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(out T value, SettingsProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            object obj;
            profile = ResolveProfile(profile);
            if (profile.GetValue(Name, out obj, false, false))
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
            value = DefaultValue;
            return false;
        }

        /// <inheritdoc/>
        internal override object ConvertValue(List<ParsingEvent> parsingEvents)
        {
            try
            {
                var eventReader = new EventReader(new MemoryParser(parsingEvents));
                return YamlSerializer.Deserialize(eventReader, Type);
            }
            catch (Exception)
            {
                // Can't decode back, use default value
                return DefaultValue;
            }
        }
    }
}