// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    struct ParameterDependencyIndex
    {
        public int Destination;
        public int[] Sources;
        public ParameterDynamicValue Dynamic;
        //public ParameterCollection.InternalValueReference[] Parameters;
    }

    struct ParameterDependency
    {
        public ParameterKey Destination;
        public ParameterKey[] Sources;
        public ParameterDynamicValue Dynamic;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder
                .Append("(")
                .Append(Destination.Name)
                .Append(")");

            foreach (var source in Sources)
            {
                builder
                    .Append(" ")
                    .Append(source.Name);
            }

            return builder.ToString();
        }
    }

    struct UpdaterKey
    {
        public ParameterKey Key;
        public bool IsDynamic;
    }

    class ShaderParameterUpdaterDefinition : ParameterUpdaterDefinition
    {
        internal ParameterDependencyIndex[] Dependencies;
        internal ParameterCollection.InternalValue[][] ThreadLocalDynamicValues;

        public ShaderParameterUpdaterDefinition(IEnumerable<ParameterKey> keys, IEnumerable<ParameterDependency> dependencies)
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
    }

    struct FastListStruct<T>
    {
        public int Count;
        public T[] Items;

        public FastListStruct(FastList<T> fastList)
        {
            Count = fastList.Count;
            Items = fastList.Items;
        }

        public FastListStruct(T[] array)
        {
            Count = array.Length;
            Items = array;
        }

        public static implicit operator FastListStruct<T>(FastList<T> fastList)
        {
            return new FastListStruct<T>(fastList);
        }

        public static implicit operator FastListStruct<T>(T[] array)
        {
            return new FastListStruct<T>(array);
        }
    }

    /// <summary>
    /// Updates ParameterCollection for rendering, including dynamic parameters.
    /// </summary>
    internal class ShaderParameterUpdater : ParameterUpdater
    {
        internal int InternalValueBinarySearch(int[] values, int keyHash)
        {
            int start = 0;
            int end = values.Length - 1;
            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var hash1 = values[middle];
                var hash2 = keyHash;

                if (hash1 == hash2)
                {
                    return middle;
                }
                if (hash1 < hash2)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }
            return ~start;
        }

        public void Update(GraphicsDevice graphicsDevice, ShaderParameterUpdaterDefinition definition, ParameterCollection[] parameterCollections, int levelCount)
        {
            Update(definition, parameterCollections, levelCount);

            // ----------------------------------------------------------------
            // Update dynamic values
            // Definitions based on Default+Pass lists
            // ----------------------------------------------------------------
            var dependencies = definition.Dependencies;
            for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; ++dependencyIndex)
            {
                var dependency = dependencies[dependencyIndex];
                int highestLevel = 0;
                int destinationLevel = InternalValues[dependency.Destination].Key;

                var destination = InternalValues[dependency.Destination];
                bool needUpdate = false;
                for (int i = 0; i < dependency.Sources.Length; ++i)
                {
                    var source = InternalValues[dependency.Sources[i]];
                    var sourceLevel = source.Key;

                    if (highestLevel < sourceLevel)
                        highestLevel = sourceLevel;
                }

                if (destinationLevel < highestLevel)
                {
                    // Dynamic value: the sources of this dynamic value are defined in a most derived collection than its destination collection
                    // as a result, we need to create or use a new dynamic value at the appropriate level.

                    // Find last collection index (excluding parameterOverrides)
                    int maxLevelNoOverride = levelCount - 1;
                    if (highestLevel <= maxLevelNoOverride)
                    {
                        // TODO: Choose target level more properly (i.e. mesh*pass => meshpass)
                        // Sources all comes from normal collections => override in the most derived collection
                        // For now, use maxLevelNoOverride instead of highestLevel just to be safe.
                        var bestCollectionForDynamicValue = parameterCollections[maxLevelNoOverride];
                        var key = definition.SortedKeys[dependency.Destination];
                        bestCollectionForDynamicValue.SetDefault(key, true);
                        InternalValues[dependency.Destination] = destination = new KeyValuePair<int, ParameterCollection.InternalValue>(highestLevel, bestCollectionForDynamicValue.GetInternalValue(key));
                    }
                    else
                    {
                        // At least one source comes from TLS override, so use dynamic from TLS dynamic storage as well.
                        InternalValues[dependency.Destination] = destination = new KeyValuePair<int, ParameterCollection.InternalValue>(parameterCollections.Length, definition.ThreadLocalDynamicValues[graphicsDevice.ThreadIndex][dependencyIndex]);
                    }

                    needUpdate = true;
                }

                // Force updating (even if no parameters has been updated)
                if (!dependency.Dynamic.AutoCheckDependencies)
                    needUpdate = true;

                if (destination.Value.Dependencies == null || destination.Value.Dependencies.Length < dependency.Sources.Length)
                    destination.Value.Dependencies = new ParameterCollection.InternalValueReference[dependency.Sources.Length];

                var dependencyParameters = destination.Value.Dependencies;
                for (int i = 0; i < dependency.Sources.Length; ++i)
                {
                    var source = InternalValues[dependency.Sources[i]];
                    var internalValue = source.Value;

                    if (dependencyParameters[i].Entry != internalValue)
                    {
                        needUpdate = true;
                        dependencyParameters[i].Entry = internalValue;
                        dependencyParameters[i].Counter = internalValue.Counter;
                    }
                    else if (dependencyParameters[i].Counter != internalValue.Counter)
                    {
                        needUpdate = true;
                        dependencyParameters[i].Counter = internalValue.Counter;
                    }
                }

                if (needUpdate)
                {
                    // At least one source was updated
                    dependency.Dynamic.GetValue(destination.Value);
                    destination.Value.Counter++;
                }
            }
        }

        internal ParameterCollection.InternalValue GetInternalValue(int index)
        {
            return InternalValues[index].Value;
        }

        public T GetValue<T>(int index)
        {
            return ((ParameterCollection.InternalValueBase<T>)InternalValues[index].Value).Value;
        }
    }
}