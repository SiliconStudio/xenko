// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Effects.Layers
{

    public interface IGraphicsLayerInput
    {
        
    }

    [DataContract("GraphicsLayerInputNone")]
    [Display("None")]
    public class GraphicsLayerInputNone : IGraphicsLayerInput
    {
    }


    public interface IGraphicsLayerOutput
    {
        
    }

    public interface IGraphicsRenderingMode
    {
    }

    [DataContract("GraphicsForwardRenderingMode")]
    [Display("Forward")]
    public class GraphicsForwardRenderingMode : IGraphicsRenderingMode
    {
    }

    public interface IGraphicsRenderer
    {
        
    }

    [DataContract("GraphicsRendererCollection")]
    public class GraphicsRendererCollection : List<IGraphicsRenderer>
    {
    }

    [DataContract("GraphicsLayer")]
    public sealed class GraphicsLayer
    {
        public GraphicsLayer()
        {
            Mode = new GraphicsForwardRenderingMode();
            Input = new GraphicsLayerInputNone();
            Renderers = new GraphicsRendererCollection();
        }

        [DataMember(0)]
        [NotNull]
        public IGraphicsRenderingMode Mode { get; set; }

        [DataMember(10)]
        [NotNull]
        public IGraphicsLayerInput Input { get; set; }

        [DataMember(20)]
        [NotNull]
        public IGraphicsLayerOutput Output { get; set; }

        [DataMember(30)]
        [Category]
        public GraphicsRendererCollection Renderers { get; private set; }
    }

    [DataContract("GraphicsLayerCollection")]
    public class GraphicsLayerCollection : List<GraphicsLayer>
    {
    }

    [DataContract("GraphicsLayerComposer")]
    public class GraphicsLayerComposer
    {
        public GraphicsLayerComposer()
        {
            Layers = new GraphicsRendererCollection();
            Master = new GraphicsLayer();
        }

        [DataMember(10)]
        [Category]
        public GraphicsRendererCollection Layers { get; private set; }

        [DataMember(20)]
        [Category]
        public GraphicsLayer Master { get; private set; }
    }
}