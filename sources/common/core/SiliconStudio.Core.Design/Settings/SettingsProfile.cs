// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// This class represents a collection of values for all registered <see cref="SettingsKey"/>. It may also contains values for settings keys that
    /// are not currently registered, if they exist in the file from which the profile was loaded.
    /// </summary>
    public class SettingsProfile : IDisposable
    {
        internal ActionStack.ActionStack ActionStack = new ActionStack.ActionStack(-1);
        internal bool Saving;
        private readonly SortedList<UFile, SettingsEntry> settings = new SortedList<UFile, SettingsEntry>();
        private readonly HashSet<UFile> modifiedSettings = new HashSet<UFile>();
        private readonly SettingsProfile parentProfile;
        private FileSystemWatcher fileWatcher;
        private UFile filePath;
        private bool monitorFileModification;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsProfile"/> class.
        /// </summary>
        /// <param name="group">The <see cref="SettingsGroup"/> containing this profile.</param>
        /// <param name="parentProfile">The parent profile.</param>
        internal SettingsProfile(SettingsGroup group, SettingsProfile parentProfile)
        {
            Group = group;
            this.parentProfile = parentProfile;
        }

        /// <summary>
        /// Gets the <see cref="SettingsGroup"/> containing this profile.
        /// </summary>
        public SettingsGroup Group { get; private set; }
        
        /// <summary>
        /// Gets the path of the file in which this profile has been saved.
        /// </summary>
        public UFile FilePath { get { return filePath; } internal set { Utils.SetAndInvokeIfChanged(ref filePath, value, UpdateMonitoring); } }

        /// <summary>
        /// Gets or sets whether to monitor external modification of the file in which this profile is stored. If <c>true</c>, The <see cref="FileModified"/> event might be raised.
        /// </summary>
        public bool MonitorFileModification { get { return monitorFileModification; } set { Utils.SetAndInvokeIfChanged(ref monitorFileModification, value, UpdateMonitoring); } }

        /// <summary>
        /// Raised when the file corresponding to this profile is modified on the disk, and <see cref="MonitorFileModification"/> is <c>true</c>.
        /// </summary>
        public event EventHandler<FileModifiedEventArgs> FileModified;
        
        /// <summary>
        /// Gets the collection of <see cref="SettingsEntry"/> currently existing in this <see cref="SettingsProfile"/>.
        /// </summary>
        internal IDictionary<UFile, SettingsEntry> Settings { get { return settings; } }

        internal bool IsDiscarding { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (fileWatcher != null)
            {
                fileWatcher.Changed -= SettingsFileChanged;
                fileWatcher.Dispose();
            }
        }

        public void ValidateSettingsChanges()
        {
            var keys = Group.GetAllSettingsKeys();
            foreach (var key in keys)
            {
                if (modifiedSettings.Contains(key.Name))
                    key.NotifyChangesValidated(this);
            }
            ActionStack.Clear();
            modifiedSettings.Clear();
        }

        public void DiscardSettingsChanges()
        {
            IsDiscarding = true;
            while (ActionStack.CanUndo)
            {
                ActionStack.Undo();
            }
            ActionStack.Clear();
            modifiedSettings.Clear();
            IsDiscarding = false;
        }
        
        /// <summary>
        /// Registers an entry that has not been registered before.
        /// </summary>
        /// <param name="entry">The entry to register.</param>
        internal void RegisterEntry(SettingsEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            Settings.Add(entry.Name, entry);
        }

        /// <summary>
        /// Gets the settings value that matches the given name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SettingsEntry"/> to fetch.</param>
        /// <param name="value">The resulting value if the name is found, <c>null</c> otherwise.</param>
        /// <param name="searchInParent">Indicates whether to search in the parent profile, if the name is not found in this profile.</param>
        /// <param name="createInCurrentProfile">If true, the list will be created in the current profile, from the value of its parent profile.</param>
        /// <returns><c>true</c> if an entry matching the name is found, <c>false</c> otherwise.</returns>
        internal bool GetValue(UFile name, out object value, bool searchInParent, bool createInCurrentProfile)
        {
            if (name == null) throw new ArgumentNullException("name");
            SettingsEntry entry = GetEntry(name, searchInParent, createInCurrentProfile);
            if (entry != null)
            {
                value = entry.Value;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Set the value of the entry that match the given name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <param name="value">The value to set.</param>
        internal void SetValue(UFile name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");

            SettingsEntry entry;
            if (!Settings.TryGetValue(name, out entry))
            {
                entry = SettingsEntry.CreateFromValue(this, name, value);
                Settings[name] = entry;
            }
            else
            {
                Settings[name].Value = value;
            }
        }

        /// <summary>
        /// Notifies that the entry with the given name has changed.
        /// </summary>
        /// <param name="name">The name of the entry that has changed.</param>
        internal void NotifyEntryChanged(UFile name)
        {
            modifiedSettings.Add(name);
        }

        /// <summary>
        /// Gets the <see cref="SettingsEntry"/> that matches the given name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SettingsEntry"/> to fetch.</param>
        /// <param name="searchInParent">Indicates whether to search in the parent profile, if the name is not found in this profile.</param>
        /// <param name="createInCurrentProfile"></param>
        /// <returns>An instance of <see cref="SettingsEntry"/> that matches the name, or <c>null</c>.</returns>
        private SettingsEntry GetEntry(UFile name, bool searchInParent, bool createInCurrentProfile)
        {
            if (name == null) throw new ArgumentNullException("name");

            SettingsEntry entry;
            if (Settings.TryGetValue(name, out entry))
                return entry;

            if (createInCurrentProfile)
            {
                entry = parentProfile.GetEntry(name, true, false);
                entry = SettingsEntry.CreateFromValue(this, name, entry.Value);
                RegisterEntry(entry);
                return entry;
            }

            return parentProfile != null && searchInParent ? parentProfile.GetEntry(name, true, false) : null;
        }
        
        private void UpdateMonitoring()
        {
            if (fileWatcher != null)
            {
                fileWatcher.Changed -= SettingsFileChanged;
                fileWatcher.Dispose();
            }
            if (MonitorFileModification && FilePath != null && File.Exists(FilePath))
            {
                fileWatcher = new FileSystemWatcher(Path.Combine(Environment.CurrentDirectory, FilePath.GetFullDirectory()), FilePath.GetFileNameWithExtension());
                fileWatcher.Changed += SettingsFileChanged;
                fileWatcher.EnableRaisingEvents = true;
            }
        }

        private void SettingsFileChanged(object sender, FileSystemEventArgs e)
        {
            if (Saving)
                return;

            var handler = FileModified;
            if (handler != null)
            {
                var args = new FileModifiedEventArgs(this);
                handler(null, args);
                if (args.ReloadFile)
                {
                    Group.ReloadSettingsProfile(this);
                }
            }
        }
    }
}
