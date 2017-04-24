// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.ViewModel;
using SiliconStudio.Xenko.Games.Serialization.Contents;

namespace ScriptTest
{
    public class EffectMeshEnumerator : IChildrenPropertyEnumerator
    {
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            throw new NotImplementedException();
        }
    }

    public class EntityComponentEnumerator : IChildrenPropertyEnumerator
    {
        private EngineContext engineContext;

        public EntityComponentEnumerator(EngineContext engineContext)
        {
            this.engineContext = engineContext;
        }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            bool handleValueType = false;

            if (viewModelNode.NodeValue is MicroThread)
            {
                var microThread = (MicroThread)viewModelNode.NodeValue;
                var script = microThread.Get(ScriptManager.ScriptProperty);

                viewModelNode.Content = new EnumerableViewModelContent<ViewModelReference>(() => new[] { new ViewModelReference(script, true) });

                handled = true;
            }
            else if (viewModelNode.NodeValue is IScript)
            {
                var script = (IScript)viewModelNode.NodeValue;

                // Expose all variables of IScript (defined by user)
                foreach (var property in script.GetType().GetProperties())
                {
                    if (property.PropertyType != typeof(int) && property.PropertyType != typeof(float))
                        continue;

                    viewModelNode.Children.Add(new ViewModelNode(property.Name, new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), property)));
                }
                handled = true;
            }
            else if (viewModelNode.NodeValue is Entity)
            {
                ViewModelNode componentsViewModelNode;

                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(Entity).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("Guid", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(Entity).GetProperty("Guid"))));
                viewModelNode.Children.Add(componentsViewModelNode = new ViewModelNode("Components", EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, Entity>(new ParentNodeValueViewModelContent(), 
                    (entity) => entity.Properties
                                        .Select(x => x.Value)
                                        .OfType<EntityComponent>()
                                        .Select(x => new ViewModelReference(x, true)))));

                var availableKeysContent = new RootViewModelContent(null, typeof(string[]));
                componentsViewModelNode.Children.Add(new ViewModelNode("AvailableKeys", availableKeysContent));

                componentsViewModelNode.Children.Add(new ViewModelNode("RequestKeys", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var availableComponentKeys = new List<string>();

                        // TODO: Improve component keys enumeration (maybe need a registry?)
                        // For now, scan assemblies for all types inheriting from EntityComponent
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.IsSubclassOf(typeof(EntityComponent))
                                    && type.GetField("Key", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null)
                                {
                                    availableComponentKeys.Add(type.AssemblyQualifiedName);
                                }
                            }
                        }

                        availableKeysContent.Value = availableComponentKeys.ToArray();
                    }))));

                componentsViewModelNode.Children.Add(new ViewModelNode("Add", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var entity = (Entity)viewModel2.Parent.Parent.NodeValue;
                        var componentType = Type.GetType((string)parameter);
                        
                        // For now, assume it will be stored in a PropertyKey inside the actual component named "Key"
                        var componentKeyField = componentType.GetField("Key", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                        entity.SetObject((PropertyKey)componentKeyField.GetValue(null), Activator.CreateInstance(componentType));
                    }))));

                handled = true;
            }
            else if (viewModelNode.NodeValue is EntityComponent)
            {
                viewModelNode.PropertyName = "EntityComponent";
                var component = (EntityComponent)viewModelNode.NodeValue;
                // Would be better higher in the hierarchy, but it would complicate model
                var propertyKey = component.Entity.Properties.First(x => x.Value == component).Key;
                var propertyKeyName = propertyKey.OwnerType.Name;
                viewModelNode.Children.Add(new ViewModelNode("PropertyKeyName", new RootViewModelContent(propertyKeyName)));
                viewModelNode.Children.Add(new ViewModelNode("Remove", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    component.Entity.SetObject(propertyKey, null);
                }))));

                var componentViewModel = new ViewModelNode("Component", component);
                viewModelNode.Children.Add(componentViewModel);
                if (component is TransformationComponent)
                {
                    componentViewModel.Children.Add(new ViewModelNode("WorldMatrix", new FieldInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(TransformationComponent).GetField("WorldMatrix"))).GenerateChildren(context));

                    // TODO: How to switch view model depending on TransformationComponent.Values type? Or should we always expose everything?
                    componentViewModel.Children.Add(new ViewModelNode("LocalMatrix", new FieldInfoViewModelContent(new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(TransformationComponent).GetProperty("Value")), typeof(TransformationValue).GetField("LocalMatrix"))).GenerateChildren(context));

                    //if (((TransformationComponent)component).Values is TransformationTRS)
                    {
                        componentViewModel.Children.Add(new ViewModelNode("Translation", new FieldInfoViewModelContent(new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(TransformationComponent).GetProperty("Value")), typeof(TransformationTRS).GetField("Translation"))).GenerateChildren(context));
                        componentViewModel.Children.Add(new ViewModelNode("Rotation", new FieldInfoViewModelContent(new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(TransformationComponent).GetProperty("Value")), typeof(TransformationTRS).GetField("Rotation"))).GenerateChildren(context));
                        componentViewModel.Children.Add(new ViewModelNode("Scaling", new FieldInfoViewModelContent(new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(TransformationComponent).GetProperty("Value")), typeof(TransformationTRS).GetField("Scaling"))).GenerateChildren(context));
                    }

                    componentViewModel.Children.Add(new ViewModelNode("Parent", LambdaViewModelContent<ViewModelReference>.FromOperand<EntityComponent>(new ParentNodeValueViewModelContent(), x => new ViewModelReference(x.Entity, false))));
                    componentViewModel.Children.Last().Children.Add(new ViewModelNode("SetAsRoot", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                        {
                            context.ViewModelByGuid.Clear();
                            context.Root = context.GetModelView(((TransformationComponent)component).Parent.Entity).Children.First(x => x.PropertyName == "Components");
                        }))));
                }
                if (component is ModelComponent)
                {
                    componentViewModel.Children.Add(new ViewModelNode("Parameters", ((ModelComponent)component).MeshParameters).GenerateChildren(context));
                    //componentViewModel.Children.Add(new ViewModelNode(
                    //    "MeshParameters",
                    //    EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, ModelComponent>(
                    //        new ParentNodeValueViewModelContent(),
                    //        (ModelComponent) => ModelComponent.MeshParameters.Keys.Select(key => new ViewModelReference(Tuple.Create(ModelComponent.MeshParameters, key), true)))));
                }
                if (component is LightComponent)
                {
                    componentViewModel.Children.Add(new ViewModelNode("Type", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("Type"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("ShadowMap", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("ShadowMap"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("Deferred", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("Deferred"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("Intensity", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("Intensity"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("DecayStart", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("DecayStart"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("Color", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("Color"))).GenerateChildren(context));
                    componentViewModel.Children.Add(new ViewModelNode("LightDirection", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightComponent).GetProperty("LightDirection"))).GenerateChildren(context));
                }
                if (component is LightShaftsComponent)
                {
                    componentViewModel.Children.Add(new ViewModelNode("Color", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightShaftsComponent).GetProperty("Color"))).GenerateChildren(context));
                    //componentViewModel.Children.Add(new ViewModelNode("LightShaftsBoundingBoxes", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(LightShaftsComponent).GetProperty("LightShaftsBoundingBoxes"))).GenerateChildren(context));
                }

                // Else try to display it using auto-display
                AutoDisplayComponent(context, viewModelNode, component);

                handled = true;
            }
            else if (viewModelNode.NodeValue is ParameterCollection)
            {
                viewModelNode.Content = EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, ParameterCollection>(new NodeValueViewModelContent(), (parameterCollection) =>
                    parameterCollection.Keys.Where(key => key.PropertyType.IsValueType).Select(key =>
                    {
                        if (key.PropertyType.IsValueType)
                        {
                            // For value type, generated tree won't change so make value based on key only.
                            return new ViewModelReference(Tuple.Create(parameterCollection, key), true);
                        }
                        else
                        {
                            // TODO: resources currently ignored (until fixed)

                            // For reference type, make value dependent on actual value reference and source.
                            // This will trigger a regeneration for reference change (i.e. new texture bound).
                            // Useful since asset type/state might be different.
                            var value = parameterCollection.GetObject(key);
                            var valueSource = engineContext.AssetManager.Url.Get(value);
                            return new ViewModelReference(Tuple.Create(parameterCollection, key, valueSource), true);
                        }
                    }));

                var availableKeysContent = new RootViewModelContent(null, typeof(string[]));
                viewModelNode.Children.Add(new ViewModelNode("AvailableKeys", availableKeysContent));

                viewModelNode.Children.Add(new ViewModelNode("RequestKeys", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var parameterCollection = (ParameterCollection)viewModel2.Parent.NodeValue;
                        var effectMesh = viewModel2.Parent.Parent.NodeValue as EffectMesh;
                        if (effectMesh != null)
                        {
                            var availableKeys = effectMesh.Effect.Passes.SelectMany(x => x.DefaultParameters.Parameters.Select(y => y.Key)).Distinct().Where(x => !parameterCollection.IsValueOwner(x)).Select(x => x.Name).ToArray();
                            availableKeysContent.Value = availableKeys;
                        }
                    }))));

                viewModelNode.Children.Add(new ViewModelNode("Add", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                    {
                        var effectMesh = viewModel2.Parent.Parent.NodeValue as EffectMesh;
                        if (effectMesh != null)
                        {
                            var key = effectMesh.Effect.Passes.SelectMany(x => x.DefaultParameters.Parameters.Select(y => y.Key)).FirstOrDefault(x => x.Name == (string)parameter);
                            if (key != null)
                            {
                                effectMesh.Parameters.SetDefault(key);
                            }
                        }
                    }))));
            }
            else if (viewModelNode.NodeValue is EffectMesh)
            {
                viewModelNode.PropertyName = "Mesh";
                viewModelNode.Children.Add(new ViewModelNode("Effect", LambdaViewModelContent<string>.FromParent<EffectMesh>(x => x.EffectMeshData.EffectData.Name, (x, effectName) => x.EffectMeshData.EffectData.Name = effectName)));
                viewModelNode.Children.Add(new ViewModelNode("Parameters", ((EffectMesh)viewModelNode.NodeValue).Parameters).GenerateChildren(context));
                //viewModelNode.Children.Add(new ViewModelNode("MeshData", LambdaViewModelContent<ViewModelReference>.FromParent<MeshData>(effectMeshData => new ViewModelReference(effectMeshData.MeshData, true))));
                handled = true;
            }
            else if (viewModelNode.NodeValue is ContentData || typeof(ContentData).IsAssignableFrom(viewModelNode.Type))
            {
                if (viewModelNode.NodeValue is ContentData)
                    viewModelNode.Content = new NodeValueViewModelContent();

                if (viewModelNode.Value != null)
                {
                    viewModelNode.Children.Add(new ViewModelNode("Url", new LambdaViewModelContent<string>(new ParentValueViewModelContent(),
                        x => engineContext.AssetManager.Url.Get((x.Value)),
                        (x, y) =>
                        {
                            var nodeValue = x.OwnerNode.Parent.NodeValue;
                        })));

                    viewModelNode.Children.Add(new ViewModelNode("ChangeUrl", new RootViewModelContent((ExecuteCommand)(async (viewModel2, parameter) =>
                        {
                            var dropParameters = (DropCommandParameters)parameter;
                            var parameterInfo = (Tuple<ParameterCollection, ParameterKey, ContentData>)viewModel2.Parent.Parent.NodeValue;

                            var parameterCollection = parameterInfo.Item1;

                            var textureData = await engineContext.AssetManager.LoadAsync<Image>((string)dropParameters.Data);
                            //parameter.Item1.SetObject(parameter.Item2, );
                            //parameterCollection.Remove(parameterInfo.Item2);

                            //var texture = engineContext.ContentManager.Convert<ITexture, Image>(textureData);
                            Texture texture;
                            throw new NotImplementedException();

                            parameterCollection.SetObject(parameterInfo.Item2, texture);
                        }))));

                    if (viewModelNode.Type == typeof(Image))
                    {
                        Image thumbnail = null;
                        Task<Image> textureData = null;

                        viewModelNode.Children.Add(new ViewModelNode("Thumbnail", new LambdaViewModelContent<Image>(new ParentValueViewModelContent(), (viewModelContent) =>
                            {
                                if (textureData == null)
                                {
                                    var textureDataNew = viewModelContent.Value as Image;
                                    if (engineContext.AssetManager.Url.Get(textureDataNew) != null)
                                    {
                                        textureData = engineContext.AssetManager.LoadAsync<Image>(engineContext.AssetManager.Url.Get(textureDataNew));
                                        textureData.ContinueWith(task =>
                                            {
                                                thumbnail = task.Result;
                                                viewModelContent.OwnerNode.Content.SerializeFlags |= ViewModelContentSerializeFlags.Static;
                                            });
                                    }
                                }
                                return thumbnail;
                            })));
                    }
                }

                handled = true;
            }
            else if (viewModelNode.NodeValue is MeshData)
            {
                handled = true;
            }
            else if (viewModelNode.NodeValue is Tuple<ParameterCollection, ParameterKey, ContentData>)
            {
                var value = (Tuple<ParameterCollection, ParameterKey, ContentData>)viewModelNode.NodeValue;

                // Ignore namespace and class name for key name
                viewModelNode.PropertyName = value.Item2.Name;
                if (viewModelNode.PropertyName.Contains('.'))
                    viewModelNode.PropertyName = viewModelNode.PropertyName.Substring(viewModelNode.PropertyName.LastIndexOf('.') + 1);

                viewModelNode.Content = new NullViewModelContent(typeof(Image));
                if (value.Item3 != null)
                    viewModelNode.Children.Add(new ViewModelNode("ObjectRef", new RootViewModelContent(value.Item3) { SerializeFlags = ViewModelContentSerializeFlags.None }).GenerateChildren(context));
                handled = true;
            }
            else if (viewModelNode.NodeValue is Tuple<ParameterCollection, ParameterKey>)
            {
                var value = (Tuple<ParameterCollection, ParameterKey>)viewModelNode.NodeValue;

                // Ignore namespace and class name for key name
                viewModelNode.PropertyName = value.Item2.Name;
                if (viewModelNode.PropertyName.Contains('.'))
                    viewModelNode.PropertyName = viewModelNode.PropertyName.Substring(viewModelNode.PropertyName.LastIndexOf('.') + 1);

                if (value.Item2.PropertyType.IsValueType)
                {
                    viewModelNode.Content =
                        new LambdaViewModelContent<object>(() => value.Item1.GetObject(value.Item2), newValue => value.Item1.SetObject(value.Item2, newValue))
                        {
                            Type = value.Item2.PropertyType
                        };

                    handleValueType = true;
                }
                handled = true;
            }
            else if (viewModelNode.Type == typeof(Matrix) || viewModelNode.Type == typeof(Vector3) || viewModelNode.Type == typeof(Color))
            {
                handled = true;
            }
            else if (viewModelNode.Type.IsValueType)
            {
                handleValueType = true;
            }

            if (handleValueType)
            {
                if (!(viewModelNode.Type == typeof(Matrix) || viewModelNode.Type == typeof(Vector3) || viewModelNode.Type == typeof(Color) || viewModelNode.Type == typeof(Color3)))
                {
                    if (viewModelNode.Type.IsValueType && !viewModelNode.Type.IsPrimitive && !viewModelNode.Type.IsEnum)
                    {
                        viewModelNode.Content.SerializeFlags = ViewModelContentSerializeFlags.None;
                        // Use default for those types
                        foreach (var fieldinfo in viewModelNode.Type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                        {

                            IViewModelContent viewModelContent = new FieldInfoViewModelContent(new ParentValueViewModelContent(viewModelNode.Type), fieldinfo);
                            //if (fieldinfo.FieldType.IsValueType && !fieldinfo.FieldType.IsPrimitive && !fieldinfo.FieldType.IsEnum)
                            //    viewModelContent.Flags = ViewModelFlags.None;
                            // Doesn't support array
                            if (fieldinfo.FieldType.IsArray)
                                continue;
                            viewModelNode.Children.Add(new ViewModelNode(fieldinfo.Name, viewModelContent).GenerateChildren(context));
                        }
                        handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Display components that are tagged with the <see cref="DisplayAttribute"/>.
        /// </summary>
        /// <param name="context">Context of the view model.</param>
        /// <param name="viewModel">The current view model</param>
        /// <param name="component">The entity component to display</param>
        private void AutoDisplayComponent(ViewModelContext context, IViewModelNode viewModel, EntityComponent component)
        {
            var displayComp = DisplayAttribute.GetDisplay(component.GetType());
            if (displayComp == null)
                return;

            var componentViewModel = viewModel.GetChildrenByName("Component");
            if (componentViewModel == null)
                return;

            // Change the name of the component being displayed
            if (!string.IsNullOrEmpty(displayComp.Name))
            {
                var componentName = viewModel.GetChildrenByName("PropertyKeyName");
                if (componentName != null)
                {
                    componentName.Value = displayComp.Name;                    
                }
            }

            var propertyToDisplay = new List<Tuple<DisplayAttribute, ViewModelNode>>();
            var memberInfos = new List<MemberInfo>();
            memberInfos.AddRange(component.GetType().GetProperties());
            memberInfos.AddRange(component.GetType().GetFields());

            // Process fields and properties
            foreach (var property in memberInfos)
            {
                var display = DisplayAttribute.GetDisplay(property);
                if (display == null) continue;

                IViewModelContent modelContent = null;
                object modelValue = null;

                var propertyInfo = property as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (typeof(ParameterCollection).IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        modelValue = propertyInfo.GetValue(component, null);
                    }
                    else
                    {
                        modelContent = new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), propertyInfo);
                    }
                }

                var fieldInfo = property as FieldInfo;
                if (fieldInfo != null)
                {
                    if (typeof(ParameterCollection).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        modelValue = fieldInfo.GetValue(component);
                    }
                    else
                    {
                        modelContent = new FieldInfoViewModelContent(new ParentNodeValueViewModelContent(), fieldInfo);
                    }
                }

                var propertyViewModel = modelValue != null ? new ViewModelNode(display.Name ?? property.Name, modelValue) : new ViewModelNode(display.Name ?? property.Name, modelContent);
                propertyViewModel.GenerateChildren(context);
                propertyToDisplay.Add(new Tuple<DisplayAttribute, ViewModelNode>(display, propertyViewModel));
            }

            foreach(var item in propertyToDisplay.OrderBy((left) => left.Item1.Order))
            {
                componentViewModel.Children.Add(item.Item2);
            }
        }
    }
}
