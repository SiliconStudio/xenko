// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Presentation.Services
{
    // TODO: make these enum independent from their System.Windows equivalent
    /// <summary>
    /// An enum representing the button pressed by the user to close a message box.
    /// </summary>
    public enum MessageBoxResult
    {
        /// <summary>
        /// The message box has not been closed yet.
        /// </summary>
        None = 0,
        /// <summary>
        /// The user pressed the OK button.
        /// </summary>
        OK = 1,
        /// <summary>
        /// The user pressed the Cancel button.
        /// </summary>
        Cancel = 2,
        /// <summary>
        /// The user pressed the Yes button.
        /// </summary>
        Yes = 6,
        /// <summary>
        /// The user pressed the No button.
        /// </summary>
        No = 7,
    }
}
