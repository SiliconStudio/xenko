// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Graphics
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
