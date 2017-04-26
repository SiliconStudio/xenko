// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Quantum
{
    internal interface IGraphNodeInternal : IInitializingGraphNode
    {
        event EventHandler<INodeChangeEventArgs> PrepareChange;
        event EventHandler<INodeChangeEventArgs> FinalizeChange;
    }
}
