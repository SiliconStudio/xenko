// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class NodePresenterException : Exception
    {
        public NodePresenterException([NotNull] string message) : base(message)
        {
        }

        public NodePresenterException([NotNull] string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
