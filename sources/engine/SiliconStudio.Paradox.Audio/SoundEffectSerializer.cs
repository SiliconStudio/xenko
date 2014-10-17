// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Audio
{
    internal class SoundEffectSerializer : ContentSerializerBase<SoundEffect>
    {
        private readonly AudioEngine audioEngine;

        public SoundEffectSerializer(AudioEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            audioEngine = engine;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref SoundEffect obj)
        {
            base.Serialize(context, stream, ref obj);

            if (context.Mode == ArchiveMode.Deserialize)
            {
                obj = SoundEffect.Load(audioEngine, stream.NativeStream);
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
