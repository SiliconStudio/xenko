// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Games.ViewModel;
using Xenko.Framework.Shaders;

namespace ScriptTest
{
    public class EffectEnumerator : IChildrenPropertyEnumerator
    {
        private readonly ViewModelContext selectedEntitiesContext;

        public EffectEnumerator(ViewModelContext selectedEntitiesContext)
        {
            this.selectedEntitiesContext = selectedEntitiesContext;
        }
        
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is EffectBuilder)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(EffectBuilder).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("EventOpen", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    selectedEntitiesContext.ViewModelByGuid.Clear();
                    selectedEntitiesContext.Root = selectedEntitiesContext.GetModelView(viewModel2.Parent.NodeValue);
                }))));

                viewModelNode.Children.Add(new ViewModelNode("Definition", new AsyncViewModelContent<EffectDefinition>(new ParentNodeValueViewModelContent(),
                        operand => new EffectDefinition
                                    {
                                        Plugins = ((EffectBuilder)operand.Value).Plugins.Select(x =>
                                                {
                                                    var pluginDefinition = new EffectPluginDefinition
                                                    {
                                                        PluginType = x.GetType().AssemblyQualifiedName,
                                                        Parameters = new Dictionary<string, EffectParameterDefinition>()
                                                    };
                                                    foreach (var property in x.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                                                    {
                                                        if (property.GetCustomAttributes(typeof(EffectDefinitionPropertyAttribute), true).Length == 0)
                                                            continue;

                                                        // Check type and do some simple conversion
                                                        var value = property.GetValue(x, null);
                                                        if (typeof(RenderPassPlugin).IsAssignableFrom(property.PropertyType))
                                                        {
                                                            value = Guid.NewGuid();
                                                        }
                                                        else if (!typeof(ShaderSource).IsAssignableFrom(property.PropertyType)
                                                            && !property.PropertyType.IsValueType)
                                                        {
                                                            throw new NotSupportedException();
                                                        }
                                                        pluginDefinition.Parameters.Add(property.Name, new EffectParameterDefinition(property.PropertyType, value));
                                                    }
                                                    return pluginDefinition;
                                                }).ToList()
                                    })));
                //new AsyncViewModelContent<EffectDefinition>(() => ) { LoadState = ViewModelContentState.NotLoaded, Flags = ViewModelFlags.Static | ViewModelFlags.Async | ViewModelFlags.Serialize });
            }
        }
    }

    public class EffectPropertyEnumerator : IChildrenPropertyEnumerator
    {
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is EffectOld)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(EffectOld).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("Plugins",
                    LambdaViewModelContent<EffectPluginDefinition[]>.FromParent<EffectOld>(
                        effect => effect.Plugins.Select(plugin => new EffectPluginDefinition { PluginType = plugin.GetType().AssemblyQualifiedName }).ToArray())));
            }
        }
    }
}
