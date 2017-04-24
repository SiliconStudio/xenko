// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Xenko;
using Xenko.Diffs;
using Xenko.Effects;
using Xenko.Engine;
using Xenko.EntityModel;
using Xenko.Framework;
using Xenko.Framework.Graphics;
using Xenko.Framework.Mathematics;
using Xenko.Framework.MicroThreading;
using Xenko.Framework.PropertyModel;
using Xenko.Framework.Serialization;
using Xenko.Framework.Serialization.Contents;

using NGit;
using NGit.Api;
using NGit.Dircache;
using NGit.Merge;
using NGit.Revwalk;
using NGit.Revwalk.Filter;
using NGit.Storage.File;
using NGit.Treewalk;
using NGit.Treewalk.Filter;

using Sharpen;

using FileMode = System.IO.FileMode;

namespace ScriptTest
{
    // Various scrips to test scene saving/loading and git merging.
    [XenkoScript]
    public static class ScriptSync
    {
        private static MemoryStream memoryStream = new MemoryStream();
        private static Serializer Serializer;

        static ScriptSync()
        {
            Serializer = Serializer.Default;
        }

        public static List<EntityDefinition> LoadEntities(Stream stream)
        {
            List<EntityDefinition> entities = null;
            var rw = new BinarySerializationReader(stream);
            rw.Context.Serializer = Serializer;
            rw.SerializeClass(null, ref entities, ArchiveMode.Deserialize);
            return entities;
        }

        [XenkoScript]
        public static void SaveAssets(EngineContext engineContext)
        {
            var entities = new List<EntityDefinition>();

            foreach (var entity in engineContext.EntityManager.Entities.OrderBy(x => x.Guid).Where(x => x.Name == "him"))
            {
                var entityDefinition = new EntityDefinition(entity.Guid);
                entities.Add(entityDefinition);

                foreach (var entityComponent in entity.Properties.Where(x => x.Value is EntityComponent).OrderBy(x => x.Key.Name))
                {
                    var componentDefinition = new EntityComponentDefinition { Name = entityComponent.Key.Name, Properties = new List<EntityComponentProperty>() };
                    entityDefinition.Components.Add(componentDefinition);

                    var entityComponentValue = entityComponent.Value as EntityComponent;

                    foreach (var field in entityComponentValue.GetType().GetFields())
                    {
                        if (field.GetCustomAttributes(typeof(VersionableAttribute), true).Length == 0)
                            continue;

                        componentDefinition.Properties.Add(new EntityComponentProperty(EntityComponentPropertyType.Field, field.Name, Encode(field.GetValue(entityComponentValue))));
                    }

                    foreach (var property in entityComponentValue.GetType().GetProperties())
                    {
                        if (property.GetCustomAttributes(typeof(VersionableAttribute), true).Length == 0)
                            continue;

                        componentDefinition.Properties.Add(new EntityComponentProperty(EntityComponentPropertyType.Property, property.Name, Encode(property.GetValue(entityComponentValue, null))));
                    }

                    componentDefinition.Properties = componentDefinition.Properties.OrderBy(x => x.Name).ToList();
                }
            }

            var fileStream = new FileStream(@"C:\DEV\hotei_scene\scene.hotei", FileMode.Create, FileAccess.Write);
            var stream = new BinarySerializationWriter(fileStream);
            stream.Context.Serializer = Serializer;
            stream.SerializeClass(null, ref entities, ArchiveMode.Serialize);
            fileStream.Close();
        }
        
        class ParameterUrl
        {
            public string Url { get; set; }
        }

        [XenkoScript]
        public static void MergeTest(EngineContext engineContext)
        {
            // TODO: Currently hardcoded
            var db = new FileRepository(new FilePath(@"C:\DEV\hotei_scene", Constants.DOT_GIT));
            var git = new Git(db);
            var tree1Ref = db.GetRef("test");
            var tree2Ref = db.GetRef(Constants.HEAD);
            var tree1CommitId = tree1Ref.GetObjectId();
            var tree2CommitId = tree2Ref.GetObjectId();

            // Merge tree1 into current tree
            var mergeResult = git.Merge().Include(tree1CommitId).Call();

            if (mergeResult.GetMergeStatus() == MergeStatus.CONFLICTING)
            {
                foreach (var conflict in mergeResult.GetConflicts())
                {
                    if (conflict.Key.EndsWith(".hotei"))
                    {
                        // Search base tree (common ancestor), if any
                        var walk = new RevWalk(db);
                        walk.SetRevFilter(RevFilter.MERGE_BASE);
                        walk.MarkStart(walk.ParseCommit(tree1CommitId));
                        walk.MarkStart(walk.ParseCommit(tree2CommitId));
                        var baseTree = walk.Next();

                        var tw = new NameConflictTreeWalk(db);
                        tw.AddTree(new RevWalk(db).ParseTree(tree1CommitId).ToObjectId());
                        tw.AddTree(new RevWalk(db).ParseTree(tree2CommitId).ToObjectId());
                        if (baseTree != null)
                            tw.AddTree(new RevWalk(db).ParseTree(baseTree.ToObjectId()).ToObjectId());
                        tw.Filter = PathFilter.Create(conflict.Key);

                        // Should be only one iteration
                        while (tw.Next())
                        {
                            var tree0 = baseTree != null ? tw.GetTree<AbstractTreeIterator>(2) : null;
                            var tree1 = tw.GetTree<AbstractTreeIterator>(0);
                            var tree2 = tw.GetTree<AbstractTreeIterator>(1);

                            // Get contents of every versions for the 3-way merge
                            var data0 = baseTree != null ? LoadEntities(new MemoryStream(tw.ObjectReader.Open(tree0.EntryObjectId).GetBytes())) : null;
                            var data1 = LoadEntities(new MemoryStream(tw.ObjectReader.Open(tree1.EntryObjectId).GetBytes()));
                            var data2 = LoadEntities(new MemoryStream(tw.ObjectReader.Open(tree2.EntryObjectId).GetBytes()));

                            // Perform 3-way merge
                            var entities = new List<EntityDefinition>();
                            ThreeWayMergeOrdered.Merge(entities, data0, data1, data2, x => x.Guid, (x, y) => x == y, ResolveEntityConflicts);

                            // Save new merged file
                            var fileStream = new FileStream(new FilePath(db.WorkTree, conflict.Key), FileMode.Create, FileAccess.Write);
                            var stream = new BinarySerializationWriter(fileStream);
                            stream.Context.Serializer = Serializer;
                            stream.SerializeClass(null, ref entities, ArchiveMode.Serialize);
                            fileStream.Close();

                            // TODO: Check if all conflicts are really resolved
                            // Add resolved file for merge commit
                            git.Add().AddFilepattern(conflict.Key).Call();
                        }
                    }
                }
            }
        }

        // Resolve conflicts for an entity
        public static void ResolveEntityConflicts(ThreeWayConflictType conflictType, IList<EntityDefinition>[] lists, int[] indices, IList<EntityDefinition> result)
        {
            switch (conflictType)
            {
                case ThreeWayConflictType.Modified1And2:
                    var entityDefinition = new EntityDefinition(lists[0][indices[0]].Guid);
                    ThreeWayMergeOrdered.Merge(entityDefinition.Components, lists[0][indices[0]].Components, lists[1][indices[1]].Components, lists[2][indices[2]].Components, x => x.Name, (x, y) => x == y, ResolveComponentConflicts);
                    result.Add(entityDefinition);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // Resolve conflicts for a component
        public static void ResolveComponentConflicts(ThreeWayConflictType conflictType, IList<EntityComponentDefinition>[] lists, int[] indices, IList<EntityComponentDefinition> result)
        {
            switch (conflictType)
            {
                case ThreeWayConflictType.Modified1And2:
                    var componentDefinition = new EntityComponentDefinition { Name = lists[0][indices[0]].Name, Properties = new List<EntityComponentProperty>() };
                    ThreeWayMergeOrdered.Merge(componentDefinition.Properties, lists[0][indices[0]].Properties, lists[1][indices[1]].Properties, lists[2][indices[2]].Properties, x => x.Name, (x, y) => x == y, ResolvePropertyConflicts);
                    result.Add(componentDefinition);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // Resolve conflicts for a property
        public static void ResolvePropertyConflicts(ThreeWayConflictType conflictType, IList<EntityComponentProperty>[] lists, int[] indices, IList<EntityComponentProperty> result)
        {
            switch (conflictType)
            {
                case ThreeWayConflictType.Modified1And2:
                    result.Add(lists[1][indices[1]]);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        [XenkoScript]
        public static void LoadAssets(EngineContext engineContext)
        {
            var fileStream = new FileStream(@"C:\DEV\hotei_scene\scene.hotei", FileMode.Open, FileAccess.Read);
            var entities = LoadEntities(fileStream);
            fileStream.Close();

            foreach (var entityDefinition in entities)
            {
                var entity = engineContext.EntityManager.Entities.FirstOrDefault(x => x.Guid == entityDefinition.Guid);
                if (entity == null)
                {
                    entity = new Entity(entityDefinition.Guid);
                    engineContext.EntityManager.AddEntity(entity);
                }

                foreach (var componentDefinition in entityDefinition.Components)
                {
                    EntityComponent component = null;
                    if (componentDefinition.Name == "TransformationComponent")
                    {
                        component = entity.Get(TransformationComponent.Key);
                        if (component == null)
                        {
                            component = new TransformationTRSComponent();
                            entity.Set(TransformationComponent.Key, (TransformationComponent)component);
                        }
                    }
                    else
                    {
                        continue;
                    }

                    foreach (var componentProperty in componentDefinition.Properties)
                    {
                        switch (componentProperty.Type)
                        {
                            case EntityComponentPropertyType.Field:
                                var field = component.GetType().GetField(componentProperty.Name);
                                field.SetValue(component, Decode(componentProperty.Data));
                                break;
                            case EntityComponentPropertyType.Property:
                                var property = component.GetType().GetProperty(componentProperty.Name);
                                property.SetValue(component, Decode(componentProperty.Data), null);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }
        }

        private static object LoadAssetFromUrl(ContentManager contentManager, string url)
        {
            throw new NotImplementedException();
        }

        private static byte[] Encode(object obj, Serializer serializer = null)
        {
            var result = new MemoryStream();
            var stream = new BinarySerializationWriter(result);
            if (serializer != null)
                stream.Context.Serializer = serializer;
            stream.SerializeExtended(null, ref obj, ArchiveMode.Serialize);

            return result.ToArray();
        }

        private static object Decode(byte[] data, Serializer serializer = null)
        {
            object result = null;
            var stream = new BinarySerializationReader(new MemoryStream(data));
            if (serializer != null)
                stream.Context.Serializer = serializer;
            stream.SerializeExtended(null, ref result, ArchiveMode.Deserialize);

            return result;
        }
    }
}
