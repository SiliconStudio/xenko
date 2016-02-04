// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A wireframe rendering mode (rendering only ModelComponent for now).
    /// </summary>
    [DataContract("CameraRendererModeWireFrame")]
    [Display("WireFrame")]
    public sealed class CameraRendererModeWireFrame : CameraRendererMode
    {
        private const string WireFrameEffect = "XenkoWireFrameShadingEffect";

        // TODO GRAPHICS REFACTOR
        //private readonly ModelComponentAndPickingRenderer modelComponentAndPickingRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraRendererModeWireFrame"/> class.
        /// </summary>
        public CameraRendererModeWireFrame()
        {
            ModelEffect = WireFrameEffect;

            // Render only CameraComponent and ModelComponent
            RenderComponentTypes.Add(typeof(CameraComponent));
            RenderComponentTypes.Add(typeof(ModelComponent));

            // TODO GRAPHICS REFACTOR
            //modelComponentAndPickingRenderer = new ModelComponentAndPickingRenderer();
            //RendererOverrides.Add(typeof(ModelComponent), modelComponentAndPickingRenderer);

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
        /// Gets or sets the color of the front.
        /// </summary>
        /// <value>The color of the front.</value>
        /// <userdoc>The color used to render front faces</userdoc>
        [DataMember(110)]
        public Color3 FrontColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable back color computed from the normal backfacing the camera.
        /// </summary>
        /// <value><c>true</c> if to enable back color computed from the normal backfacing the camera; otherwise, <c>false</c>.</value>
        /// <userdoc>If checked, use the color specified by 'Back Color' to render the back faces. Otherwise, uses the same color as for front faces.</userdoc>
        [DataMember(115)]
        [DefaultValue(false)]
        public bool EnableBackColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the back.
        /// </summary>
        /// <value>The color of the back.</value>
        /// <userdoc>The color used to render front faces if 'Enable Back Color' is checked.</userdoc>
        [DataMember(120)]
        public Color3 BackColor { get; set; }

        /// <summary>
        /// Gets or sets the alpha blend.
        /// </summary>
        /// <value>The blend.</value>
        /// <userdoc>Specifies the opacity of the wireframe.</userdoc>
        [DataMember(130)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01f, 0.1f, 2)]
        public float AlphaBlend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is separating color and alpha blending. Default is false (Color Blend is using Alpha Blend)
        /// </summary>
        /// <value><c>true</c> if this instance is separate color and alpha blending; otherwise, <c>false</c>.</value>
        /// <userdoc>If checked, blend the provided wireframe color with the default color of the model. Otherwise, use only the provided wireframe color.</userdoc>
        [DataMember(140)]
        [DefaultValue(false)]
        public bool EnableColorBlend { get; set; }

        /// <summary>
        /// Gets or sets the color blend.
        /// </summary>
        /// <value>The blend.</value>
        /// <userdoc>The blend factor between provided wireframe color and the default model color. A factor of 0 represents the default model color. A factor of 1 results in the provided wireframe color.</userdoc>
        [DataMember(150)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01f, 0.1f, 2)]
        public float ColorBlend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show backface. Default is true.
        /// </summary>
        /// <value><c>true</c> if show backface (default is true); otherwise, <c>false</c>.</value>
        /// <userdoc>If checked, both the front and back faces are rendered. Otherwise, only front faces are rendered.</userdoc>
        [DataMember(160)]
        [DefaultValue(true)]
        public bool ShowBackface { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable depth].
        /// </summary>
        /// <value><c>true</c> if [enable depth]; otherwise, <c>false</c>.</value>
        /// <userdoc>If checked, read and writes into the depth buffer when rendering the wireframe, otherwise not.</userdoc>
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

        // TODO GRAPHICS REFACTOR
        //[DataMemberIgnore]
        //public ModelComponentRenderer ModelRenderer
        //{
        //    get
        //    {
        //        return modelComponentAndPickingRenderer.ModelRenderer;
        //    }
        //}

        /// <summary>
        /// Gets the default <see cref="RasterizerState" /> for models drawn by this render mode.
        /// </summary>
        /// <param name="isGeomertryInverted"><c>true</c> if the rendered gometry is inverted through scaling, <c>false</c> otherwise.</param>
        /// <returns>The rasterizer state.</returns>
        public override RasterizerStateDescription GetDefaultRasterizerState(bool isGeomertryInverted)
        {
            if (EnableBackColor || ShowBackface)
                return Context.GraphicsDevice.RasterizerStates.WireFrame;

            return isGeomertryInverted ? Context.GraphicsDevice.RasterizerStates.WireFrameCullFront : Context.GraphicsDevice.RasterizerStates.WireFrameCullBack;
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var sceneCameraRenderer = context.RenderContext.Tags.Get(SceneCameraRenderer.Current);

            var graphicsDevice = context.GraphicsDevice;
            try
            {
                // TODO GRAPHICS REFACTOR
                //graphicsDevice.PushState();

                // If we have a scene camera renderer use it to disable depth
                if (sceneCameraRenderer != null)
                {
                    sceneCameraRenderer.ActivateOutput(context, !EnableDepth);
                }

                // TODO GRAPHICS REFACTOR
                // Setup the backface paramters
                //if (context.Parameters.Get(MaterialFrontBackBlendShaderKeys.UseNormalBackFace) != EnableBackColor)
                //{
                //    context.Parameters.Set(MaterialFrontBackBlendShaderKeys.UseNormalBackFace, EnableBackColor);
                //}
                //
                //context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorFront, FrontColor.ToColorSpace(graphicsDevice.ColorSpace));
                //context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorBack, (EnableBackColor ? BackColor : FrontColor).ToColorSpace(graphicsDevice.ColorSpace));
                //context.Parameters.Set(MaterialFrontBackBlendShaderKeys.AlphaBlend, AlphaBlend * BlendFactor);
                //context.Parameters.Set(MaterialFrontBackBlendShaderKeys.ColorBlend, (EnableColorBlend ? ColorBlend : AlphaBlend) * BlendFactor);

                // TODO GRAPHICS REFACTOR
                //graphicsDevice.SetBlendState(graphicsDevice.BlendStates.AlphaBlend);
                //graphicsDevice.SetRasterizerState(EnableBackColor || ShowBackface ? graphicsDevice.RasterizerStates.WireFrame : graphicsDevice.RasterizerStates.WireFrameCullBack);
                //graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.Default);

                // TODO GRAPHICS REFACTOR
                //modelComponentAndPickingRenderer.ModelRenderer.ForceRasterizer = true;

                base.DrawCore(context);
            }
            finally 
            {
                // TODO GRAPHICS REFACTOR
                //graphicsDevice.PopState();
            }

        }
    }
}