using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Sorts meshes using <see cref="GenerateSortKey"/>.
    /// </summary>
    public abstract class RenderMeshSorter
    {
        [ThreadStatic]
        private static int[] indicesTLS;

        /// <summary>
        /// Sorts the meshes using <see cref="GenerateSortKey"/>.
        /// </summary>
        /// <param name="meshes">The meshes.</param>
        public void Sort(FastList<RenderMesh> meshes)
        {
            // Allocate sort space (if necessary)
            var indices = indicesTLS;
            if (indices == null || indices.Length < meshes.Count)
                indicesTLS = indices = new int[meshes.Count];

            // Generate sort indices
            for (int index = 0; index < meshes.Count; index++)
                indices[index] = GenerateSortKey(meshes[index]);

            // Sort both indices and meshes at the same time
            // Note: Tried a custom quick sort, performance was little bit better but not by much; might be good to investigate other sort
            Array.Sort(indices, meshes.Items, 0, meshes.Count, Comparer<int>.Default);
        }

        /// <summary>
        /// Generates the sort key for each mesh. Meshes will be sorted in ascending order using this key.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        /// <returns></returns>
        public abstract int GenerateSortKey(RenderMesh renderMesh);
    }
}