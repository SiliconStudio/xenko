// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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