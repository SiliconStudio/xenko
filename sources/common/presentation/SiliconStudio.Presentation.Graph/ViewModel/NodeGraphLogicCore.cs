// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using GraphX;
using GraphX.GraphSharp;
using GraphX.Logic;
using QuickGraph;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// Logics core object which contains all algorithms and logic settings
    /// </summary>
    public class NodeGraphLogicCore : GXLogicCore<NodeVertex, NodeEdge, BidirectionalGraph<NodeVertex, NodeEdge>> { }
}
