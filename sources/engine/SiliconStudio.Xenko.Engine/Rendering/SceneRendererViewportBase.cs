// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A <see cref="SceneRendererBase"/> that supports a <see cref="Viewport"/>.
    /// </summary>
    [DataContract]
    public abstract class SceneRendererViewportBase : SceneRendererBase, ISceneRendererViewport
    {
        protected SceneRendererViewportBase()
        {
            Viewport = new RectangleF(0, 0, 100f, 100f);
            IsViewportInPercentage = true;
        }

        /// <summary>
        /// Gets or sets the viewport in percentage or pixel.
        /// </summary>
        /// <value>The viewport in percentage or pixel.</value>
        /// <userdoc>Specify the region of the output target in which to draw. 
        /// Those values are either in pixels or relative depending on status of 'Viewport in %'.</userdoc>
        [DataMember(110)]
        public RectangleF Viewport { get; set; }

        /// <summary>
        /// Gets the actual viewport size (only valid after <see cre=f"ActivateOutput"/>).
        /// </summary>
        [DataMemberIgnore]
        public Viewport ComputedViewport { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the viewport is in fixed pixels instead of percentage.
        /// </summary>
        /// <value><c>true</c> if the viewport is in pixels instead of percentage; otherwise, <c>false</c>.</value>
        /// <userdoc>When this value is true, the Viewport size is a percentage (0-100) calculated relatively to the size of the Output, else it is a fixed size in pixels.</userdoc>
        [DataMember(120)]
        [DefaultValue(true)]
        [Display("Viewport in %")]
        public bool IsViewportInPercentage { get; set; }

        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>
        /// The aspect ratio.
        /// </value>
        /// <userdoc>The aspect ratio used if Add Letterbox/Pillarbox is checked.</userdoc>
        [DataMember(130)]
        [DefaultValue(CameraComponent.DefaultAspectRatio)]
        public float FixedAspectRatio { get; set; } = CameraComponent.DefaultAspectRatio;

        /// <summary>
        /// Gets or sets a value wether to edit the Viewport to force the aspect ratio and add letterboxes or pillarboxes where needed
        /// </summary>
        /// <userdoc>If checked and the viewport will be modified to fit the aspect ratio of Default Back Buffer Width and Default Back Buffer Height and letterboxes/pillarboxes might be added.</userdoc>
        [DataMember(140)]
        public bool ForceAspectRatio { get; set; }

        public override void Collect(RenderContext context)
        {
            base.Collect(context);

            var rect = Viewport;

            var output = GetOutput(context);

            // Setup the viewport
            if (!ForceAspectRatio)
            {

                if (IsViewportInPercentage)
                {
                    var width = output.Width;
                    var height = output.Height;
                    ComputedViewport = new Viewport((int)(rect.X*width/100.0f), (int)(rect.Y*height/100.0f), (int)(rect.Width*width/100.0f), (int)(rect.Height*height/100.0f));
                }
                else
                {
                    ComputedViewport = new Viewport((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                }
            }
            else
            {
                var currentAr = output.Width / (float)output.Height;
                var requiredAr = FixedAspectRatio;

                var arDiff = currentAr - requiredAr;

                // Pillarbox 
                if (arDiff > 0.0f)
                {
                    var newWidth = (float)Math.Max(1.0f, Math.Round(output.Height * requiredAr));
                    var adjX = (float)Math.Round(0.5f * (output.Width - newWidth));
                    ComputedViewport = new Viewport((int)adjX, 0, (int)newWidth, output.Height);
                }
                // Letterbox
                else
                {
                    var newHeight = (float)Math.Max(1.0f, Math.Round(output.Width / requiredAr));
                    var adjY = (float)Math.Round(0.5f * (output.Height - newHeight));
                    ComputedViewport = new Viewport(0, (int)adjY, output.Width, (int)newHeight);
                }
            }
        }

        protected override void ActivateOutputCore(RenderDrawContext context, RenderFrame output, bool disableDepth)
        {
            base.ActivateOutputCore(context, output, disableDepth);
            context.CommandList.SetViewport(ComputedViewport);
        }
    }
}
