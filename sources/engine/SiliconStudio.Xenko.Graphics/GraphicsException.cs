// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public class GraphicsException : Exception
    {
        public GraphicsException()
        {
        }

        public GraphicsException(string message, GraphicsDeviceStatus status = GraphicsDeviceStatus.Normal)
            : base(message)
        {
            Status = status;
        }

        public GraphicsException(string message, Exception innerException, GraphicsDeviceStatus status = GraphicsDeviceStatus.Normal)
            : base(message, innerException)
        {
            Status = status;
        }

        public GraphicsDeviceStatus Status { get; private set; }
    }
}