// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// The arguments associated to an key event.
    /// </summary>
    internal class TextEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The character that was entered
        /// </summary>
        public char Character { get; internal set; }
    }
}