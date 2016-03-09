// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Drawing
{
    public interface IDrawingView
    {
        /// <summary>
        /// Gets the model.
        /// </summary>
        IDrawingModel Model { get; }
        
        /// <summary>
        /// Invalidates the drawing (not blocking the UI thread)
        /// </summary>
        void InvalidateDrawing();
    }
}
