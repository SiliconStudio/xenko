// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SerializerContext = SiliconStudio.Core.Yaml.Serialization.SerializerContext;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A serializer for the <see cref="AssetComposite"/> type.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    public class AssetCompositeSerializer : ObjectSerializer, IDataCustomVisitor
    {
        /// <summary>
        /// Context containing information about asset parts being serialized.
        /// </summary>
        private static readonly ThreadLocal<AssetCompositeVisitorContext> LocalContext = new ThreadLocal<AssetCompositeVisitorContext>();

        /// <inheritdoc/>
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return CanVisit(typeDescriptor.Type) ? this : null;
        }

        /// <inheritdoc/>
        public override void WriteYaml(ref ObjectContext objectContext)
        {
            var contextToken = PrepareLocalContext(objectContext.Descriptor.Type);
            try
            {
                base.WriteYaml(ref objectContext);
            }
            finally
            {
                CleanLocalContext(contextToken);
            }
        }

        /// <inheritdoc/>
        public override object ReadYaml(ref ObjectContext objectContext)
        {
            var contextToken = PrepareLocalContext(objectContext.Descriptor.Type);
            try
            {
                return base.ReadYaml(ref objectContext);
            }
            finally
            {
                CleanLocalContext(contextToken);
            }
        }

        /// <inheritdoc/>
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            if (LocalContext.Value.SerializeAsReference)
            {
                var attribute = LocalContext.Value.EnteredTypes.Peek();
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
            if (LocalContext.Value.SerializeAsReference)
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

        public bool CanVisit(Type type)
        {
            // Accepts any type inheriting from AssetComposite
            if (typeof(AssetComposite).IsAssignableFrom(type))
            {
                return true;
            }
            // Accepts any part of an AssetComposite, they might be serialized out of a parent asset.
            if (LocalContext.Value == null && AssetRegistry.IsAssetPartType(type))
            {
                return true;
            }
            // Accepts any type known as asset part type for the current asset type.
            if (LocalContext.Value != null && LocalContext.Value.References.Any(x => x.ReferenceableType.IsAssignableFrom(type)))
            {
                return true;
            }
            return false;
        }

        public void Visit(ref VisitorContext context)
        {
            var contextToken = PrepareLocalContext(context.Descriptor.Type);
            try
            {
                context.Visitor.VisitObject(context.Instance, context.Descriptor, !LocalContext.Value.SerializeAsReference);
            }
            finally
            {
                CleanLocalContext(contextToken);
            }
        }

        private struct LocalContextToken
        {
            public Type Type;
            public bool RemoveLastEnteredType;
            public bool ClearLocalContext;
            public AssetCompositeVisitorContext OldContext;
        }

        private static LocalContextToken PrepareLocalContext(Type type)
        {
            var token = new LocalContextToken
            {
                Type = type,
                OldContext = LocalContext.Value,
                ClearLocalContext = false
            };

            if (typeof(AssetComposite).IsAssignableFrom(token.Type))
            {
                // Entering the asset root node, create the local context.
                LocalContext.Value = new AssetCompositeVisitorContext(token.Type);
                token.ClearLocalContext = true;
            }
            else if (LocalContext.Value == null && AssetRegistry.IsAssetPartType(token.Type))
            {
                var attributes = AssetRegistry.GetPartReferenceAttributes(token.Type);
                LocalContext.Value = new AssetCompositeVisitorContext(attributes);
                token.ClearLocalContext = true;
            }

            token.RemoveLastEnteredType = LocalContext.Value?.EnterNode(token.Type) ?? false;
            return token;
        }

        private static void CleanLocalContext(LocalContextToken token)
        {
            LocalContext.Value?.LeaveNode(token.Type, token.RemoveLastEnteredType);

            if (token.ClearLocalContext)
            {
                // Exiting the asset root node, clear the local context.
                LocalContext.Value = token.OldContext;
            }
        }
    }
}
