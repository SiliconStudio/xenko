// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>Fake rasterizer state (Description should be valid).</summary>
    public class FakeRasterizerState : RasterizerState
    {
        public FakeRasterizerState()
        {
        }

        public FakeRasterizerState(RasterizerStateDescription description) : base(description)
        {
        }
    }

    public class RasterizerStateSerializer : SiliconStudio.Core.Serialization.Contents.ContentSerializerBase<RasterizerState>
    {
        public override void Serialize(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context, SerializationStream stream, ref RasterizerState rasterizerState)
        {
            if (context.Mode == ArchiveMode.Serialize)
            {
                var rasterizerStateDescription = rasterizerState.Description;
                stream.Serialize(ref rasterizerStateDescription, context.Mode);
            }
            else
            {
                var rasterizerStateDescription = RasterizerStateDescription.Default;
                stream.Serialize(ref rasterizerStateDescription, context.Mode);
                rasterizerState = new FakeRasterizerState(rasterizerStateDescription);
            }
        }

        public override object Construct(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context)
        {
            return null;
        }
    }
}
