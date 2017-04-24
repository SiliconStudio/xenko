// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if !SILICONSTUDIO_PLATFORM_UWP && !SILICONSTUDIO_RUNTIME_CORECLR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SiliconStudio.Xenko.Engine.Network
{
    // TODO: Switch to internal serialization engine
    public class SocketSerializer
    {
        public Stream Stream { get; set; }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        public void Serialize(object obj)
        {
            if (Stream == null)
                return;
            lock (this)
            {
                binaryFormatter.Serialize(Stream, obj);
            }
        }
        public object Deserialize()
        {
            return binaryFormatter.Deserialize(Stream);
        }
    }
}
#endif
