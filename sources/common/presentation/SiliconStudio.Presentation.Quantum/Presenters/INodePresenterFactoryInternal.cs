using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodePresenterFactoryInternal
    {
        void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode);
    }
}
