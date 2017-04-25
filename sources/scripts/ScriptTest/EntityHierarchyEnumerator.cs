// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Collections;
using SiliconStudio.Xenko.Games.ViewModel;

namespace ScriptTest
{
    public class EntityHierarchyEnumerator : IChildrenPropertyEnumerator
    {
        private TrackingHashSet<Entity> selectedEntities = new TrackingHashSet<Entity>();
        private PropertyKey<bool> isSelectedProperty;
        private ViewModelContext selectedEntitiesContext;
        private ConditionalWeakTable<EffectMesh, EffectMeshIndex> effectMeshIndices = new ConditionalWeakTable<EffectMesh, EffectMeshIndex>();
        private EntitySystem EntitySystem;

        public EntityHierarchyEnumerator(EntitySystem EntitySystem, ViewModelContext selectedEntitiesContext)
        {
            isSelectedProperty = new PropertyKey<bool>("IsSelected", typeof(EntityHierarchyEnumerator), new StaticDefaultValueMetadata(false) { PropertyUpdateCallback = IsSelectedChanged });

            this.EntitySystem = EntitySystem;
            this.selectedEntitiesContext = selectedEntitiesContext;
        }

        public TrackingHashSet<Entity> SelectedEntities
        {
            get { return selectedEntities; }
        }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is Entity)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(Entity).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("Guid", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(Entity).GetProperty("Guid"))));
                viewModelNode.Children.Add(new ViewModelNode("IsSelected", new PropertyKeyViewModelContent(new ParentNodeValueViewModelContent(), isSelectedProperty)));
                viewModelNode.Children.Add(new ViewModelNode("ParentReference", LambdaViewModelContent<ViewModelReference>.FromParent<Entity>(x =>
                    {
                        var transformationComponent = x.Transformation;
                        var parent = transformationComponent != null ? transformationComponent.Parent : null;
                        return new ViewModelReference(parent != null ? parent.Entity : null);
                    })));
                viewModelNode.Children.Add(new ViewModelNode("HierarchicalEntities", EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, Entity>(new ParentNodeValueViewModelContent(), 
                    (entity) =>
                        {
                            var result = Enumerable.Empty<ViewModelReference>();
                            
                            // Enumerates children nodes
                            var transformationComponent = entity.Transformation;
                            if (transformationComponent != null)
                                result = result.Concat(transformationComponent.Children
                                    .Select(x => new ViewModelReference(x.Entity, true)));

                            // Enumerates EffectMesh
                            var meshComponent = entity.Get(ModelComponent.Key);
                            if (meshComponent != null && meshComponent.InstantiatedSubMeshes != null)
                                result = result.Concat(meshComponent.InstantiatedSubMeshes.Select((x, i) =>
                                    {
                                        effectMeshIndices.GetOrCreateValue(x.Value).Index = i;
                                        return new ViewModelReference(x.Value, true);
                                    }));

                            return result;
                        })));

                viewModelNode.Children.Add(new ViewModelNode("EventOpen", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        ScriptDebug.SelectEntity((Entity)viewModel2.Parent.NodeValue);
                    }))));

                viewModelNode.Children.Add(new ViewModelNode("CreateNewEntity", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var entity = (Entity)viewModel2.Parent.NodeValue;
                        var newEntity = new Entity("New Entity");
                        entity.Transformation.Children.Add(newEntity.Transformation);
                    }))));

                viewModelNode.Children.Add(new ViewModelNode("Remove", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var entity = (Entity)viewModel2.Parent.NodeValue;
                        EntitySystem.Remove(entity);
                    }))));

                handled = true;
            }
            else if (viewModelNode.NodeValue is EffectMesh)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new LambdaViewModelContent<string>(new NullViewModelContent(), (content) =>
                    {
                        var effectMesh = (EffectMesh)content.OwnerNode.Parent.NodeValue;
                        var result = effectMeshIndices.GetOrCreateValue(effectMesh).Index.ToString();
                        if (effectMesh.Name != null)
                            result += " - " + effectMesh.Name;

                        return result;
                    })));

                viewModelNode.Children.Add(new ViewModelNode("EventOpen", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    selectedEntitiesContext.ViewModelByGuid.Clear();
                    selectedEntitiesContext.Root = new ViewModelNode("Root", new RootViewModelContent(new[] { new ViewModelReference(viewModel2.Parent.NodeValue, true) }, typeof(IList<ViewModelReference>)));
                }))));

                handled = true;
            }
        }

        private void IsSelectedChanged(PropertyContainer propertyContainer, PropertyKey propertykey, object newvalue, object oldvalue)
        {
            if ((bool)newvalue)
                selectedEntities.Add((Entity)propertyContainer);
            else
                selectedEntities.Remove((Entity)propertyContainer);
        }

        private class EffectMeshIndex
        {
            public int Index;
        }
    }
}
