using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Presentation.Quantum.View
{
    public class RangedValueTemplateProvider : ObservableNodeTemplateProvider
    {
        public override string Name { get { return "RangedValueTemplateProvider"; } }

        public override bool MatchNode(IObservableNode node)
        {
            return node.Type.IsNumeric() && node.AssociatedData.ContainsKey("Minimum") && node.AssociatedData.ContainsKey("Maximum")
                   && node.AssociatedData.ContainsKey("SmallStep") && node.AssociatedData.ContainsKey("LargeStep");
        }
    }
}