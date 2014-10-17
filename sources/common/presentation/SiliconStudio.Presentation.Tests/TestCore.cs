// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Extensions;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Tests
{
    [TestFixture]
    class TestCore
    {
        public class Dummy : ViewModelBase, IComparable
        {
            private string name;

            public string Name { get { return name; } set { SetValue(ref name, value); } }

            public Dummy(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return Name;
            }

            public int CompareTo(object obj)
            {
                return obj == null ? 1 : String.Compare(Name, ((Dummy)obj).Name, StringComparison.Ordinal);
            }
        }

        [Test]
        public void TestSortedObservableCollection()
        {
            var collection = new SortedObservableCollection<int> { 5, 13, 2, 9, 0, 8, 5, 11, 1, 7, 14, 12, 4, 10, 3, 6 };
            collection.Remove(5);

            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i] == i);
                Assert.That(collection.BinarySearch(i) == i);
            }

            Assert.Throws<InvalidOperationException>(() => collection[4] = 10);
            Assert.Throws<InvalidOperationException>(() => collection.Move(4, 5));
        }

        [Test]
        public void TestAutoUpdatingSortedObservableCollection()
        {
            var collection = new AutoUpdatingSortedObservableCollection<Dummy> { new Dummy("sss"), new Dummy("eee") };

            var dummy = new Dummy("ggg");
            collection.Add(dummy);

            var sorted = new[] { "eee", "ggg", "sss" };

            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }

            dummy.Name = "aaa";
            sorted = new[] { "aaa", "eee", "sss" };
            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }

            dummy.Name = "zzz";
            sorted = new[] { "eee", "sss", "zzz" };
            for (int i = 0; i < collection.Count; ++i)
            {
                Assert.That(collection[i].Name == sorted[i]);
                Assert.That(collection.BinarySearch(sorted[i], (d, s) => String.Compare(d.Name, s, StringComparison.Ordinal)) == i);
            }
        }

        [Test]
        public void TestCamelCaseSplit()
        {
            const string String1 = "ThisIsOneTestString";
            const string String2 = "ThisOneABCContainsAbreviation";
            const string String3 = "ThisOneContainsASingleCharacterWord";
            const string String4 = "ThisOneEndsWithAbbreviationABC";
            const string String5 = "ThisOneEndsWithASingleCharacterWordZ";
            var expected1 = new[] { "This", "Is", "One", "Test", "String" };
            var expected2 = new[] { "This", "One", "ABC", "Contains", "Abreviation" };
            var expected3 = new[] { "This", "One", "Contains", "A", "Single", "Character", "Word" };
            var expected4 = new[] { "This", "One", "Ends", "With", "Abbreviation", "ABC" };
            var expected5 = new[] { "This", "One", "Ends", "With", "A", "Single", "Character", "Word", "Z" };

            var split1 = String1.CamelCaseSplit();
            Assert.AreEqual(expected1.Length, split1.Count);
            for (int i = 0; i < expected1.Length; ++i)
            {
                Assert.AreEqual(expected1[i], split1[i]);
            }
            var split2 = String2.CamelCaseSplit();
            Assert.AreEqual(expected2.Length, split2.Count);
            for (int i = 0; i < expected2.Length; ++i)
            {
                Assert.AreEqual(expected2[i], split2[i]);
            }
            var split3 = String3.CamelCaseSplit();
            Assert.AreEqual(expected3.Length, split3.Count);
            for (int i = 0; i < expected3.Length; ++i)
            {
                Assert.AreEqual(expected3[i], split3[i]);
            }
            var split4 = String4.CamelCaseSplit();
            Assert.AreEqual(expected4.Length, split4.Count);
            for (int i = 0; i < expected4.Length; ++i)
            {
                Assert.AreEqual(expected4[i], split4[i]);
            }
            var split5 = String5.CamelCaseSplit();
            Assert.AreEqual(expected5.Length, split5.Count);
            for (int i = 0; i < expected5.Length; ++i)
            {
                Assert.AreEqual(expected5[i], split5[i]);
            }
        }
    }
}
