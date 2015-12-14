// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpYaml.Events;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// A container object that contains a collection of <see cref="SettingsKey"/>. Each settings key can store a corresponding value into a <see cref="SettingsProfile"/>.
    /// When a <see cref="SettingsContainer"/> is created, it will contain a default root <see cref="SettingsProfile"/>. This profile has no parent, and every profile created
    /// or loaded afterward will have the default profile as parent, unless another non-null parent is specified.
    /// </summary>
    public class SettingsContainer
    {
        /// <summary>
        /// A dictionary containing every existing <see cref="SettingsKey"/>.
        /// </summary>
        private readonly Dictionary<UFile, SettingsKey> settingsKeys = new Dictionary<UFile, SettingsKey>();

        /// <summary>
        /// A <see cref="SettingsProfile"/> that contains the default value of all registered <see cref="SettingsKey"/>.
        /// </summary>
        private readonly SettingsProfile rootProfile;

        /// <summary>
        /// A list containing every <see cref="SettingsProfile"/> registered in the <see cref="SettingsContainer"/>.
        /// </summary>
        private readonly List<SettingsProfile> profileList = new List<SettingsProfile>();

        internal static readonly object SettingsLock = new object();
        /// <summary>
        /// The settings profile that is currently active.
        /// </summary>
        private SettingsProfile currentProfile;

        public SettingsContainer()
        {
            rootProfile = new SettingsProfile(this, null);
            profileList.Add(rootProfile);
            currentProfile = rootProfile;
            Logger = new LoggerResult();
        }

        /// <summary>
        /// Gets the logger associated to the <see cref="SettingsContainer"/>.
        /// </summary>
        public LoggerResult Logger { get; }

        /// <summary>
        /// Gets the root profile of this settings container.
        /// </summary>
        public SettingsProfile RootProfile => rootProfile;

        /// <summary>
        /// Gets or sets the <see cref="SettingsProfile"/> that is currently active.
        /// </summary>
        public SettingsProfile CurrentProfile { get { return currentProfile; } set { ChangeCurrentProfile(currentProfile, value); } }

        /// <summary>
        /// Gets the list of registered profiles.
        /// </summary>
        public IEnumerable<SettingsProfile> Profiles => profileList;

        /// <summary>
        /// Raised when a settings file has been loaded.
        /// </summary>
        public event EventHandler<SettingsFileLoadedEventArgs> SettingsFileLoaded;

        /// <summary>
        /// Gets a list of all registered <see cref="SettingsKey"/> instances.
        /// </summary>
        /// <returns>A list of all registered <see cref="SettingsKey"/> instances.</returns>
        public List<SettingsKey> GetAllSettingsKeys()
        {
            return settingsKeys.Values.ToList();
        }

        /// <summary>
        /// Creates a new settings profile.
        /// </summary>
        /// <param name="setAsCurrent">If <c>true</c>, the created profile will also be set as <see cref="CurrentProfile"/>.</param>
        /// <param name="parent">The parent profile of the settings to create. If <c>null</c>, the default profile will be used.</param>
        /// <returns>A new instance of the <see cref="SettingsProfile"/> class.</returns>
        public SettingsProfile CreateSettingsProfile(bool setAsCurrent, SettingsProfile parent = null)
        {
            var profile = new SettingsProfile(this, parent ?? rootProfile);
            lock (SettingsLock)
            {
                profileList.Add(profile);
                if (setAsCurrent)
                    CurrentProfile = profile;
            }
            return profile;
        }

        /// <summary>
        /// Loads a settings profile from the given file.
        /// </summary>
        /// <param name="filePath">The path of the file from which to load settings.</param>
        /// <param name="setAsCurrent">If <c>true</c>, the loaded profile will also be set as <see cref="CurrentProfile"/>.</param>
        /// <param name="parent">The profile to use as parent for the loaded profile. If <c>null</c>, a default profile will be used.</param>
        /// <returns><c>true</c> if settings were correctly loaded, <c>false</c> otherwise.</returns>
        public SettingsProfile LoadSettingsProfile(UFile filePath, bool setAsCurrent, SettingsProfile parent = null)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
            {
                Logger.Error("Settings file [{0}] was not found", filePath);
                return null;
            }

            var profile = new SettingsProfile(this, parent ?? rootProfile) { FilePath = filePath };
            try
            {
                var settingsFile = new SettingsFile(profile);
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    YamlSerializer.Deserialize(stream, settingsFile);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading settings file [{0}]: {1}", e, filePath, e.FormatFull());
                return null;
            }

            lock (SettingsLock)
            {
                profileList.Add(profile);
                if (setAsCurrent)
                {
                    CurrentProfile = profile;
                }
            }
            
            var handler = SettingsFileLoaded;
            handler?.Invoke(null, new SettingsFileLoadedEventArgs(filePath));
            return profile;
        }

        /// <summary>
        /// Reloads a profile from its file, updating the value that have changed.
        /// </summary>
        /// <param name="profile">The profile to reload.</param>
        public void ReloadSettingsProfile(SettingsProfile profile)
        {
            var filePath = profile.FilePath;
            if (filePath == null) throw new ArgumentException("profile");
            if (!File.Exists(filePath))
            {
                Logger.Error("Settings file [{0}] was not found", filePath);
                throw new ArgumentException("profile");
            }

            try
            {
                var settingsFile = new SettingsFile(profile);
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    YamlSerializer.Deserialize(stream, settingsFile);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading settings file [{0}]: {1}", e, filePath, e.FormatFull());
            }

            var handler = SettingsFileLoaded;
            handler?.Invoke(null, new SettingsFileLoadedEventArgs(filePath));
        }

        /// <summary>
        /// Unloads a profile that was previously loaded.
        /// </summary>
        /// <param name="profile">The profile to unload.</param>
        public void UnloadSettingsProfile(SettingsProfile profile)
        {
            if (profile == rootProfile)
                throw new ArgumentException("The default profile cannot be unloaded");
            if (profile == CurrentProfile)
                throw new InvalidOperationException("Unable to unload the current profile.");
            lock (SettingsLock)
            {
                profileList.Remove(profile);
            }
        }

        /// <summary>
        /// Saves the given settings profile to a file at the given path.
        /// </summary>
        /// <param name="profile">The profile to save.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <returns><c>true</c> if the file was correctly saved, <c>false</c> otherwise.</returns>
        public bool SaveSettingsProfile(SettingsProfile profile, UFile filePath)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            try
            {
                profile.Saving = true;
                Directory.CreateDirectory(filePath.GetFullDirectory());

                var settingsFile = new SettingsFile(profile);
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    YamlSerializer.Serialize(stream, settingsFile);
                }

                if (filePath != profile.FilePath)
                {
                    if (File.Exists(profile.FilePath))
                    {
                        File.Delete(profile.FilePath);
                    }

                    profile.FilePath = filePath;
                }               
            }
            catch (Exception e)
            {
                Logger.Error("Error while saving settings file [{0}]: {1}", e, filePath, e.FormatFull());
                return false;
            }
            finally
            {
                profile.Saving = false;
            }
            return true;
        }

        internal void EncodeSettings(SettingsProfile profile, SettingsDictionary settingsDictionary)
        {
            lock (SettingsLock)
            {
                foreach (var entry in profile.Settings.Values)
                {
                    try
                    {
                        // Find key
                        SettingsKey key;
                        settingsKeys.TryGetValue(entry.Name, out key);
                        settingsDictionary.Add(entry.Name, entry.GetSerializableValue(key));
                    }
                    catch (Exception e)
                    {
                        e.Ignore();
                    }
                }
            }
        }

        internal void DecodeSettings(SettingsDictionary settingsDictionary, SettingsProfile profile)
        {
            lock (SettingsLock)
            {
                foreach (var settings in settingsDictionary)
                {
                    SettingsKey key;
                    var value = settings.Value;
                    object finalValue = value;
                    if (settingsKeys.TryGetValue(settings.Key, out key))
                    {
                        finalValue = key.ConvertValue(value);
                    }
                    profile.SetValue(settings.Key, finalValue);
                }
            }
        }

        /// <summary>
        /// Gets the settings key that matches the given name.
        /// </summary>
        /// <param name="name">The name of the settings property to fetch.</param>
        /// <returns>The settings key that matches the given name, or <c>null</c>.</returns>
        public SettingsKey GetSettingsKey(UFile name)
        {
            lock (SettingsLock)
            {
                SettingsKey key;
                settingsKeys.TryGetValue(name, out key);
                return key;
            }
        }

        /// <summary>
        /// Clears the current settings, by removing registered <see cref="SettingsKey"/> and <see cref="SettingsProfile"/> instances. This method should be used only for tests.
        /// </summary>
        public void ClearSettings()
        {
            lock (SettingsLock)
            {
                CurrentProfile = rootProfile;
                CurrentProfile.ValidateSettingsChanges();
                profileList.Clear();
                rootProfile.Settings.Clear();
                settingsKeys.Clear();
            }
        }

        internal void RegisterSettingsKey(UFile name, object defaultValue, SettingsKey settingsKey)
        {
            lock (SettingsLock)
            {
                settingsKeys.Add(name, settingsKey);
                var entry = SettingsEntry.CreateFromValue(rootProfile, name, defaultValue);
                rootProfile.RegisterEntry(entry);

                // Ensure that the value is converted to the key type in each loaded profile.
                foreach (var profile in Profiles.Where(x => x != rootProfile))
                {
                    if (profile.Settings.TryGetValue(name, out entry))
                    {
                        var parsingEvents = entry.Value as List<ParsingEvent>;
                        var convertedValue = parsingEvents != null ? settingsKey.ConvertValue(parsingEvents) : entry.Value;
                        entry = SettingsEntry.CreateFromValue(profile, name, convertedValue);
                        profile.Settings[name] = entry;
                    }
                }
            }
        }

        private void ChangeCurrentProfile(SettingsProfile oldProfile, SettingsProfile newProfile)
        {
            if (oldProfile == null) throw new ArgumentNullException(nameof(oldProfile));
            if (newProfile == null) throw new ArgumentNullException(nameof(newProfile));
            currentProfile = newProfile;

            lock (SettingsLock)
            {
                foreach (var key in settingsKeys)
                {
                    object oldValue;
                    oldProfile.GetValue(key.Key, out oldValue, true, false);
                    object newValue;
                    newProfile.GetValue(key.Key, out newValue, true, false);
                    var oldList = oldValue as IList;
                    var newList = newValue as IList;
                    var oldDictionary = oldValue as IDictionary;
                    var newDictionary = newValue as IDictionary;
                    bool isDifferent;
                    if (oldList != null && newList != null)
                    {
                        isDifferent = oldList.Count != newList.Count;
                        for (int i = 0; i < oldList.Count && !isDifferent; ++i)
                        {
                            if (!Equals(oldList[i], newList[i]))
                                isDifferent = true;
                        }
                    }
                    else if (oldDictionary != null && newDictionary != null)
                    {
                        isDifferent = oldDictionary.Count != newDictionary.Count;
                        foreach (var k in oldDictionary.Keys)
                        {
                            if (!newDictionary.Contains(k) || !Equals(oldDictionary[k], newDictionary[k]))
                                isDifferent = true;
                        }
                    }
                    else
                    {
                        isDifferent = !Equals(oldValue, newValue);
                    }
                    if (isDifferent)
                    {
                        newProfile.NotifyEntryChanged(key.Key);
                    }
                }
            }

            // Changes have been notified, empty the list of modified settings.
            newProfile.ValidateSettingsChanges();
        }
    }
}