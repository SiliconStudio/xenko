// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Paradox.Threading
{
    public interface ILockMechanism
    {
        object OnBegin(object syncRoot, Action action);
        void OnEnd(object syncRoot);
    }
}
