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
            // Similar to Array.Sort (which is not available in Windows Store/Phone)
            QuickSort(indices, meshes.Items, 0, meshes.Count - 1);
        }

        private void QuickSort(int[] indices, RenderMesh[] meshes, int left, int right)
        {
            if (left > right)
                return;

            int i = left;
            int j = right;
            int pivot = indices[(left + right) / 2];

            while (i <= j)
            {
                while (indices[i] < pivot)
                    ++i;
                while (indices[j] > pivot)
                    --j;
                if (i <= j)
                {
                    RenderMesh meshTmp = meshes[i];
                    meshes[i] = meshes[j];
                    meshes[j] = meshTmp;

                    int indexTmp = indices[i];
                    indices[i++] = indices[j];
                    indices[j--] = indexTmp;
                }
            }

            // Recurse
            if (left < j)
            {
                QuickSort(indices, meshes, left, j);
            }
            if (i < right)
            {
                QuickSort(indices, meshes, i, right);
            }
        }

        /// <summary>
        /// Generates the sort key for each mesh. Meshes will be sorted in ascending order using this key.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        /// <returns></returns>
        public abstract int GenerateSortKey(RenderMesh renderMesh);
    }
}