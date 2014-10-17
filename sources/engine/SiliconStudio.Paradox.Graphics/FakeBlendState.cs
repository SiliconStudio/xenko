// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>Fake blend state (Description should be valid).</summary>
    public class FakeBlendState : BlendState
    {
        public FakeBlendState()
        {
        }

        public FakeBlendState(BlendStateDescription description) : base(description)
        {
        }
    }

    public class BlendStateSerializer : SiliconStudio.Core.Serialization.Contents.ContentSerializerBase<BlendState>
    {
        public override void Serialize(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context, SerializationStream stream, ref BlendState blendState)
        {
            if (context.Mode == ArchiveMode.Serialize)
            {
                var blendStateDescription = blendState.Description;
                stream.Serialize(ref blendStateDescription, context.Mode);
            }
            else
            {
                var blendStateDescription = BlendStateDescription.Default;
                stream.Serialize(ref blendStateDescription, context.Mode);
                blendState = new FakeBlendState(blendStateDescription);
            }
        }

        public override object Construct(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context)
        {
            return null;
        }
    }
}
