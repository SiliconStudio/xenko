// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;
using SerializerContext = SharpYaml.Serialization.SerializerContext;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A serializer for the <see cref="AssetComposite"/> type.
    /// </summary>
    public class AssetCompositeSerializer : ObjectSerializer
    {
        /// <summary>
        /// Context containing information about asset parts being serialized.
        /// </summary>
        private readonly ThreadLocal<AssetCompositeVisitorContext> localContext = new ThreadLocal<AssetCompositeVisitorContext>();

        /// <inheritdoc/>
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            // Accepts any type inheriting from AssetComposite
            if (typeof(AssetComposite).IsAssignableFrom(typeDescriptor.Type))
            {
                return this;
            }
            // Accepts any part of an AssetComposite, they might be serialized out of a parent asset.
            if (localContext.Value == null && AssetRegistry.IsAssetPartType(typeDescriptor.Type))
            {
                return this;
            }
            // Accepts any type known as asset part type for the current asset type.
            if (localContext.Value != null && localContext.Value.References.Any(x => x.ReferenceableType.IsAssignableFrom(typeDescriptor.Type)))
            {
                return this;
            }
            return null;
        }

        /// <inheritdoc/>
        public override void WriteYaml(ref ObjectContext objectContext)
        {
            var type = objectContext.Descriptor.Type;
            bool clearLocalContext = false;
            // Entering the asset root node, create the local context.
            if (typeof(AssetComposite).IsAssignableFrom(type))
            {
                localContext.Value = new AssetCompositeVisitorContext(type);
                clearLocalContext = true;
            }
            else if (localContext.Value == null && AssetRegistry.IsAssetPartType(type))
            {
                var attributes = AssetRegistry.GetPartReferenceAttributes(type);
                localContext.Value = new AssetCompositeVisitorContext(attributes);
                clearLocalContext = true;
            }

            var removeLastEnteredType = localContext.Value?.EnterNode(type) ?? false;

            try
            {
                base.WriteYaml(ref objectContext);
            }
            finally
            {
                localContext.Value?.LeaveNode(type, removeLastEnteredType);

                if (clearLocalContext)
                {
                    localContext.Value = null;
                }
            }
        }

        /// <inheritdoc/>
        public override object ReadYaml(ref ObjectContext objectContext)
        {
            var type = objectContext.Descriptor.Type;
            var oldContext = localContext.Value;
            bool clearLocalContext = false;

            if (typeof(AssetComposite).IsAssignableFrom(type))
            {
                // Entering the asset root node, create the local context.
                localContext.Value = new AssetCompositeVisitorContext(type);
                clearLocalContext = true;
            }
            else if (localContext.Value == null && AssetRegistry.IsAssetPartType(type))
            {
                var attributes = AssetRegistry.GetPartReferenceAttributes(type);
                localContext.Value = new AssetCompositeVisitorContext(attributes);
                clearLocalContext = true;
            }

            var removeLastEnteredType = localContext.Value?.EnterNode(type) ?? false;

            try
            {
                var result = base.ReadYaml(ref objectContext);

                if (typeof(AssetComposite).IsAssignableFrom(type))
                {
                    // Let's fixup part references after serialization
                    var assetComposite = (AssetComposite)objectContext.Instance;
                    assetComposite.FixupPartReferences();
                }

                return result;
            }
            finally
            {
                localContext.Value?.LeaveNode(type, removeLastEnteredType);

                if (clearLocalContext)
                {
                    // Exiting the asset root node, clear the local context.
                    localContext.Value = oldContext;
                }
            }
        }

        /// <inheritdoc/>
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            if (localContext.Value.SerializeAsReference)
            {
                var attribute = localContext.Value.EnteredTypes.Peek();
                var referenceType = attribute.ReferenceType;
                var reference = (IAssetPartReference)Activator.CreateInstance(referenceType);

                if (objectContext.SerializerContext.IsSerializing)
                {
                    // Serialization: properly fill the reference with information from the real object
                    reference.FillFromPart(objectContext.Instance);
                }
                else
                {
                    // Deserialization: store the real type of the asset part (this information won't be accessible later when we need it)
                    reference.InstanceType = objectContext.Descriptor.Type;
                }

                // Replace the real object with the reference.
                objectContext.Instance = reference;
            }

            base.CreateOrTransformObject(ref objectContext);

            // TODO: FIXME: Decouple the deserialization of the graph of objects and the patching of single objects (shouldn't be at the same place), and move this code in the Entity assembly (remove dynamic usage)
            // When deserializing, we don't keep the TransformComponent created when the Entity is created
            if (!objectContext.SerializerContext.IsSerializing && objectContext.Instance?.GetType().Name == "Entity")
            {
                dynamic entity = objectContext.Instance;
                entity.Components.Clear();
            }
        }

        /// <inheritdoc/>
        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            if (localContext.Value.SerializeAsReference)
            {
                if (!objectContext.SerializerContext.IsSerializing)
                {
                    // Deserialization: generate a proxy asset part from the reference. The real asset part will be resolved at the end.
                    var reference = objectContext.Instance as IAssetPartReference;
                    if (reference != null)
                    {
                        var proxyPart = reference.GenerateProxyPart(reference.InstanceType);
                        objectContext.Instance = proxyPart;
                        return;
                    }
                }
            }

            base.TransformObjectAfterRead(ref objectContext);
        }
    }
}
