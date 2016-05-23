// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// The arguments associated to an key event.
    /// </summary>
    internal class KeyEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The key that triggered the event.
        /// </summary>
        public Keys Key { get; internal set; }

        /// <summary>
        /// A reference to the input system that can be used to check the status of the other keys.
        /// </summary>
        public InputManager Input { get; internal set; }
    }
}