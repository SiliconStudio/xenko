// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// Arguments of the <see cref="SettingsContainer.SettingsFileLoaded"/> event.
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
