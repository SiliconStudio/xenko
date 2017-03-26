using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterMemberAttributes
    {
        public class ClassA
        {
            [Display(1, "String1FromClassA")]
            public string String1 { get; set; }
            [Display(2, "String2FromClassA")]
            public virtual string String2 { get; set; }
            [Display(3, "String3FromClassA")]
            public virtual string String3 { get; set; }
        }

        public class ClassB : ClassA
        {
            [Display(2, "String2FromClassB")]
            public override string String2 { get; set; }
        }

        public class ClassC : ClassB
        {
            [Display(3, "String3FromClassC")]
            public override string String3 { get; set; }
        }

        [Test]
        public void TestSimpleAttribute()
        {
            var instance = new ClassA();
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(3, presenter.Children.Count);

            var string1 = (MemberNodePresenter)presenter.Children[0];
            Assert.AreEqual("String1", string1.Name);
            Assert.AreEqual(1, string1.MemberAttributes.Count);
            CompareAttribute("String1FromClassA", typeof(ClassA).GetProperty("String1").GetCustomAttributes(true)[0], string1.MemberAttributes[0]);

            var string2 = (MemberNodePresenter)presenter.Children[1];
            Assert.AreEqual("String2", string2.Name);
            Assert.AreEqual(1, string1.MemberAttributes.Count);
            CompareAttribute("String2FromClassA", typeof(ClassA).GetProperty("String2").GetCustomAttributes(true)[0], string2.MemberAttributes[0]);

            var string3 = (MemberNodePresenter)presenter.Children[2];
            Assert.AreEqual("String3", string3.Name);
            Assert.AreEqual(1, string3.MemberAttributes.Count);
            CompareAttribute("String3FromClassA", typeof(ClassA).GetProperty("String3").GetCustomAttributes(true)[0], string3.MemberAttributes[0]);
        }

        [Test]
        public void TestInheritedAttribute()
        {
            var instance = new ClassB();
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(3, presenter.Children.Count);

            var string1 = (MemberNodePresenter)presenter.Children[0];
            Assert.AreEqual("String1", string1.Name);
            Assert.AreEqual(1, string1.MemberAttributes.Count);
            CompareAttribute("String1FromClassA", typeof(ClassB).GetProperty("String1").GetCustomAttributes(true)[0], string1.MemberAttributes[0]);

            var string2 = (MemberNodePresenter)presenter.Children[1];
            Assert.AreEqual("String2", string2.Name);
            Assert.AreEqual(1, string2.MemberAttributes.Count);
            CompareAttribute("String2FromClassB", typeof(ClassB).GetProperty("String2").GetCustomAttributes(true)[0], string2.MemberAttributes[0]);

            var string3 = (MemberNodePresenter)presenter.Children[2];
            Assert.AreEqual("String3", string3.Name);
            Assert.AreEqual(1, string3.MemberAttributes.Count);
            CompareAttribute("String3FromClassA", typeof(ClassB).GetProperty("String3").GetCustomAttributes(true)[0], string3.MemberAttributes[0]);
        }

        [Test]
        public void TestDoubleInheritedAttribute()
        {
            var instance = new ClassC();
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(3, presenter.Children.Count);

            var string1 = (MemberNodePresenter)presenter.Children[0];
            Assert.AreEqual("String1", string1.Name);
            Assert.AreEqual(1, string1.MemberAttributes.Count);
            CompareAttribute("String1FromClassA", typeof(ClassC).GetProperty("String1").GetCustomAttributes(true)[0], string1.MemberAttributes[0]);

            var string2 = (MemberNodePresenter)presenter.Children[1];
            Assert.AreEqual("String2", string2.Name);
            Assert.AreEqual(1, string2.MemberAttributes.Count);
            CompareAttribute("String2FromClassB", typeof(ClassC).GetProperty("String2").GetCustomAttributes(true)[0], string2.MemberAttributes[0]);

            var string3 = (MemberNodePresenter)presenter.Children[2];
            Assert.AreEqual("String3", string3.Name);
            Assert.AreEqual(1, string3.MemberAttributes.Count);
            CompareAttribute("String3FromClassC", typeof(ClassC).GetProperty("String3").GetCustomAttributes(true)[0], string3.MemberAttributes[0]);
        }

        private static void CompareAttribute(string expectedName, object expected, object actual)
        {
            Assert.IsInstanceOf<DisplayAttribute>(expected);
            Assert.IsInstanceOf<DisplayAttribute>(actual);
            var expectedDisplay = (DisplayAttribute)expected;
            var actualDisplay = (DisplayAttribute)actual;
            // Assert on the name first, gives a better error message
            Assert.AreEqual(expectedName, expectedDisplay.Name);
            Assert.AreEqual(expectedDisplay.Name, actualDisplay.Name);
            Assert.AreEqual(expectedDisplay, actualDisplay);
        }

        private static IObjectNode BuildQuantumGraph(object instance)
        {
            var container = new NodeContainer();
            var node = container.GetOrCreateNode(instance);
            return node;
        }
    }
}
