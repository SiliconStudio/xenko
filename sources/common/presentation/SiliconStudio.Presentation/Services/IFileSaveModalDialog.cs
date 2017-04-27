// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface representing a modal file save dialog.
    /// </summary>
    public interface IFileSaveModalDialog : IFileModalDialog
    {
        /// <summary>
        /// Gets the file path selected by the user.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets or sets the default extension to apply when the user type a file name without extension.
        /// </summary>
        string DefaultExtension { get; set; }
    }
}
