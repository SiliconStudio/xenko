// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Core.Design.Tests.Extensions
{
    [TestFixture]
    public class TestDesingExtensions
    {
        private class Node
        {
            public Node(string value)
            {
                Value = value;
            }

            public ICollection<Node> Children { get; } = new List<Node>();

            public string Value { get; }
        }

        private Node tree;

        [TestFixtureSetUp]
        public void Setup()
        {
            tree = new Node("A")
            {
                Children =
                {
                    new Node("B")
                    {
                        Children =
                        {
                            new Node("D"),
                            new Node("E")
                            {
                                Children =
                                {
                                    new Node("H")
                                },
                            },
                        },
                    },
                    new Node("C")
                    {
                        Children =
                        {
                            new Node("F"),
                            new Node("G"),
                        },
                    },
                },
            };
        }

        [Test]
        public void TestBreadthFirst()
        {
            var result = tree.BreadthFirst(n => n.Children).Aggregate(string.Empty, (s, n) => string.Concat(s, n.Value));
            Assert.AreEqual("ABCDEFGH", result);

            result = tree.Children.BreadthFirst(n => n.Children).Aggregate(string.Empty, (s, n) => string.Concat(s, n.Value));
            Assert.AreEqual("BCDEFGH", result);
        }
        
        [Test]
        public void TestDepthFirst()
        {
            var result = tree.DepthFirst(n => n.Children).Aggregate(string.Empty, (s, n) => string.Concat(s, n.Value));
            Assert.AreEqual("ABDEHCFG", result);

            result = tree.Children.DepthFirst(n => n.Children).Aggregate(string.Empty, (s, n) => string.Concat(s, n.Value));
            Assert.AreEqual("BDEHCFG", result);
        }

        [Test]
        public void TestSelectDeep()
        {
            var result = tree.Children.SelectDeep(n => n.Children).Aggregate(string.Empty, (s, n) => string.Concat(s, n.Value));
            Assert.AreEqual("BCFGDEH", result);
        }
    }
}
