// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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