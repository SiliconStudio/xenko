// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Windows.Input;

namespace SiliconStudio.Presentation.Drawing
{
    /// <summary>
    /// Interface of controller manipulators.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    public interface IManipulatorBase<in T>
        where T : InputEventArgs
    {
        /// <summary>
        /// Gets the view where the event was raised.
        /// </summary>
        /// <value>The view.</value>
        IDrawingView View { get; }

        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">The event data.</param>
        void Completed(T e);

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The event data.</param>
        void Delta(T e);

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The event data.</param>
        void Started(T e);
    }
}
