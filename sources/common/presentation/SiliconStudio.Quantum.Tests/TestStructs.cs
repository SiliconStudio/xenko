// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core;

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
            var container = new NodeContainer();
            container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);

            // Members should never have children
            Assert.That(model.GetChild(nameof(ClassWithStructs.PrimitiveStruct)).Children.Count, Is.EqualTo(0));
            // Primitive struct has been registered as a primitive type, so it should not hold a reference.
            Assert.Null(model.GetChild(nameof(ClassWithStructs.PrimitiveStruct)).Content.Reference);
            // The nested struct should have two children.
            Assert.That(model.GetChild(nameof(ClassWithStructs.NestedStruct)).Children.Count, Is.EqualTo(2));
            // NestedStruct members should be directly accessible, no reference.
            Assert.Null(model.GetChild(nameof(ClassWithStructs.NestedStruct)).Content.Reference);

            var structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct));
            // Similarly, the Struct member of the NestedStruct should be directly accessible, no reference.
            Assert.Null(structNode.GetChild(nameof(NestedStruct.Struct)).Content.Reference);

            structNode = structNode.GetChild(nameof(NestedStruct.Struct));
            // It should have two children, as the SimpleStruct has.
            Assert.That(structNode.Children.Count, Is.EqualTo(2));
            // Finally, we run the ModelConsistencyCheckVisitor to detect potential other issues.
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestViewModelUpdate()
        {
            var obj = new ClassWithStructs();
            var container = new NodeContainer();
            //container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            // Modify direct struct via Quantum, check value on actual object
            var structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct));
            structNode.GetChild(nameof(NestedStruct.SecondValue)).Content.Update(15);
            Assert.That(obj.NestedStruct.SecondValue, Is.EqualTo(15));
            // Modify nested struct via Quantum, check value on actual object
            structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct)).GetChild(nameof(NestedStruct.Struct));
            structNode.GetChild(nameof(PrimitiveStruct.FirstValue)).Content.Update(20);
            Assert.That(obj.NestedStruct.Struct.FirstValue, Is.EqualTo(20));
            // Modify another value of the nested struct via Quantum, check value on actual object
            structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct)).GetChild(nameof(NestedStruct.Struct));
            structNode.GetChild(nameof(PrimitiveStruct.SecondValue)).Content.Update(30);
            Assert.That(obj.NestedStruct.Struct.FirstValue, Is.EqualTo(20));
            Assert.That(obj.NestedStruct.Struct.SecondValue, Is.EqualTo(30));
            // Modify direct struct on actual value, check value via Quantum
            obj.NestedStruct = new NestedStruct { Struct = new SimpleStruct { FirstValue = 30 }, SecondValue = 10 };
            // TODO: this is needed to refresh the references in the node - maybe we could add a Refresh method in the IModelNode?
            model = container.GetNode(obj);
            structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct));
            Assert.That(structNode.GetChild(nameof(PrimitiveStruct.SecondValue)).Content.Value, Is.EqualTo(10));
            structNode = model.GetChild(nameof(ClassWithStructs.NestedStruct)).GetChild(nameof(NestedStruct.Struct));
            Assert.That(structNode.GetChild(nameof(PrimitiveStruct.FirstValue)).Content.Value, Is.EqualTo(30));
            // Finally, we run the ModelConsistencyCheckVisitor to detect potential other issues.
            Helper.ConsistencyCheck(container, obj);
        }
    }
}
