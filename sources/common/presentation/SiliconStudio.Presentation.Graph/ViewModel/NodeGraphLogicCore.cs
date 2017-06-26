// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using GraphX;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// Logics core object which contains all algorithms and logic settings
    /// </summary>
    public class NodeGraphLogicCore : GXLogicCore<NodeVertex, NodeEdge, BidirectionalGraph<NodeVertex, NodeEdge>> { }
}
