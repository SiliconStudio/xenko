// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public static class EntityHierarchyDataExtensions
    {
        /// <summary>
        /// Helper method to dump this hierarchy to a text output
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="writer"></param>
        /// <returns><c>true</c> if the dump was sucessful, <c>false</c> otherwise</returns>
        public static bool DumpTo(this AssetCompositeHierarchyData<EntityDesign, Entity> hierarchy, TextWriter writer)
        {
            bool result = true;
            writer.WriteLine("***************");
            writer.WriteLine($"RootEntities [{hierarchy.RootPartIds.Count}]");
            writer.WriteLine("===============");
            foreach (var id in hierarchy.RootPartIds)
            {
                if (!hierarchy.Parts.ContainsKey(id))
                {
                    result = false;
                }
                writer.WriteLine(hierarchy.Parts.ContainsKey(id) ? $"{id} => {hierarchy.Parts[id].Entity}" : $"{id} => ERROR - Entity not found in [Entities]");
            }

            writer.WriteLine("***************");
            writer.WriteLine($"Entities [{hierarchy.Parts.Count}]");
            writer.WriteLine("===============");
            for (int i = 0; i < hierarchy.Parts.Count; i++)
            {
                var entityEntry = hierarchy.Parts[i];

                writer.Write($"{entityEntry.Entity.Id} => {entityEntry.Entity}");

                if (entityEntry.BaseId != null)
                {
                    writer.Write($" Base: {entityEntry.BaseId}");
                }

                if (entityEntry.BasePartInstanceId != null)
                {
                    writer.Write($" BasePartInstanceId: {entityEntry.BasePartInstanceId}");
                }
                writer.WriteLine();

                foreach (var child in entityEntry.Entity.Transform.Children)
                {

                    writer.Write($"  - {child.Entity.Id} => {child.Entity.Name}");
                    if (!hierarchy.Parts.ContainsKey(child.Entity.Id))
                    {
                        writer.Write(" <= ERROR, Entity not found in [Entities]");
                        result = false;

                    }
                    writer.WriteLine();
                }
            }
            return result;
        }
    }
}
