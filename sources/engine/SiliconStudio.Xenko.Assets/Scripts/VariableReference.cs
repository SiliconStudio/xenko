using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    [DataStyle(DataStyle.Compact)]
    [NonIdentifiable]
    public sealed class VariableReference : IAssetPartReference
    {
        /// <summary>
        /// Gets or sets the identifier of the asset part represented by this reference.
        /// </summary>
        public Guid Id { get; set; }

        [DataMemberIgnore]
        public Type InstanceType { get; set; }

        public void FillFromPart(object assetPart)
        {
            var variable = (Variable)assetPart;
            Id = IdentifiableHelper.GetId(variable);
        }

        public object GenerateProxyPart(Type partType)
        {
            var variable = new Variable();
            IdentifiableHelper.SetId(variable, Id);
            return variable;
        }
    }
}