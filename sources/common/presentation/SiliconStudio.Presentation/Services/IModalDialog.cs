// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface representing a modal dialog.
    /// </summary>
    public interface IModalDialog
    {
        /// <summary>
        /// Display the modal dialog. This method will block until the user close the dialog.
        /// </summary>
        /// <returns>A <see cref="DialogResult"/> value indicating how the user closed the dialog.</returns>
        Task<DialogResult> Show();

        /// <summary>
        /// Gets or sets a data context for the modal dialog.
        /// </summary>
        object DataContext { get; set; }
    }
}
