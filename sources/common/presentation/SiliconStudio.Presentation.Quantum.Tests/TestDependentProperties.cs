using NUnit.Framework;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;
using TestContext = SiliconStudio.Presentation.Quantum.Tests.Helpers.TestContext;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestDependentProperties
    {
        private const string Title = nameof(Types.DependentPropertyContainer.Title);
        private const string Instance = nameof(Types.DependentPropertyContainer.Instance);
        private const string Name = nameof(Types.SimpleObject.Name);
        private const string Nam = nameof(Types.SimpleObject.Nam);

        private const string TestDataKey = "TestData";
        private const string UpdateCountKey = "UpdateCount";

        private abstract class DependentPropertiesUpdater : IPropertyNodeUpdater
        {
            private int count;
            public void UpdateNode(SingleNodeViewModel node)
            {
                if (node.Name == nameof(Types.DependentPropertyContainer.Title))
                {
                    var instance = (Types.DependentPropertyContainer)node.Owner.RootNode.Value;
                    node.AddAssociatedData(TestDataKey, instance.Instance.Name);

                    var dependencyPath = GetDependencyPath(node.Owner);
                    node.AddDependency(dependencyPath, IsRecursive);

                    node.AddAssociatedData(UpdateCountKey, count++);
                }
            }

            protected abstract bool IsRecursive { get; }

            protected abstract string GetDependencyPath(GraphViewModel viewModel);
        }

        private class SimpleDependentPropertiesUpdater : DependentPropertiesUpdater
        {
            protected override bool IsRecursive => false;

            protected override string GetDependencyPath(GraphViewModel viewModel)
            {
                return viewModel.RootNode.GetChild(Instance).GetChild(Name).Path;
            }
        }

        private class RecursiveDependentPropertiesUpdater : DependentPropertiesUpdater
        {
            protected override bool IsRecursive => true;

            protected override string GetDependencyPath(GraphViewModel viewModel)
            {
                return viewModel.RootNode.GetChild(Instance).Path;
            }
        }

        [Test]
        public void TestSimpleDependency()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestSimpleDependencyChangeParent()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue2" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependency()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue2" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependencyChangeChild()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependencyMixedChanges()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue2" };
            nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue3";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue3", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(3, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.InternalNodeValue = new Types.SimpleObject { Name = "NewValue4" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue4", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(4, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestChangeDifferentPropertyWithSameStart()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.RegisterPropertyNodeUpdater(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            var namNode = viewModel.RootNode.GetChild(Instance).GetChild(Nam);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            namNode.InternalNodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            namNode.InternalNodeValue = "NewValue3";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.InternalNodeValue = "NewValue4";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue4", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }
    }
}
