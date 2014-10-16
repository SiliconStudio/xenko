// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Presentation.Controls.PropertyGrid.Attributes;

namespace SiliconStudio.Presentation.SampleApp
{
    public enum TestEnum
    {
        FirstValue,
        SecondValue,
        ThirdValue,
        FourthValue
    }

    public struct TestStruct
    {
        public int Integer { get; set; }
        public float Float { get; set; }
        public string String { get; set; }
    }

    public class TestClass
    {
        public float Float { get; set; }
        public string String { get; set; }
    }

    public abstract class TestAbstractClass
    {
        public int DefaultInt { get; set; }
    }

    public class TestImplemClass1 : TestAbstractClass
    {
        public string String1 { get; set; }
    }

    public class TestImplemClass2 : TestAbstractClass
    {
        public string String2 { get; set; }
    }

    public class PropertyGridViewModel
    {
        public PropertyGridViewModel()
        {
            IntegerList = new List<int>();
            StructureList = new List<TestStruct>();
            ClassList = new List<TestClass>();
            IntegerDictionary = new Dictionary<string, int>();
            ClassDictionary = new Dictionary<string, TestClass>();
            AbstractList = new List<TestAbstractClass>();
            AbstractDictionary = new Dictionary<string, TestAbstractClass>();
            ListOfListsInt = new List<List<int>>();
            ListOfListsStruct = new List<List<TestStruct>>();
            ListOfListsClass = new List<List<TestClass>>();
            Color = new Color(0, 0, 0, 255);
            Color4 = new Color4(Color3, 1.0f);
        }

        [PropertyOrder(10)]
        public string Name { get; set; }

        [PropertyOrder(20)]
        public float Size { get; set; }

        [PropertyOrder(23)]
        public byte Byte { get; set; }

        [PropertyOrder(25)]
        public bool Boolean { get; set; }

        [PropertyOrder(27)]
        public ushort UnsignedShort { get; set; }

        [PropertyOrder(30)]
        public TestEnum Enum { get; set; }

        [PropertyOrder(35)]
        public TestEnum? NullableEnum { get; set; }

        [PropertyOrder(40)]
        public char Character { get; set; }

        [PropertyOrder(42)]
        [ExpandableObject]
        public Color Color { get; set; }

        [PropertyOrder(43)]
        [ExpandableObject]
        public Color3 Color3 { get; set; }

        [PropertyOrder(44)]
        [ExpandableObject]
        public Color4 Color4 { get; set; }

        [PropertyOrder(45)]
        [ExpandableObject]
        public Vector2 Vector2 { get; set; }

        [PropertyOrder(46)]
        [ExpandableObject]
        public Vector3 Vector3 { get; set; }

        [PropertyOrder(47)]
        [ExpandableObject]
        public Vector4 Vector4 { get; set; }

        [PropertyOrder(48)]
        [ExpandableObject]
        public Matrix Matrix { get; set; }

        [PropertyOrder(49)]
        public Rectangle? NullableRectangle { get; set; }

        [PropertyOrder(50)]
        public List<int> IntegerList { get; private set; }

        [PropertyOrder(60)]
        [ExpandableObject]
        public TestStruct Structure { get; set; }

        [PropertyOrder(70)]
        public List<TestStruct> StructureList { get; set; }

        [PropertyOrder(80)]
        [ExpandableObject]
        public TestClass Class { get; set; }

        [PropertyOrder(90)]
        public List<TestClass> ClassList { get; set; }

        [PropertyOrder(110)]
        public Dictionary<string, int> IntegerDictionary { get; set; }

        [PropertyOrder(120)]
        public Dictionary<string, TestClass> ClassDictionary { get; set; }

        [PropertyOrder(130)]
        public TestAbstractClass AbstractClass { get; set; }

        [PropertyOrder(140)]
        public List<TestAbstractClass> AbstractList { get; set; }

        [PropertyOrder(150)]
        public Dictionary<string, TestAbstractClass> AbstractDictionary { get; set; }

        [PropertyOrder(160)]
        public List<List<int>> ListOfListsInt { get; set; }

        [PropertyOrder(170)]
        public List<List<TestStruct>> ListOfListsStruct { get; set; }

        [PropertyOrder(180)]
        public List<List<TestClass>> ListOfListsClass { get; set; }

        public static PropertyGridViewModel CreateNew()
        {
            var testAsset = new PropertyGridViewModel
            {
                Name = "The name",
                Enum = TestEnum.ThirdValue,
                Character = 'f',
                Size = 16.5f,
                IntegerList = { 4, 6, 8 },
                Structure = new TestStruct { Float = 1.0f, Integer = 2, String = "Inner string" },
                StructureList =
                {
                    new TestStruct { Float = 2.0f, Integer = 1, String = "Inner string1" },
                    new TestStruct { Float = 4.0f, Integer = 3, String = "Inner string2" },
                    new TestStruct { Float = 8.0f, Integer = 6, String = "Inner string3" },
                },
                Class = new TestClass { Float = 8.0f, String = "Inner class string" },
                ClassList =
                {
                    new TestClass { Float = 18.0f, String = "Inner class string1" },
                    new TestClass { Float = 28.0f, String = "Inner class string2" },
                    new TestClass { Float = 38.0f, String = "Inner class string3" },
                },
                ClassDictionary =
                {
                    { "First", new TestClass { Float = 21.0f, String = "Inner class string1" } },
                    { "Second", new TestClass { Float = 31.0f, String = "Inner class string2" } },
                    { "Third", new TestClass { Float = 41.0f, String = "Inner class string3" } },
                },
                ListOfListsInt =
                {
                    new List<int> { 2, 4 },
                    new List<int> { 6, 8 }
                },
                ListOfListsStruct = 
                {
                    new List<TestStruct> { new TestStruct() },
                    new List<TestStruct> { new TestStruct() }
                },
                ListOfListsClass = 
                {
                    new List<TestClass> { new TestClass() },
                    new List<TestClass> { new TestClass() }
                }
            };

            return testAsset;
        }
    }
}
