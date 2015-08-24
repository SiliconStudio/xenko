// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Settings
{
    [DataSerializer(typeof(SettingsCollection.Serializer))]
    public class SettingsCollection
    {
        private readonly SettingsProfile profile;

        public SettingsCollection(SettingsContainer settingsContainer)
        {
            profile = settingsContainer.CreateSettingsProfile(false);
        }

        public SettingsProfile Profile { get { return profile; } }

        /// <summary>
        /// Gets a value for the specified key, null if not found.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>a value for the specified key, null if not found.</returns>
        public T Get<T>(SettingsValueKey<T> key)
        {
            T result;
            key.TryGetValue(out result, false, profile);
            return result;
        }

        /// <summary>
        /// Gets a value for the specified key, null if not found.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>a value for the specified key, null if not found.</returns>
        public object Get(SettingsKey key)
        {
            object result;
            key.TryGetValue(out result, false, profile);
            return result;
        }

        /// <summary>
        /// Sets a value for the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(SettingsValueKey<T> key, T value)
        {
            key.SetValue(value, profile);
        }

        /// <summary>
        /// Sets a value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(SettingsKey key, object value)
        {
            key.SetValue(value, profile);
        }

        public object this[SettingsKey key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public bool ContainsKey(SettingsKey key)
        {
            return key.ContainsKey(profile);
        }

        public bool Remove(SettingsKey key)
        {
            return key.Remove(profile);
        }

        public void CopyTo(SettingsCollection settings, bool overrideValues)
        {
            foreach (var setting in profile.Settings)
            {
                if (!overrideValues && settings.profile.Settings.ContainsKey(setting.Key))
                {
                    continue;
                }
                settings.profile.SetValue(setting.Key, setting.Value.Value);
            }
        }

        internal class Serializer : DataSerializer<SettingsCollection>
        {
            public override void Serialize(ref SettingsCollection obj, ArchiveMode mode, SerializationStream stream)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}