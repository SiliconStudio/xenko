// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Quantum
{
    public static class AssetQuantumRegistry
    {
        private static readonly Type[] AssetPropertyNodeGraphConstructorSignature = { typeof(AssetPropertyGraphContainer), typeof(AssetItem), typeof(ILogger) };
        private static readonly Dictionary<Type, Type> NodeGraphTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, AssetPropertyGraphDefinition> NodeGraphDefinitions = new Dictionary<Type, AssetPropertyGraphDefinition>();

        public static void RegisterAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(AssetPropertyGraph).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<AssetPropertyGraphAttribute>();
                    if (attribute == null)
                        continue;

                    if (type.GetConstructor(AssetPropertyNodeGraphConstructorSignature) == null)
                        throw new InvalidOperationException($"The type {type.Name} does not have a public constructor matching the expected signature: ({string.Join(", ", (IEnumerable<Type>)AssetPropertyNodeGraphConstructorSignature)})");

                    if (NodeGraphTypes.ContainsKey(attribute.AssetType))
                        throw new ArgumentException($"The type {attribute.AssetType.Name} already has an associated property node graph type.");

                    NodeGraphTypes.Add(attribute.AssetType, type);
                }

                if (typeof(AssetPropertyGraphDefinition).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<AssetPropertyGraphDefinitionAttribute>();
                    if (attribute == null)
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        throw new InvalidOperationException($"The type {type.Name} does not have a public parameterless constructor.)");

                    if (NodeGraphDefinitions.ContainsKey(attribute.AssetType))
                        throw new ArgumentException($"The type {attribute.AssetType.Name} already has an associated property node graph type.");

                    var definition = (AssetPropertyGraphDefinition)Activator.CreateInstance(type);
                    NodeGraphDefinitions.Add(attribute.AssetType, definition);
                }
            }
        }

        public static AssetPropertyGraph ConstructPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
        {
            var assetType = assetItem.Asset.GetType();
            while (assetType != null)
            {
                Type propertyGraphType;
                var typeToTest = assetType.IsGenericType ? assetType.GetGenericTypeDefinition() : assetType;
                if (NodeGraphTypes.TryGetValue(typeToTest, out propertyGraphType))
                {
                    return (AssetPropertyGraph)Activator.CreateInstance(propertyGraphType, container, assetItem, logger);
                }
                assetType = assetType.BaseType;
            }
            throw new InvalidOperationException("No AssetPropertyGraph type matching the given asset type has been found");
        }

        public static AssetPropertyGraphDefinition GetDefinition(Type assetType)
        {
            if (!typeof(Asset).IsAssignableFrom(assetType))
                throw new ArgumentException($"The type {assetType.Name} is not an asset type");

            while (assetType != typeof(Asset))
            {
                AssetPropertyGraphDefinition definition;
                // ReSharper disable once AssignNullToNotNullAttribute - cannot happen
                if (NodeGraphDefinitions.TryGetValue(assetType, out definition))
                    return definition;

                assetType = assetType.BaseType;
            }

            return NodeGraphDefinitions[typeof(Asset)];
        }
    }
}
