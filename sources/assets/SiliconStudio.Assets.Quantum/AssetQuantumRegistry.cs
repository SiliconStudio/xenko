using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public static class AssetQuantumRegistry
    {
        private static readonly Type[] AssetPropertyNodeGraphConstructorSignature = new Type[] { typeof(INodeContainer), typeof(AssetItem) };
        private static readonly Dictionary<Type, Type> NodeGraphTypes = new Dictionary<Type, Type>();

        public static void RegisterAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(AssetPropertyNodeGraph).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<AssetPropertyNodeGraphAttribute>();
                    if (type.GetConstructor(AssetPropertyNodeGraphConstructorSignature) == null)
                        throw new InvalidOperationException($"The type {type.Name} does not have a public constructor matching the expected signature: ({string.Join(", ", (IEnumerable<Type>)AssetPropertyNodeGraphConstructorSignature)})");

                    if (NodeGraphTypes.ContainsKey(attribute.AssetType))
                        throw new ArgumentException($"The type {attribute.AssetType.Name} already has an associated property node graph type.");

                    NodeGraphTypes.Add(attribute.AssetType, type);
                }
            }
        }

        public static AssetPropertyNodeGraph ConstructPropertyGraph(INodeContainer nodeContainer, AssetItem assetItem)
        {
            var assetType = assetItem.Asset.GetType();
            while (assetType != null)
            {
                Type propertyGraphType;
                if (NodeGraphTypes.TryGetValue(assetType, out propertyGraphType))
                {
                    return (AssetPropertyNodeGraph)Activator.CreateInstance(propertyGraphType, nodeContainer, assetItem);
                }
                assetType = assetType.BaseType;
            }
            throw new InvalidOperationException("No AssetPropertyNodeGraph type matching the given asset type has been found");
        }
    }
}
