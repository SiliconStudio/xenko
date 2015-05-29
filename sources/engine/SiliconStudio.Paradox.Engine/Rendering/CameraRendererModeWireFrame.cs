// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A wireframe rendering mode (rendering only ModelComponent for now).
    /// </summary>
    [DataContract("CameraRendererModeWireFrame")]
    [Display("WireFrame")]
    public sealed class CameraRendererModeWireFrame : CameraRendererMode
    {
        private const string WireFrameEffect = "ParadoxWireFrameShadingEffect";

        private readonly ModelComponentAndPickingRenderer modelComponentAndPickingRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraRendererModeWireFrame"/> class.
        /// </summary>
        public CameraRendererModeWireFrame()
        {
            ModelEffect = WireFrameEffect;

            // Render only CameraComponent and ModelComponent
            RenderComponentTypes.Add(typeof(CameraComponent));
            RenderComponentTypes.Add(typeof(ModelComponent));

            modelComponentAndPickingRenderer = new ModelComponentAndPickingRenderer();
            RendererOverrides.Add(typeof(ModelComponent), modelComponentAndPickingRenderer);

            FrontColor = new Color3(0, 1.0f, 0.0f);
            BackColor = new Color3(0, 0.5f, 0.0f);

            AlphaBlend = 1.0f;
            ColorBlend = 1.0f;

            BlendFactor = 1.0f;

            ShowBackface = true;
        }

        /// <inheritdoc/>
        [DefaultValue(WireFrameEffect)]
        public override string ModelEffect { get; set; }

        /// <summary>
        /// Gets or sets the material filter used to render this scene camera.
        /// </summary>
        /// <value>The material filter.</value>
        [DataMember(110)]
        public Color3 FrontColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable back color computed from the normal backfacing the camera.
        /// </summary>
        /// <value><c>true</c> if to enable back color computed from the normal backfacing the camera; otherwise, <c>false</c>.</value>
        [DataMember(115)]
        [DefaultValue(false)]
        public bool EnableBackColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the back.
        /// </summary>
        /// <value>The color of the back.</value>
        [DataMember(120)]
        public Color3 BackColor { get; set; }

        /// <summary>
        /// Gets or sets the alpha blend.
        /// </summary>
        /// <value>The blend.</value>
        [DataMember(130)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01f, 0.1f, 2)]
        public float AlphaBlend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is separating color and alpha blending. Default is false (Color Blend is using Alpha Blend)
        /// </summary>
        /// <value><c>true</c> if this instance is separate color and alpha blending; otherwise, <c>false</c>.</value>
        [DataMember(140)]
        [DefaultValue(false)]
        public bool EnableColorBlend { get; set; }

        /// <summary>
        /// Gets or sets the color blend.
        /// </summary>
        /// <value>The blend.</value>
        [DataMember(150)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01f, 0.1f, 2)]
        public float ColorBlend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show backface. Default is true.
        /// </summary>
        /// <value><c>true</c> if show backface (default is true); otherwise, <c>false</c>.</value>
        [DataMember(160)]
        [DefaultValue(true)]
        public bool ShowBackface { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable depth].
        /// </summary>
        /// <value><c>true</c> if [enable depth]; otherwise, <c>false</c>.</value>
        [DataMember(170)]
        [DefaultValue(false)]
        public bool EnableDepth { get; set; }

        /// <summary>
        /// Gets or sets the blend factor that will be multiplied to <see cref="AlphaBlend"/> and <see cref="ColorBlend"/>
        /// </summary>
        /// <value>The blend factor.</value>
        [DataMemberIgnore]
        [DefaultValue(1.0f)]
        public float BlendFactor { get; set; }

        [DataMemberIgnore]
        public ModelComponentRenderer ModelRenderer
        {
            get
            {
                return modelComponentAndPickingRenderer.ModelRenderer;
            }
        }

        protected override void DrawCore(RenderContext context)
        {
            var sceneCameraRenderer = context.Tags.Get(SceneCameraRenderer.Current);

            var graphicsDevice = context.GraphicsDevice;
            try
            {
                graphicsDevice.PushState();

                // If we have a scene camera renderer use it to disable depth
                if (sceneCameraRenderer != null)
                {
                    sceneCameraRenderer.ActivateOutput(context, !EnableDepth);
                }

                // Setup the backface paramters
                if (context.Parameters.Get(MaterialFrontBackBlendShaderKeys.UseNormalBackFace) != EnableBackColor)
                {
                    context.Parameters.Set(MaterialFrontBackBlendShaderKeys.UseNormalBackFace, EnableBackColor);
                }

                context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorFront, FrontColor);
                context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorBack, EnableBackColor ? BackColor : FrontColor);
                context.Parameters.Set(MaterialFrontBackBlendShaderKeys.AlphaBlend, AlphaBlend * BlendFactor);
                context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorBlend, (EnableColorBlend ? ColorBlend : AlphaBlend) * BlendFactor);

                graphicsDevice.SetBlendState(graphicsDevice.BlendStates.AlphaBlend);
                graphicsDevice.SetRasterizerState(EnableBackColor || ShowBackface ? graphicsDevice.RasterizerStates.WireFrame : graphicsDevice.RasterizerStates.WireFrameCullBack);
                graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.None);

                base.DrawCore(context);
            }
            finally 
            {
                graphicsDevice.PopState();
            }

        }
    }
}