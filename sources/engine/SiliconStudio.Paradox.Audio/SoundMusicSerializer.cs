// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Audio
{
    internal class SoundMusicSerializer : ContentSerializerBase<SoundMusic>
    {
        private  readonly AudioEngine audioEngine;

        public SoundMusicSerializer(AudioEngine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            audioEngine = engine;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref SoundMusic obj)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                obj = SoundMusic.Load(audioEngine, stream.NativeStream);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return null;
        }
    }
}
