// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NUnit.Framework;

using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestMultipleReferencer
    {
        public class SimpleObject
        {
            private static int counter;
            public SimpleObject()
            {
                Name = $"(Simple Object {++counter})";
            }

            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public class DoubleReferenceClass
        {
            public DoubleReferenceClass(SimpleObject obj) : this()
            {
                Object1 = obj;
                Object2 = obj;
                List1.Add(obj);
            }

            public DoubleReferenceClass(SimpleObject obj1, SimpleObject obj2) : this()
            {
                Object1 = obj1;
                Object2 = obj2;
                List1.Add(obj1);
                List1.Add(obj2);
            }

            private DoubleReferenceClass()
            {
                List1 = new List<SimpleObject>();
                List2 = List1;
            }

            public SimpleObject Object1 { get; set; }
            public SimpleObject Object2 { get; set; }

            public List<SimpleObject> List1 { get; set; }
            public List<SimpleObject> List2 { get; set; }
        }

        // TODO: multiple references in lists, dictionaries, structs

        [Test]
        public void TestDoubleReferenceAtConstruction()
        {
            var doubleRef = new DoubleReferenceClass(new SimpleObject());
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(doubleRef);
            Assert.That(doubleRef.Object1, Is.EqualTo(doubleRef.Object2));
            Assert.That(model.GetChild("Object1").Content.Value, Is.EqualTo(doubleRef.Object1));
            Assert.That(model.GetChild("Object2").Content.Value, Is.EqualTo(doubleRef.Object2));

            Assert.That(model.GetChild("Object1").Content.IsReference, Is.True);
            Assert.That(model.GetChild("Object2").Content.IsReference, Is.True);

            var object1TargetNode = ((ObjectReference)model.GetChild("Object1").Content.Reference).TargetNode;
            var object2TargetNode = ((ObjectReference)model.GetChild("Object2").Content.Reference).TargetNode;
            Assert.That(object1TargetNode, Is.EqualTo(object2TargetNode));
            Assert.That(object1TargetNode.Content.Value, Is.EqualTo(doubleRef.Object1));
        }

        [Test]
        public void TestDoubleReferenceMemberDataUpdate()
        {
            var doubleRef = new DoubleReferenceClass(new SimpleObject());
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(doubleRef);

            doubleRef.Object1.Name = "New Name";

            Assert.That(doubleRef.Object1.Name, Is.EqualTo("New Name"));
            Assert.That(doubleRef.Object2.Name, Is.EqualTo("New Name"));
            var object1TargetNode = ((ObjectReference)model.GetChild("Object1").Content.Reference).TargetNode;
            var object2TargetNode = ((ObjectReference)model.GetChild("Object2").Content.Reference).TargetNode;
            Assert.That(object1TargetNode.GetChild("Name").Content.Value, Is.EqualTo("New Name"));
            Assert.That(object2TargetNode.GetChild("Name").Content.Value, Is.EqualTo("New Name"));
        }

        [Test]
        public void TestDoubleReferenceMemberQuantumUpdate()
        {
            var doubleRef = new DoubleReferenceClass(new SimpleObject());
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(doubleRef);

            ((ObjectReference)model.GetChild("Object1").Content.Reference).TargetNode.GetChild("Name").Content.Update("New Name");

            Assert.That(doubleRef.Object1.Name, Is.EqualTo("New Name"));
            Assert.That(doubleRef.Object2.Name, Is.EqualTo("New Name"));
            var object1TargetNode = ((ObjectReference)model.GetChild("Object1").Content.Reference).TargetNode;
            var object2TargetNode = ((ObjectReference)model.GetChild("Object2").Content.Reference).TargetNode;
            Assert.That(object1TargetNode.GetChild("Name").Content.Value, Is.EqualTo("New Name"));
            Assert.That(object2TargetNode.GetChild("Name").Content.Value, Is.EqualTo("New Name"));
        }

        // Disabling this test until we got time to fix the issue
        //[Test]
        //public void TestDoubleReferenceBreak()
        //{
        //    var doubleRef = new DoubleReferenceClass(new SimpleObject());
        //    var container = new ModelContainer();
        //    container.GetOrCreateModelNode(doubleRef, doubleRef.GetType());

        //    doubleRef.Object1 = new SimpleObject();

        //    IModelNode viewModel = container.GetOrCreateModelNode(doubleRef, doubleRef.GetType());

        //    Assert.That(doubleRef.Object1, !Is.EqualTo(doubleRef.Object2));
        //    Assert.That(viewModel.GetChild("Object1").Content.Value, Is.EqualTo(doubleRef.Object1));
        //    Assert.That(viewModel.GetChild("Object2").Content.Value, Is.EqualTo(doubleRef.Object2));

        //    Assert.That(viewModel.GetChild("Object1").Content.IsReference, Is.True);
        //    Assert.That(viewModel.GetChild("Object2").Content.IsReference, Is.True);
        //    var object1TargetNode = ((ObjectReference)viewModel.GetChild("Object1").Content.Reference).TargetNode;
        //    var object2TargetNode = ((ObjectReference)viewModel.GetChild("Object2").Content.Reference).TargetNode;
        //    Console.WriteLine(viewModel.PrintHierarchy());
        //    Console.WriteLine(object1TargetNode.PrintHierarchy());
        //    Console.WriteLine(object2TargetNode.PrintHierarchy());
        //    // TODO: the object2 reference is not updated to be different from object1
        //    Assert.That(object1TargetNode, !Is.EqualTo(object2TargetNode));
        //    Assert.That(object1TargetNode.Content.Value, Is.EqualTo(doubleRef.Object1));
        //    Assert.That(object2TargetNode.Content.Value, Is.EqualTo(doubleRef.Object1));
        //}

        // Disabling this test until we got time to fix the issue
        //[Test]
        //public void TestDoubleListReferenceAtConstruction()
        //{
        //    var doubleRef = new DoubleReferenceClass(new SimpleObject());
        //    var container = new ModelContainer();
        //    IModelNode viewModel = container.GetOrCreateModelNode(doubleRef, doubleRef.GetType());
        //    Console.WriteLine(viewModel.PrintHierarchy());
            
        //    Assert.That(doubleRef.List1, Is.EqualTo(doubleRef.List2));
        //    Assert.That(viewModel.GetChild("List1").Content.Value, Is.EqualTo(doubleRef.List1));
        //    Assert.That(viewModel.GetChild("List2").Content.Value, Is.EqualTo(doubleRef.List2));

        //    Assert.That(viewModel.GetChild("List1").Content.IsReference, Is.True);
        //    // TODO: The second list is not built as a reference because the visitor does not visit again the same object
        //    Assert.That(viewModel.GetChild("List2").Content.IsReference, Is.True);

        //    var object1TargetNode = ((ObjectReference)((ReferenceEnumerable)viewModel.GetChild("Object1").Content.Reference).First()).TargetNode;
        //    var object2TargetNode = ((ObjectReference)((ReferenceEnumerable)viewModel.GetChild("Object2").Content.Reference).First()).TargetNode;
        //    Assert.That(object1TargetNode, Is.EqualTo(object2TargetNode));
        //    Assert.That(object1TargetNode.Content.Value, Is.EqualTo(doubleRef.Object1));
        //}
    }
}
