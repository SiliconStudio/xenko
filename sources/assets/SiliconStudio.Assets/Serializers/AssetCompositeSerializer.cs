// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A serializer for the <see cref="AssetComposite"/> type.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    public class AssetCompositeSerializer : IDataCustomVisitor
    {
        // Exposed temporarily for the use of AssetCompositePartReferenceCollector
        // TODO: Unify IDataCustomVisitor and AssetVisitorBase?
        /// <summary>
        /// Context containing information about asset parts being serialized.
        /// </summary>
        internal static readonly ThreadLocal<AssetCompositeVisitorContext> LocalContext = new ThreadLocal<AssetCompositeVisitorContext>();

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
            public bool RemoveLastEnteredNode;
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

            token.RemoveLastEnteredNode = LocalContext.Value?.EnterNode(token.Type) ?? false;
            return token;
        }

        private static void CleanLocalContext(LocalContextToken token)
        {
            LocalContext.Value?.LeaveNode(token.RemoveLastEnteredNode);

            if (token.ClearLocalContext)
            {
                // Exiting the asset root node, clear the local context.
                LocalContext.Value = token.OldContext;
            }
        }
    }
}
