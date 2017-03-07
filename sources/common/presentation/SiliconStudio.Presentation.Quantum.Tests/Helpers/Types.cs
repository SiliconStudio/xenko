using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum.Tests.Helpers
{
    public static class Types
    {
        public class TestPropertiesProvider : IPropertiesProviderViewModel
        {
            private readonly IObjectNode rootNode;

            public TestPropertiesProvider(IObjectNode rootNode)
            {
                this.rootNode = rootNode;
            }
            public bool CanProvidePropertiesViewModel => true;

            public IObjectNode GetRootNode()
            {
                return rootNode;
            }

            public ExpandReferencePolicy ShouldConstructChildren(IGraphNode graphNode, Index index) => ExpandReferencePolicy.Full;

            public bool ShouldConstructMember(IMemberNode member, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
            public bool ShouldConstructItem(IObjectNode collection, Index index, ExpandReferencePolicy expandReferencePolicy) => expandReferencePolicy == ExpandReferencePolicy.Full;
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

        public class SimpleType
        {
            public string String { get; set; }
        }

        public class ClassWithRef
        {
            [Display(1)]
            public string String { get; set; }
            [Display(2)]
            public ClassWithRef Ref { get; set; }
        }

        public class ClassWithCollection
        {
            [Display(1)]
            public string String { get; set; }
            [Display(2)]
            public List<string> List { get; set; } = new List<string>();
        }

    }
}
