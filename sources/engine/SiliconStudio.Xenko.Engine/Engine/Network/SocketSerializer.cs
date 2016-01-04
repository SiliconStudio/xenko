// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME && !SILICONSTUDIO_RUNTIME_CORECLR
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