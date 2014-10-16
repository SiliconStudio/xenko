// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface representing a modal folder selection dialog.
    /// </summary>
    public interface IFolderOpenModalDialog : IModalDialog
    {
        /// <summary>
        /// Gets or sets the initial directory of the folder dialog.
        /// </summary>
        string InitialDirectory { get; set; }

        /// <summary>
        /// Gets the directory selected by the user.
        /// </summary>
        string Directory { get; }
    }
}
