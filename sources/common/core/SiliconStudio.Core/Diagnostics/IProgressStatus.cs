// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Provides progress of an operation.
    /// </summary>
    public interface IProgressStatus
    {
        // TODO: Current design is poor as it does not support recursive progress

        /// <summary>
        /// An event handler to notify the progress of an operation.
        /// </summary>
        event EventHandler<ProgressStatusEventArgs> ProgressChanged;

        /// <summary>
        /// Handles the <see cref="E:ProgressChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ProgressStatusEventArgs"/> instance containing the event data.</param>
        void OnProgressChanged(ProgressStatusEventArgs e);
    }
}