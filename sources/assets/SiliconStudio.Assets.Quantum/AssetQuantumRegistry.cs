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

        public static void RegisterAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(AssetPropertyGraph).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<AssetPropertyGraphAttribute>();
                    if (type.GetConstructor(AssetPropertyNodeGraphConstructorSignature) == null)
                        throw new InvalidOperationException($"The type {type.Name} does not have a public constructor matching the expected signature: ({string.Join(", ", (IEnumerable<Type>)AssetPropertyNodeGraphConstructorSignature)})");

                    if (NodeGraphTypes.ContainsKey(attribute.AssetType))
                        throw new ArgumentException($"The type {attribute.AssetType.Name} already has an associated property node graph type.");

                    NodeGraphTypes.Add(attribute.AssetType, type);
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
    }
}
