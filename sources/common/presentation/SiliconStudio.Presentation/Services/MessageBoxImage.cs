// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Presentation.Services
{
    // TODO: make these enum independent from their System.Windows equivalent
    /// <summary>
    /// An enum representing the image to display in a message box.
    /// </summary>
    public enum MessageBoxImage
    {
        /// <summary>
        /// No image will be displayed in the message box.
        /// </summary>
        None = 0,
        /// <summary>
        /// An image representing an error will be displayed in the message box.
        /// </summary>
        Error = 16,
        /// <summary>
        /// An image representing a question will be displayed in the message box.
        /// </summary>
        Question = 32,
        /// <summary>
        /// An image representing a warning will be displayed in the message box.
        /// </summary>
        Warning = 48,
        /// <summary>
        /// An image representing an information will be displayed in the message box.
        /// </summary>
        Information = 64,
    }
}
