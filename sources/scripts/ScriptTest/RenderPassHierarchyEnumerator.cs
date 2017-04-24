// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;

using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.ViewModel;

namespace ScriptTest
{
    public class RenderPassHierarchyEnumerator : IChildrenPropertyEnumerator
    {
        public void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled)
        {
            if (viewModelNode.NodeValue is RenderPass)
            {
                viewModelNode.Children.Add(new ViewModelNode("Name", new PropertyInfoViewModelContent(new ParentNodeValueViewModelContent(), typeof(ComponentBase).GetProperty("Name"))));
                viewModelNode.Children.Add(new ViewModelNode("SubPasses", EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, RenderPass>(new ParentNodeValueViewModelContent(),
                    (renderPass) => renderPass.Passes != null
                                        ? renderPass.Passes.Select(x => new ViewModelReference(x, true))
                                        : Enumerable.Empty<ViewModelReference>())));
            }
        }
    }
}
