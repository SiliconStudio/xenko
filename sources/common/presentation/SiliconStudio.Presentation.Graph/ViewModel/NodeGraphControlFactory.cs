// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows;
using GraphX;
using GraphX.Controls;
using GraphX.Controls.Models;
using SiliconStudio.Presentation.Graph.Controls;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// Factory class responsible for NodeVertexControl and NodeEdgeControl objects creation
    /// </summary>
    public class NodeGraphControlFactory : IGraphControlFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphArea"></param>
        public NodeGraphControlFactory(GraphAreaBase graphArea)
        {
            FactoryRootArea = graphArea;
        }

        /// <summary>
        /// Create vertex control.
        /// </summary>
        /// <param name="vertexData"></param>
        /// <returns></returns>
        public VertexControl CreateVertexControl(object vertexData)
        {
            return new NodeVertexControl(vertexData) { RootArea = FactoryRootArea };
        }

        /// <summary>
        /// Create edge control
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="edge"></param>
        /// <param name="showLabels"></param>
        /// <param name="showArrows"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true, Visibility visibility = Visibility.Visible)
        {
            return new NodeEdgeControl(source, target, edge, showLabels, showArrows) { Visibility = visibility, RootArea = FactoryRootArea };
        }

        /// <summary>
        /// Root graph area for the factory
        /// </summary>
        public GraphAreaBase FactoryRootArea { get; set; }
    }
}
