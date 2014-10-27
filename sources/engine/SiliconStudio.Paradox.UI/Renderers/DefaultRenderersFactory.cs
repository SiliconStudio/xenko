// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Renderers
{
    /// <summary>
    /// A factory that create the default renderer for each <see cref="UIElement"/> type.
    /// </summary>
    internal class DefaultRenderersFactory : IElementRendererFactory
    {
        private readonly ElementRenderer defaultRenderer;

        private readonly Dictionary<Type, ElementRenderer> typeToRenderers = new Dictionary<Type, ElementRenderer>();
        
        public DefaultRenderersFactory(IServiceRegistry services)
        {
            defaultRenderer = new ElementRenderer(services);
            typeToRenderers[typeof(ImageElement)] = new DefaultImageRenderer(services);
            typeToRenderers[typeof(Button)] = new DefaultButtonRenderer(services);
            typeToRenderers[typeof(ImageButton)] = new ElementRenderer(services);
            typeToRenderers[typeof(ToggleButton)] = new ElementRenderer(services);
            typeToRenderers[typeof(TextBlock)] = new DefaultTextBlockRenderer(services);
            typeToRenderers[typeof(ScrollingText)] = new DefaultScrollingTextRenderer(services);
            typeToRenderers[typeof(ModalElement)] = new DefaultModalElementRenderer(services);
            typeToRenderers[typeof(ScrollBar)] = new DefaultScrollBarRenderer(services);
            typeToRenderers[typeof(EditText)] = new DefaultEditTextRenderer(services);
            typeToRenderers[typeof(ContentDecorator)] = new DefaultContentDecoratorRenderer(services);
            typeToRenderers[typeof(Border)] = new DefaultBorderRenderer(services);
            typeToRenderers[typeof(ToggleButton)] = new DefaultToggleButtonRenderer(services);
            typeToRenderers[typeof(Slider)] = new DefaultSliderRenderer(services);
        }

        public ElementRenderer TryCreateRenderer(UIElement element)
        {
            // try to get the renderer from the registered default renderer
            var currentType = element.GetType();
            while (currentType != null)
            {
                if (typeToRenderers.ContainsKey(currentType))
                    return typeToRenderers[currentType];

                currentType = currentType.GetTypeInfo().BaseType;
            }

            return defaultRenderer;
        }
    }
}