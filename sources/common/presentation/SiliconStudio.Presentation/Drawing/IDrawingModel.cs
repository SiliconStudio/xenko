// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Presentation.Drawing
{
    public interface IDrawingModel
    {
        /// <summary>
        /// Attaches this item with the specified drawing view.
        /// </summary>
        /// <param name="view"></param>
        void Attach(IDrawingView view);

        /// <summary> 
        /// Detaches this item with the specified drawing view.
        /// </summary>
        /// <param name="view"></param>
        void Detach(IDrawingView view);

        /// <summary>
        /// Renders the canvas with the specified renderer.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        /// <param name="width">The available width.</param>
        /// <param name="height">The available height.</param>
        void Render(IDrawingContext context, double width, double height);

        /// <summary>
        /// Updates.
        /// </summary>
        /// <param name="updateData">If set to <c>true</c> all data collections will be updated.</param>
        void Update(bool updateData);
    }
}
