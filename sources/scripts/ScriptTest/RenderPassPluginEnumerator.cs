// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Reflection;

using SiliconStudio.Xenko.Effects;
#if XENKO_YEBIS
using Xenko.Effects.Yebis;
#endif
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Mathematics;
using SiliconStudio.Xenko.Games.ViewModel;

namespace ScriptTest
{
    public class RenderPassPluginsEnumerator : IChildrenPropertyEnumerator
    {
        public ViewModelContext SelectedRenderPassPluginContext { get; set; }

        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is RenderPassPlugin)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(ComponentBase).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("EventOpen", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    SelectedRenderPassPluginContext.ViewModelByGuid.Clear();
                    //SelectedRenderPassPluginContext.Root = SelectedRenderPassPluginContext.GetModelView(viewModel2.Parent.NodeValue);
                    SelectedRenderPassPluginContext.Root = new ViewModelNode("Root", new RootViewModelContent(new[] { new ViewModelReference(viewModel2.Parent.NodeValue, true) }, typeof(IList<ViewModelReference>)));
                    //SelectedRenderPassPluginContext.GetModelView(viewModel2.Parent.NodeValue);
                }))));

                handled = true;
            }
        }
    }
    public class RenderPassPluginEnumerator : IChildrenPropertyEnumerator
    {
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is RenderPassPlugin)
            {
                var pluginType = viewModelNode.NodeValue.GetType();

                foreach (var propertyInfo in pluginType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                {
                    IViewModelContent viewModelContent = new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), propertyInfo);

                    if (propertyInfo.PropertyType == typeof(RenderPass) || propertyInfo.PropertyType.IsSubclassOf(typeof(RenderPass)))
                        viewModelContent = LambdaViewModelContent<ViewModelReference>.FromOperand<RenderPass>(viewModelContent, x => new ViewModelReference(x, false));
                    else if (!propertyInfo.PropertyType.IsValueType || !propertyInfo.PropertyType.FullName.StartsWith("System."))
                        continue;

                    viewModelNode.Children.Add(new ViewModelNode(propertyInfo.Name, viewModelContent));
                }


                foreach (var fieldinfo in pluginType.GetFields( BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                {
                    IViewModelContent viewModelContent = new FieldInfoViewModelContent(new ParentNodeValueViewModelContent(pluginType), fieldinfo);
                    //if (fieldinfo.FieldType.IsValueType && !fieldinfo.FieldType.IsPrimitive && !fieldinfo.FieldType.IsEnum)
                    //    viewModelContent.Flags = ViewModelFlags.None;
                    viewModelNode.Children.Add(new ViewModelNode(fieldinfo.Name, viewModelContent).GenerateChildren(context));
                }
            }
#if XENKO_YEBIS
            else if (viewModelNode.Type.Namespace == typeof(ToneMap).Namespace
                && viewModelNode.Type.IsValueType && !viewModelNode.Type.IsPrimitive && !viewModelNode.Type.IsEnum)
            {
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
            }
#endif
        }
    }
}
