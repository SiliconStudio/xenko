// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Settings
{
    /// <summary>
    /// A static class that manages settings loading and saving for an application.
    /// </summary>
    public static class SettingsService
    {
        /// <summary>
        /// A dictionary containing every existing <see cref="SettingsKey"/>.
        /// </summary>
        private static readonly Dictionary<UFile, SettingsKey> SettingsKeys = new Dictionary<UFile, SettingsKey>();

        /// <summary>
        /// A <see cref="SettingsProfile"/> that contains the default value of all registered <see cref="SettingsKey"/>.
        /// </summary>
        private static readonly SettingsProfile DefaultProfile = new SettingsProfile(null);

        private static readonly List<SettingsProfile> ProfileList = new List<SettingsProfile>();

        private static SettingsProfile currentProfile;

        static SettingsService()
        {
            ProfileList.Add(DefaultProfile);
            currentProfile = DefaultProfile;
            Logger = new LoggerResult();
        }

        /// <summary>
        /// Gets the logger associated to the <see cref="SettingsService"/>.
        /// </summary>
        public static LoggerResult Logger { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="SettingsProfile"/> that is currently active.
        /// </summary>
        public static SettingsProfile CurrentProfile { get { return currentProfile; } set { ChangeCurrentProfile(currentProfile, value); } }

        /// <summary>
        /// Gets the list of registered profiles.
        /// </summary>
        public static IEnumerable<SettingsProfile> Profiles { get { return ProfileList; } }

        /// <summary>
        /// Raised when a settings file has been loaded.
        /// </summary>
        public static event EventHandler<SettingsFileLoadedEventArgs> SettingsFileLoaded;

        /// <summary>
        /// Gets a list of all registered <see cref="SettingsKey"/> instances.
        /// </summary>
        /// <param name="includeNonEditable">Inidcates whether to include or not settings key which have the <see cref="SettingsKey.IsEditable"/> property set to <c>false</c>.</param>
        /// <returns>A list of all registered <see cref="SettingsKey"/> instances.</returns>
        public static List<SettingsKey> GetAllSettingsKeys(bool includeNonEditable)
        {
            return (includeNonEditable ? SettingsKeys.Values : SettingsKeys.Values.Where(x => x.IsEditable)).ToList();
        }

        /// <summary>
        /// Creates a new settings profile.
        /// </summary>
        /// <param name="setAsCurrent">If <c>true</c>, the created profile will also be set as <see cref="CurrentProfile"/>.</param>
        /// <param name="parent">The parent profile of the settings to create. If <c>null</c>, a default profile will be used.</param>
        /// <returns>A new instance of the <see cref="SettingsProfile"/> class.</returns>
        public static SettingsProfile CreateSettingsProfile(bool setAsCurrent, SettingsProfile parent = null)
        {
            var profile = new SettingsProfile(parent ?? DefaultProfile);
            ProfileList.Add(profile);
            if (setAsCurrent)
                CurrentProfile = profile;

            return profile;
        }

        /// <summary>
        /// Loads a settings profile from the given file.
        /// </summary>
        /// <param name="filePath">The path of the file from which to load settings.</param>
        /// <param name="setAsCurrent">If <c>true</c>, the loaded profile will also be set as <see cref="CurrentProfile"/>.</param>
        /// <param name="parent">The profile to use as parent for the loaded profile. If <c>null</c>, a default profile will be used.</param>
        /// <returns><c>true</c> if settings were correctly loaded, <c>false</c> otherwise.</returns>
        public static SettingsProfile LoadSettingsProfile(UFile filePath, bool setAsCurrent, SettingsProfile parent = null)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
            {
                Logger.Error("Settings file [{0}] was not found", filePath);
                return null;
            }

            SettingsProfile profile;
            try
            {
                SettingsFile settingsFile;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    settingsFile = (SettingsFile)YamlSerializer.Deserialize(stream);
                }
                profile = new SettingsProfile(parent ?? DefaultProfile) { FilePath = filePath };

                foreach (var settings in settingsFile.Settings)
                {
                    SettingsKey key;
                    var value = settings.Value;
                    if (SettingsKeys.TryGetValue(settings.Key, out key))
                    {
                        value = key.ConvertValue(value);
                    }
                    profile.SetValue(settings.Key, value);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading settings file [{0}]: {1}", e, filePath, e.FormatForReport());
                return null;
            }

            ProfileList.Add(profile);
            if (setAsCurrent)
            {
                CurrentProfile = profile;
            }
            
            var handler = SettingsFileLoaded;
            if (handler != null)
            {
                SettingsFileLoaded(null, new SettingsFileLoadedEventArgs(filePath));
            }
            return profile;
        }

        /// <summary>
        /// Reloads a profile from its file, updating the value that have changed.
        /// </summary>
        /// <param name="profile">The profile to reload.</param>
        public static void ReloadSettingsProfile(SettingsProfile profile)
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
                SettingsFile settingsFile;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    settingsFile = (SettingsFile)YamlSerializer.Deserialize(stream);
                }

                foreach (var settings in settingsFile.Settings)
                {
                    SettingsKey key;
                    var value = settings.Value;
                    if (SettingsKeys.TryGetValue(settings.Key, out key))
                    {
                        value = key.ConvertValue(value);
                    }
                    profile.SetValue(settings.Key, value);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while loading settings file [{0}]: {1}", e, filePath, e.FormatForReport());
            }

            var handler = SettingsFileLoaded;
            if (handler != null)
            {
                SettingsFileLoaded(null, new SettingsFileLoadedEventArgs(filePath));
            }
        }

        /// <summary>
        /// Unloads a profile that was previously loaded.
        /// </summary>
        /// <param name="profile">The profile to unload.</param>
        public static void UnloadSettingsProfile(SettingsProfile profile)
        {
            if (profile == DefaultProfile)
                throw new ArgumentException("The default profile cannot be unloaded");
            if (profile == CurrentProfile)
                throw new InvalidOperationException("Unable to unload the current profile.");
            ProfileList.Remove(profile);
        }

        /// <summary>
        /// Saves the given settings profile to a file at the given path.
        /// </summary>
        /// <param name="profile">The profile to save.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <returns><c>true</c> if the file was correctly saved, <c>false</c> otherwise.</returns>
        public static bool SaveSettingsProfile(SettingsProfile profile, UFile filePath)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            try
            {
                profile.Saving = true;
                Directory.CreateDirectory(filePath.GetFullDirectory());

                var settingsFile = new SettingsFile();
                foreach (var entry in profile.Settings.Values)
                {
                    settingsFile.Settings.Add(entry.Name, entry.GetSerializableValue());
                }

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    YamlSerializer.Serialize(stream, settingsFile);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while saving settings file [{0}]: {1}", e, filePath, e.FormatForReport());
                return false;
            }
            finally
            {
                profile.Saving = false;
            }
            return true;
        }

        /// <summary>
        /// Gets the settings key that matches the given name.
        /// </summary>
        /// <param name="name">The name of the settings property to fetch.</param>
        /// <returns>The settings key that matches the given name, or <c>null</c>.</returns>
        public static SettingsKey GetSettingsKey(UFile name)
        {
            SettingsKey key;
            SettingsKeys.TryGetValue(name, out key);
            return key;
        }

        /// <summary>
        /// Clears the current settings, including registered <see cref="SettingsKey"/> and <see cref="SettingsProfile"/> instances. This method should be used only for tests.
        /// </summary>
        public static void ClearSettings()
        {
            CurrentProfile = DefaultProfile;
            CurrentProfile.ValidateSettingsChanges();
            ProfileList.Clear();
            DefaultProfile.Settings.Clear();
            SettingsKeys.Clear();
        }
        
        internal static void RegisterSettingsKey(UFile name, object defaultValue, SettingsKey settingsKey)
        {
            SettingsKeys.Add(name, settingsKey);
            var entry = SettingsEntry.CreateFromValue(DefaultProfile, name, defaultValue);
            DefaultProfile.RegisterEntry(entry);
            // Ensure that the value is converted to the key type in each loaded profile.
            foreach (var profile in Profiles.Where(x => x != DefaultProfile))
            {
                if (profile.Settings.TryGetValue(name, out entry))
                {
                    var convertedValue = settingsKey.ConvertValue(entry.Value);
                    entry = SettingsEntry.CreateFromValue(profile, name, convertedValue);
                    profile.Settings[name] = entry;
                }
            }
        }

        private static void ChangeCurrentProfile(SettingsProfile oldProfile, SettingsProfile newProfile)
        {
            if (oldProfile == null) throw new ArgumentNullException("oldProfile");
            if (newProfile == null) throw new ArgumentNullException("newProfile");
            currentProfile = newProfile;

            foreach (var key in SettingsKeys)
            {
                object oldValue;
                oldProfile.GetValue(key.Key, out oldValue, true, false);
                object newValue;
                newProfile.GetValue(key.Key, out newValue, true, false);
                var oldList = oldValue as IList;
                var newList = newValue as IList;

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
                else
                {
                    isDifferent = !Equals(oldValue, newValue);
                }
                if (isDifferent)
                {
                    newProfile.NotifyEntryChanged(key.Key);
                }
            }

            // Changes have been notified, empty the list of modified settings.
            newProfile.ValidateSettingsChanges();
        }
    }
}