using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodePresenterFactory
    {
        INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath);
    }
}
