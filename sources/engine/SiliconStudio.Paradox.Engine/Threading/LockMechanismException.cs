// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Paradox.Threading
{
    public enum LockMechanismStage
    {
        OnBegin,
        OnEnd,
    }

    public class LockMechanismException : Exception
    {
        public LockMechanismException(LockMechanismStage stage, ILockMechanism lockMechanism, Exception innerException)
            : base(string.Format("{0} ({1})", stage, lockMechanism.GetType().FullName), innerException)
        {
        }
    }
}
