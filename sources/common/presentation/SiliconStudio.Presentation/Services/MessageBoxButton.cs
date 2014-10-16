// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.Services
{
    // TODO: make these enum independent from their System.Windows equivalent
    /// <summary>
    /// An enum representing the buttons to display in a message box.
    /// </summary>
    public enum MessageBoxButton
    {
        /// <summary>
        /// Display a single OK button.
        /// </summary>
        OK = 0,
        /// <summary>
        /// Display a OK button and a Cancel button.
        /// </summary>
        OKCancel = 1,
        /// <summary>
        /// Display a Yes button, a No button and a Cancel button.
        /// </summary>
        YesNoCancel = 3,
        /// <summary>
        /// Display a Yes button and a No button.
        /// </summary>
        YesNo = 4,
    }
}