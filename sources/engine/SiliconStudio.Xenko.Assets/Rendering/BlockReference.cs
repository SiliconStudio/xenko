using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Assets.Rendering
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [NonIdentifiable]
    public sealed class BlockReference : IAssetPartReference
    {
        /// <summary>
        /// Gets or sets the identifier of the asset part represented by this reference.
        /// </summary>
        public Guid Id { get; set; }

        [DataMemberIgnore]
        public Type InstanceType { get; set; }

        public void FillFromPart(object assetPart)
        {
            var block = (Block)assetPart;
            Id = IdentifiableHelper.GetId(block);
        }

        public object GenerateProxyPart(Type partType)
        {
            var block = FakeBlock.Create();
            IdentifiableHelper.SetId(block, Id);
            return block;
        }

        /// <summary>
        /// Used temporarily during deserialization when creating references.
        /// </summary>
        /// <remarks>
        /// We don't expose a public ctor so that is not listed in list of available blocks to create.
        /// </remarks>
        class FakeBlock : Block
        {
            private FakeBlock()
            {
            }

            internal static FakeBlock Create()
            {
                return new FakeBlock();
            }

            public override void RegenerateSlots()
            {
            }
        }
    }
}