// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestStructs
    {
        #region Test class definitions
        public struct SimpleStruct
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public struct NestedStruct
        {
            [DataMember(1)]
            public SimpleStruct Struct { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
       }

        public struct PrimitiveStruct
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public class ClassWithStructs
        {
            [DataMember(1)]
            public NestedStruct NestedStruct { get; set; }

            [DataMember(2)]
            public PrimitiveStruct PrimitiveStruct { get; set; }
        }
        #endregion Test class definitions

        [Test]
        public void TestConstruction()
        {
            var obj = new ClassWithStructs();
            var container = new ModelContainer();
            container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Helper.PrintModelContainerContent(container, model);

            // Members should never have children
            Assert.That(model.GetChild("PrimitiveStruct").Children.Count, Is.EqualTo(0));
            // Primitive struct has been registered as a primitive type, so it should not hold a reference.
            Assert.Null(model.GetChild("PrimitiveStruct").Content.Reference);
            // Members should never have children.
            Assert.That(model.GetChild("NestedStruct").Children.Count, Is.EqualTo(0));
            // NestedStruct members should be accessible via a reference.
            Assert.NotNull(model.GetChild("NestedStruct").Content.Reference);
            // The referenced node must exist.
            var structNode = model.GetChild("NestedStruct").Content.Reference.AsObject.TargetNode;
            Assert.NotNull(structNode);
            // It should have two children, as the NestedStruct has.
            Assert.That(structNode.Children.Count, Is.EqualTo(2));
            // Similarly, the Struct member of the NestedStruct should hold a reference.
            Assert.NotNull(structNode.GetChild("Struct").Content.Reference);
            // The referenced node must exist.
            structNode = structNode.GetChild("Struct").Content.Reference.AsObject.TargetNode;
            Assert.NotNull(structNode);
            // It should have two children, as the SimpleStruct has.
            Assert.That(structNode.Children.Count, Is.EqualTo(2));
            // Finally, we run the ModelConsistencyCheckVisitor to detect potential other issues.
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestViewModelUpdate()
        {
            var obj = new ClassWithStructs();
            var container = new ModelContainer();
            container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Helper.PrintModelContainerContent(container, model);
            // Modify direct struct via Quantum, check value on actual object
            var structNode = container.GetModelNode(((ObjectReference)model.GetChild("NestedStruct").Content.Reference).TargetGuid);
            structNode.GetChild("SecondValue").Content.Update(15);
            Assert.That(obj.NestedStruct.SecondValue, Is.EqualTo(15));
            // Modify nested struct via Quantum, check value on actual object
            structNode = container.GetModelNode(((ObjectReference)structNode.GetChild("Struct").Content.Reference).TargetGuid);
            structNode.GetChild("FirstValue").Content.Update(20);
            Assert.That(obj.NestedStruct.Struct.FirstValue, Is.EqualTo(20));
            // Modify direct struct on actual value, check value via Quantum
            obj.NestedStruct = new NestedStruct { Struct = new SimpleStruct { FirstValue = 30 }, SecondValue = 10 };
            // TODO: this is needed to refresh the references in the node - maybe we could add a Refresh method in the IModelNode?
            model = container.GetModelNode(obj);
            structNode = container.GetModelNode(((ObjectReference)model.GetChild("NestedStruct").Content.Reference).TargetGuid);
            Assert.That(structNode.GetChild("SecondValue").Content.Value, Is.EqualTo(10));
            structNode = container.GetModelNode(((ObjectReference)structNode.GetChild("Struct").Content.Reference).TargetGuid);
            Assert.That(structNode.GetChild("FirstValue").Content.Value, Is.EqualTo(30));
            // Finally, we run the ModelConsistencyCheckVisitor to detect potential other issues.
            Helper.ConsistencyCheck(container, obj);
        }
    }
}
