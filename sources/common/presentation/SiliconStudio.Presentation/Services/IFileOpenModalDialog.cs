// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface representing a modal file open dialog.
    /// </summary>
    public interface IFileOpenModalDialog : IFileModalDialog
    {
        /// <summary>
        /// Gets or sets whether multi-selection is allowed.
        /// </summary>
        bool AllowMultiSelection { get; set; }

        /// <summary>
        /// Gets the list of file paths selected by the user.
        /// </summary>
        IReadOnlyCollection<string> FilePaths { get; }
    }
}
