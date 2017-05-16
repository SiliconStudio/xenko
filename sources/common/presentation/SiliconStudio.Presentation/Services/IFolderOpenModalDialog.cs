// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
