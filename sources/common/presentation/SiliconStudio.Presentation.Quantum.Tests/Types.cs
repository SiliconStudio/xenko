using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Presentation.Quantum;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Tests
{
    public static class Types
    {
        public class TestPropertiesProvider : IPropertiesProviderViewModel
        {
            private readonly IGraphNode rootNode;

            public TestPropertiesProvider(IGraphNode rootNode)
            {
                this.rootNode = rootNode;
            }
            public bool CanProvidePropertiesViewModel => true;

            public IGraphNode GetRootNode()
            {
                return rootNode;
            }

            public bool ShouldExpandReference(MemberContent member, ObjectReference reference) => true;

            public bool ShouldConstructMember(MemberContent content) => true;
        }

        public class SimpleObject
        {
            public string Name { get; set; }
        }

        public class DependentPropertyContainer
        {
            public string Title => Instance.Name;

            public SimpleObject Instance { get; set; }
        }
    }
}
