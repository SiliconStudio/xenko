// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>Fake sampler state (Description should be valid).</summary>
    public class FakeSamplerState : SamplerState
    {
        public FakeSamplerState()
        {
        }

        public FakeSamplerState(SamplerStateDescription description) : base(description)
        {
        }
    }

    public class SamplerStateSerializer : SiliconStudio.Core.Serialization.Contents.ContentSerializerBase<SamplerState>
    {
        public override void Serialize(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context, SerializationStream stream, ref SamplerState samplerState)
        {
            if (context.Mode == ArchiveMode.Serialize)
            {
                var samplerStateDescription = samplerState.Description;
                stream.Serialize(ref samplerStateDescription, context.Mode);
            }
            else
            {
                var samplerStateDescription = SamplerStateDescription.Default;
                stream.Serialize(ref samplerStateDescription, context.Mode);
                samplerState = new FakeSamplerState(samplerStateDescription);
            }
        }

        public override object Construct(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context)
        {
            return null;
        }
    }
}