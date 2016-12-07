using NUnit.Framework;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Tests
{
    [TestFixture]
    public class TestDependentProperties
    {
        [Test]
        public void TestSimple()
        {
            var nodeContainer = new NodeContainer();
            var container = new Types.DependentPropertyContainer { Instance = new Types.SimpleObject { Name = "Test" } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var test = Helper.CreateTestContext(containerNode);
        }
    }
}
