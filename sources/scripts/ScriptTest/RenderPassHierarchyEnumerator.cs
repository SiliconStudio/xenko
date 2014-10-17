using System.Linq;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Games.ViewModel;

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