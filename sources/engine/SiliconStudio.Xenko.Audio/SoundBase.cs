// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for all the sounds and sound instances.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SoundBase>), Profile = "Content")]
    [DataSerializer(typeof(SoundBaseSerializer))]
    public abstract partial class SoundBase : ComponentBase
    {
        internal string CompressedDataUrl { get; set; }

        internal int SampleRate { get; set; } = 44100;

        internal int SamplesPerFrame { get; set; } = 1024;

        internal int Channels { get; set; } = 2;

        internal bool StreamFromDisk { get; set; }

        internal bool Spatialized { get; set; }

        internal int NumberOfPackets { get; set; }

        internal int MaxPacketLength { get; set; }

        [DataMemberIgnore]
        internal Stream CompressedDataStream;

        [DataMemberIgnore]
        internal UnmanagedArray<short> PreloadedData;

        /// <summary>
        /// Create an empty instance of soundBase. Used for serialization.
        /// </summary>
        internal SoundBase()
        {
        }

        [DataMemberIgnore]
        internal AudioEngineState EngineState => AudioEngine.State;

        #region Disposing Utilities
        
        internal void CheckNotDisposed()
        {
            if(IsDisposed)
                throw new ObjectDisposedException("this");
        }

        #endregion
    }
}
