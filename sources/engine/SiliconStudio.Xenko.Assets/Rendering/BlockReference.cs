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
            var block = (Block)Activator.CreateInstance(partType);
            IdentifiableHelper.SetId(block, Id);
            return block;
        }
    }
}