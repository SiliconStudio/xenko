// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A delegate renderer.
    /// </summary>
    [DataContract("SceneDelegateRenderer")]
    [Display(Browsable = false)] // This type is not browsable from the editor
    public class SceneDelegateRenderer : SceneRendererViewportBase
    {
        private readonly Action<RenderDrawContext, RenderFrame> drawAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneDelegateRenderer"/> class.
        /// </summary>
        /// <param name="drawAction">The draw action.</param>
        /// <exception cref="System.ArgumentNullException">drawAction</exception>
        public SceneDelegateRenderer(Action<RenderDrawContext, RenderFrame> drawAction)
        {
            if (drawAction == null) throw new ArgumentNullException("drawAction");
            this.drawAction = drawAction;
        }

        protected override void DrawCore(RenderDrawContext context, RenderFrame output)
        {
            drawAction(context, output);
        }
    }
}