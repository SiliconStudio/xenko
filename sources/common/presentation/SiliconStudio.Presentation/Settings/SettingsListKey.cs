// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.IO;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Settings
{
    /// <summary>
    /// This is the base class to represent a <see cref="SettingsKey"/> containing a list of values.
    /// </summary>
    public abstract class SettingsListKey : SettingsKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsListKey"/> class.
        /// </summary>
        /// <param name="name">The name of the settings key. Must be unique amongst an application.</param>
        /// <param name="defaultValue">The default values for this settings key.</param>
        protected SettingsListKey(UFile name, object defaultValue)
            : base(name, defaultValue)
        {
        }

        /// <summary>
        /// Gets the non-generic list of values of this settings key in the current profile.
        /// </summary>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns>The non-generic list of values of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public IList GetNonGenericList(bool searchInParentProfile = true, SettingsProfile profile = null, bool createInCurrentProfile = false)
        {
            profile = profile ?? SettingsService.CurrentProfile;
            object value;
            if (profile.GetValue(Name, out value, searchInParentProfile, createInCurrentProfile))
            {
                var list = value as IList;
                if (list != null)
                {
                    return list;
                }
            }

            throw new KeyNotFoundException("Settings key not found");
        }
    }

    /// <summary>
    /// This class represents a <see cref="SettingsKey"/> containing a list of values of the given type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the list contained in this key.</typeparam>
    public class SettingsListKey<T> : SettingsListKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsListKey{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the settings key. Must be unique amongst an application.</param>
        /// <param name="defaultValue">The default values for this settings key.</param>
        public SettingsListKey(UFile name, IEnumerable<T> defaultValue)
            : base(name, defaultValue.SafeArgument("defaultValue").ToList())
        {
            if (defaultValue == null) throw new ArgumentNullException("defaultValue");
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(IList<T>); } }
        
        /// <summary>
        /// Gets the default values of this settings key.
        /// </summary>
        public IEnumerable<T> DefaultValue { get { return (IEnumerable<T>)DefaultObjectValue; } }

        /// <summary>
        /// Gets the list of values of this settings key in the current profile.
        /// </summary>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns>The list of values of this settings key.</returns>
        /// <exception cref="KeyNotFoundException">No value can be found in the given profile matching this settings key.</exception>
        public IList<T> GetList(bool searchInParentProfile = true, SettingsProfile profile = null, bool createInCurrentProfile = false)
        {
            var list = GetNonGenericList(searchInParentProfile, profile, createInCurrentProfile) as IList<T>;
            if (list != null)
            {
                return list;
            }
            throw new KeyNotFoundException("Settings key not found");
        }

        /// <summary>
        /// Tries to gets the list of values of this settings key in the current profile.
        /// </summary>
        /// <param name="list">The resulting value, if found</param>
        /// <param name="searchInParentProfile">If true, the settings service will look in the parent profile of the given profile if the settings key is not defined into it.</param>
        /// <param name="profile">The profile in which to look for the value. If <c>null</c>, it will look in the <see cref="SettingsService.CurrentProfile"/>.</param>
        /// <returns><c>true</c> if the list was found, <c>false</c> otherwise.</returns>
        public bool TryGetList(out IList<T> list, bool searchInParentProfile = true, SettingsProfile profile = null)
        {
            profile = profile ?? SettingsService.CurrentProfile;
            object value;
            if (profile.GetValue(Name, out value, searchInParentProfile, false))
            {
                list = value as IList<T>;
                return list != null;
            }
            list = null;
            return false;
        }
        
        /// <inheritdoc/>
        internal override object ConvertValue(object value)
        {
            var result = new List<T>();
            try
            {
                var list = value as IList;
                if (list == null)
                    throw new Exception();

                foreach (object item in list)
                {
                    var newValue = ConvertObject(item, default(T));
                    result.Add(newValue);
                }
            }
            catch (Exception)
            {
                result = new List<T>(DefaultValue);
                return result;
            }
            return result;
        }
    }
}
