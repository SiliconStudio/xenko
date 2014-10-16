using System;
using System.Collections.Generic;

using NUnit.Framework;

using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Assets.Tests.Diff
{
    /// <summary>
    /// Test the <see cref="DataMatcher"/>
    /// </summary>
    [TestFixture()]
    public class TestMatcher
    {
        public class PrimitiveFields
        {
            public string Name { get; set; }

            public int Value { get; set; }

            public Vector2 Position { get; set; }
        }

        [Test]
        public void TestMatchOnPrimitive()
        {
            var value1 = new PrimitiveFields() { Name = "A", Value = 1, Position = new Vector2(1, 2) };
            var value2 = new PrimitiveFields() { Name = "A", Value = 1, Position = new Vector2(1, 2) };

            var result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(4,4)); // Match 3 fields perfectly

            value1.Value = 3;

            result = MatchObjects(value1, value2); 
            Assert.AreEqual(result, new DataMatch(3, 4)); // Match 3 fields perfectly

            value1.Position = new Vector2(3, 4);

            result = MatchObjects(value1, value2); 
            Assert.AreEqual(result, new DataMatch(1, 4)); // Match 3 fields perfectly

            value2.Name = null;

            result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(0, 4)); // Match 3 fields perfectly
        }

        public class ObjectWithImmutableStruct
        {
            public UFile File { get; set; }
        }

        [Test]
        public void TestMatchOnImmutableStruct()
        {
            var value1 = new ObjectWithImmutableStruct() {File = "toto.txt"};
            var value2 = new ObjectWithImmutableStruct() {File = "toto.txt"};

            var result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(1, 1)); // Match 3 fields perfectly

            value1.File = "test.txt";

            result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(0, 1)); // Match 3 fields perfectly
        }

        public class ListPrimitiveFields
        {
            public ListPrimitiveFields()
            {
                Values = new List<int>();
                Positions = new List<Vector2>();
            }

            public List<int> Values { get; set; }

            public List<Vector2> Positions { get; set; }

            public List<string> Null { get; set; } 
        }

        [Test]
        public void TestMatchListPrimitive()
        {
            var value1 = new ListPrimitiveFields();
            value1.Values.Add(1);
            value1.Values.Add(2);
            value1.Positions.Add(new Vector2(3));
            value1.Positions.Add(new Vector2(4));
            var value2 = new ListPrimitiveFields();
            value2.Values.Add(1);
            value2.Values.Add(2);
            value2.Positions.Add(new Vector2(3));
            value2.Positions.Add(new Vector2(4));

            var result = MatchObjects(value1, value2);

            // Match 5/5: Values[0], Values[1], Positions[0], Positions[1] and Null
            Assert.AreEqual(result, new DataMatch(7,7));

            value1.Values[0] = 2; // Changing a list element will generate a Added and Deleted event

            result = MatchObjects(value1, value2); 
            Assert.AreEqual(result, new DataMatch(6, 8));

            value1.Positions[0] = new Vector2(4);

            result = MatchObjects(value1, value2); 
            Assert.AreEqual(result, new DataMatch(4, 12));

            value1.Values.Add(1);

            result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(4, 13));
        }

        public class MyAsset
        {
            public MyAsset()
            {
                SubObjects = new List<SubObject>();
            }

            public SubObject SubObject { get; set; }

            public List<SubObject> SubObjects { get; set; }
        }

        public class SubObject
        {
            public string Name { get; set; }

            public int Value { get; set; }
        }

        [Test]
        public void TestSubObject()
        {
            var value1 = new MyAsset() {SubObject = new SubObject() {Name = "test1", Value = 1}};
            value1.SubObjects.Add(new SubObject() { Name = "test2", Value = 2 });

            var value2 = new MyAsset() { SubObject = new SubObject() { Name = "test1", Value = 1 } };
            value2.SubObjects.Add(new SubObject() { Name = "test2", Value = 2 });

            var result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(4, 4));

            value2.SubObject.Name = "test3";
            result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(3, 4));

            value2.SubObjects[0].Name = "test3";
            result = MatchObjects(value1, value2);
            Assert.AreEqual(result, new DataMatch(2, 4));
        }

        private DataMatch MatchObjects(object left, object right)
        {
            Console.WriteLine("---");

            var diff1 = DataVisitNodeBuilder.Run(AssetRegistry.TypeDescriptorFactory, left);
            var diff2 = DataVisitNodeBuilder.Run(AssetRegistry.TypeDescriptorFactory, right);

            var matcher = new DataMatcher(AssetRegistry.TypeDescriptorFactory);
            return matcher.Match(diff1, diff2);
        }
    }
}