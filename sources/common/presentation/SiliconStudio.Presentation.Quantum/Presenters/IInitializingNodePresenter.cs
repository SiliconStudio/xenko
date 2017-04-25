// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public interface IInitializingNodePresenter : INodePresenter
    {
        void AddChild(IInitializingNodePresenter child);

        void FinalizeInitialization();
    }
}
