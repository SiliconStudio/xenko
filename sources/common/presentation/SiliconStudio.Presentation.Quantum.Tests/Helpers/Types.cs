using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public static class Types
    {
        public class TestPropertiesProvider : IPropertiesProviderViewModel
        {
            private readonly IContentNode rootNode;

            public TestPropertiesProvider(IContentNode rootNode)
            {
                this.rootNode = rootNode;
            }
            public bool CanProvidePropertiesViewModel => true;

            public IContentNode GetRootNode()
            {
                return rootNode;
            }

            public ExpandReferencePolicy ShouldExpandReference(IMemberNode member, ObjectReference reference) => ExpandReferencePolicy.Full;

            public bool ShouldConstructMember(IMemberNode content, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
        }

        public class SimpleObject
        {
            public string Name { get; set; }

            public string Nam { get; set; } // To test scenario when Name.StartsWith(Nam) is true
        }

        public class DependentPropertyContainer
        {
            public string Title { get; set; }

            public SimpleObject Instance { get; set; }
        }
    }
}
