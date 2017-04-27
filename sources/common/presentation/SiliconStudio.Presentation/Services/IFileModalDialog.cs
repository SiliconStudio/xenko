// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface representing modal file dialogs.
    /// </summary>
    public interface IFileModalDialog : IModalDialog
    {
        /// <summary>
        /// Gets or sets the list of filter to use in the file dialog.
        /// </summary>
        IList<FileDialogFilter> Filters { get; set; }

        /// <summary>
        /// Gets or sets the initial directory of the file dialog.
        /// </summary>
        string InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets the default file name to display when opening the file dialog.
        /// </summary>
        string DefaultFileName { get; set; }
    }
}
