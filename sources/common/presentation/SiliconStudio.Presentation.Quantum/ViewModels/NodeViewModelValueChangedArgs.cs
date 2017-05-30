// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    public class NodeViewModelValueChangedArgs : EventArgs
    {
        public NodeViewModelValueChangedArgs(GraphViewModel viewModel, NodeViewModel node)
        {
            ViewModel = viewModel;
            Node = node;
        }

        public GraphViewModel ViewModel { get; private set; }

        public NodeViewModel Node { get; private set; }
    }
}
