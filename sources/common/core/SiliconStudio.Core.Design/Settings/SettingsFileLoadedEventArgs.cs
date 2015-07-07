// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// Arguments of the <see cref="SettingsGroup.SettingsFileLoaded"/> event.
    /// </summary>
    public class SettingsFileLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFileLoadedEventArgs"/> class.
        /// </summary>
        /// <param name="path"></param>
        public SettingsFileLoadedEventArgs(UFile path)
        {
            FilePath = path;
        }

        /// <summary>
        /// Gets the path of the file that has been loaded.
        /// </summary>
        public UFile FilePath { get; private set; }
    }
}