// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

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
            var container = new ModelContainer();
            container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            Assert.That(model.GetChild("NestedStruct").Children.Count, Is.EqualTo(2));
            Assert.That(model.GetChild("NestedStruct").GetChild("Struct").Children.Count, Is.EqualTo(2));
            Assert.That(model.GetChild("PrimitiveStruct").Children.Count, Is.EqualTo(0));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithStructs), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestViewModelUpdate()
        {
            var obj = new ClassWithStructs();
            var container = new ModelContainer();
            container.NodeBuilder.PrimitiveTypes.Add(typeof(PrimitiveStruct));
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            model.GetChild("NestedStruct").GetChild("SecondValue").Content.Value = 15;
            model.GetChild("NestedStruct").GetChild("Struct").GetChild("SecondValue").Content.Value = 20;

            Assert.That(obj.NestedStruct.SecondValue, Is.EqualTo(15));
            Assert.That(obj.NestedStruct.Struct.SecondValue, Is.EqualTo(20));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithStructs), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }
    }
}
