// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A delegate renderer.
    /// </summary>
    [DataContract("SceneDelegateRenderer")]
    [Browsable(false)] // This type is not browsable from the editor
    public class SceneDelegateRenderer : SceneRendererBase
    {
        private readonly Action<RenderContext, RenderFrame> drawAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneDelegateRenderer"/> class.
        /// </summary>
        /// <param name="drawAction">The draw action.</param>
        /// <exception cref="System.ArgumentNullException">drawAction</exception>
        public SceneDelegateRenderer(Action<RenderContext, RenderFrame> drawAction)
        {
            if (drawAction == null) throw new ArgumentNullException("drawAction");
            this.drawAction = drawAction;
        }

        protected override void DrawCore(RenderContext context)
        {
            var output = Output.GetSafeRenderFrame(context);
            if (output != null)
            {
                drawAction(context, output);
            }
        }
    }
}