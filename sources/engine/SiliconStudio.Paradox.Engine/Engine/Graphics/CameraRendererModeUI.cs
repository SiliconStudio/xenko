// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Renderers;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A forward rendering mode.
    /// </summary>
    [DataContract("CameraRendererModeUI")]
    [Display("UI")]
    public sealed class CameraRendererModeUI : CameraRendererMode, IRendererManager
    {
        private readonly UIComponentRenderer uiComponentRenderer;

        private Vector3 virtualResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraRendererModeForward"/> class.
        /// </summary>
        public CameraRendererModeUI()
        {
            ClearDepthBuffer = true;
            RenderComponentTypes.Add(typeof(UIComponent));

            // create and add the ui component renderer override (we need a reference to the renderer)
            uiComponentRenderer = new UIComponentRenderer();
            RendererOverrides.Add(typeof(UIComponent), uiComponentRenderer);

            // Set a default value for the virtual resolution
            VirtualResolution = new Vector3(5);
        }

        /// <summary>
        /// Gets or sets the value indicating whether the depth buffer should be cleared before drawing.
        /// </summary>
        /// <userdoc>Indicate if the render should clear the current depth buffer before rendering.</userdoc>
        [DataMember(100)]
        [DefaultValue(true)]
        public bool ClearDepthBuffer { get; set; }

        /// <summary>
        /// Gets or sets the virtual resolution of the UI element.
        /// </summary>
        /// <userdoc>The virtual resolution of UI element in scene unit.</userdoc>
        [DataMember(10)]
        [Display("Virtual Resolution")]
        public Vector3 VirtualResolution
        {
            get { return virtualResolution; }
            set
            {
                if (virtualResolution == value)
                    return;

                virtualResolution = value;
                uiComponentRenderer.VirtualResolution = value;
            }
        }

        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);

            if(uiComponentRenderer.Services == null)
                uiComponentRenderer.Initialize(context);
        }

        protected override void Destroy()
        {
            base.Destroy();

            uiComponentRenderer.Dispose();
        }

        public ElementRenderer GetRenderer(UIElement element)
        {
            return ((IRendererManager)uiComponentRenderer).GetRenderer(element);
        }

        public void RegisterRendererFactory(Type uiElementType, IElementRendererFactory factory)
        {
            ((IRendererManager)uiComponentRenderer).RegisterRendererFactory(uiElementType, factory);
        }

        public void RegisterRenderer(UIElement element, ElementRenderer renderer)
        {
            ((IRendererManager)uiComponentRenderer).RegisterRenderer(element, renderer);
        }
    }
}