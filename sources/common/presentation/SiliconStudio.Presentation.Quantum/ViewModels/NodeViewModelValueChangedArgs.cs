// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.ViewModels
{
    public class NodeViewModelValueChangedArgs : EventArgs
    {
        public NodeViewModelValueChangedArgs(GraphViewModel viewModel, string nodePath)
        {
            ViewModel = viewModel;
            NodePath = nodePath;
        }

        public GraphViewModel ViewModel { get; private set; }

        public string NodePath { get; private set; }
    }
}
