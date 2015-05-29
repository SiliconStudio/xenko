using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    // Used internally by EffectParameterCollectionGroup
    class EffectParameterUpdaterDefinition : ParameterUpdaterDefinition
    {
        internal ParameterDependencyIndex[] Dependencies;
        internal ParameterCollection.InternalValue[][] ThreadLocalDynamicValues;

        internal EffectParameterUpdaterDefinition(IEnumerable<ParameterKey> keys, IEnumerable<ParameterDependency> dependencies)
        {
            SortedKeys = keys.OrderBy(x => x.HashCode).ToArray();
            SortedKeyHashes = SortedKeys.Select(x => x.HashCode).ToArray();

            // Sort dependencies
            dependencies = BuildDependencies(dependencies).ToArray();

            // Build dependencies (with indices instead of keys)
            var dependenciesIndex = new List<ParameterDependencyIndex>();
            foreach (var dependency in dependencies)
            {
                var destinationIndex = Array.IndexOf(SortedKeys, dependency.Destination);
                if (destinationIndex == -1)
                    throw new InvalidOperationException();
                var sourceIndices = dependency.Sources.Select(x => Array.IndexOf(SortedKeys, x)).ToArray();
                if (sourceIndices.Any(x => x == -1))
                    throw new InvalidOperationException();
                dependenciesIndex.Add(new ParameterDependencyIndex
                {
                    Destination = destinationIndex,
                    Sources = sourceIndices,
                    Dynamic = dependency.Dynamic,
                    //Parameters = new ParameterCollection.InternalValueReference[dependency.Sources.Length],
                });
            }

            this.Dependencies = dependenciesIndex.ToArray();

            ThreadLocalDynamicValues = new ParameterCollection.InternalValue[GraphicsDevice.ThreadCount][];
            for (int i = 0; i < ThreadLocalDynamicValues.Length; ++i)
            {
                ThreadLocalDynamicValues[i] = Dependencies.Select(x => ParameterCollection.CreateInternalValue(SortedKeys[x.Destination])).ToArray();
            }
        }

        /// <summary>
        /// Builds list of dynamic dependencies.
        /// </summary>
        private static void VisitDependencies(Dictionary<ParameterDependency, List<ParameterDependency>> graph, HashSet<ParameterDependency> processedVertices, List<ParameterDependency> result, ParameterDependency vertex)
        {
            if (!processedVertices.Contains(vertex))
            {
                processedVertices.Add(vertex);
                List<ParameterDependency> outEdges;
                if (graph.TryGetValue(vertex, out outEdges))
                {
                    foreach (var outVertex in outEdges)
                    {
                        VisitDependencies(graph, processedVertices, result, outVertex);
                    }
                }

                // Complexity is not so good, but should be sufficient for small n.
                if (!result.Contains(vertex))
                    result.Add(vertex);
            }
        }

        /// <summary>
        /// Builds dependency graph and generates update ordering.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private static IEnumerable<ParameterDependency> BuildDependencies(IEnumerable<ParameterDependency> dependencies)
        {
            var processedVertices = new HashSet<ParameterDependency>();
            var result = new List<ParameterDependency>();
            var keyToDependencies = new Dictionary<ParameterKey, ParameterDependency>();
            foreach (var dependency in dependencies)
            {
                keyToDependencies[dependency.Destination] = dependency;
            }

            var graph = new Dictionary<ParameterDependency, List<ParameterDependency>>();
            foreach (var dependency in dependencies)
            {
                foreach (var sourceKey in dependency.Sources)
                {
                    ParameterDependency source;
                    if (!keyToDependencies.TryGetValue(sourceKey, out source))
                        continue;

                    List<ParameterDependency> outEdges;
                    if (!graph.TryGetValue(dependency, out outEdges))
                    {
                        graph[dependency] = outEdges = new List<ParameterDependency>();
                    }
                    outEdges.Add(source);
                }
            }

            foreach (var dependency in dependencies)
            {
                VisitDependencies(graph, processedVertices, result, dependency);
            }

            return result;
        }

        internal struct ParameterDependencyIndex
        {
            public int Destination;
            public int[] Sources;
            public ParameterDynamicValue Dynamic;
            //public ParameterCollection.InternalValueReference[] Parameters;
        }
    }
}