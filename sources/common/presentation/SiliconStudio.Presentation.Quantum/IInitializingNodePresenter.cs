using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum
{
    public interface IInitializingNodePresenter : INodePresenter
    {
        void AddChild(IInitializingNodePresenter child);

        void FinalizeInitialization();
    }
}
