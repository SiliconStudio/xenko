// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Engine.Network
{
    /// <summary>
    /// Used when there is a socket exception.
    /// </summary>
    public class SimpleSocketException : Exception
    {
        public SimpleSocketException(string message) : base(message)
        {
        }
    }
}
