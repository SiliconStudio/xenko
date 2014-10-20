// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Texture;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestTexturePacker
    {
        [Test]
        public void TestFindMacRectWithNoRegion()
        {
            var sheetRect = new Rectangle(0, 0, 50, 78);
            var maxRect = TexturePacker.FindMaxRectangle(ref sheetRect, new List<TexturePacker.RotatableRectangle>());
            Assert.AreEqual(sheetRect, maxRect);
        }

        [Test]
        public void TestFindMacRectWithOneRegion()
        {
            var sheetRect = new Rectangle(0, 0, 100, 100);
            var maxRect = TexturePacker.FindMaxRectangle(ref sheetRect, 
                new List<TexturePacker.RotatableRectangle>
                    {
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(0, 0, 20, 30), IsRotated = false }
                    }
                );
            Assert.AreEqual(new Rectangle(20, 0, 80, 100), maxRect);
        }

        [Test]
        public void TestFindMacRectWithMultipleRegions()
        {
            var sheetRect = new Rectangle(0, 0, 100, 100);
            var maxRect = TexturePacker.FindMaxRectangle(ref sheetRect,
                new List<TexturePacker.RotatableRectangle>
                    {
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(0, 0, 20, 30), IsRotated = false },
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(20, 0, 80, 30), IsRotated = false },
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(0, 30, 60, 70), IsRotated = false },
                    }
                );
            Assert.AreEqual(new Rectangle(60, 30, 40, 70), maxRect);
        }

        [Test]
        public void TestFindMacRectWithMultipleRegionsWithRotation()
        {
            var sheetRect = new Rectangle(0, 0, 120, 100);
            var maxRect = TexturePacker.FindMaxRectangle(ref sheetRect,
                new List<TexturePacker.RotatableRectangle>
                    {
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(0, 0, 20, 30), IsRotated = true },
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(30, 0, 70, 20), IsRotated = false },
                        new TexturePacker.RotatableRectangle { BaseRectangle = new Rectangle(0, 40, 20, 60), IsRotated = false },
                    }
                );
            Assert.AreEqual(new Rectangle(20, 20, 100, 80), maxRect);
        }

        [Test]
        public void TestPackRectanglesWithRotate()
        {
            var configuration = new TexturePacker.PackingConfiguration
                {
                    CanRotate = true,
                    PreferredWidth = 100,
                    PreferredHeight = 100
                };

            var packedSheet = TexturePacker.PackRectangles(configuration, 
                new List<Rectangle>
                {
                    new Rectangle(0, 0, 20, 30), // 4
                    new Rectangle(0, 0, 70, 20), // 2
                    new Rectangle(0, 0, 20, 60), // 3
                    new Rectangle(0, 0, 79, 50), // 1
                });

            Assert.AreEqual(configuration.PreferredWidth, packedSheet[0].Width);
            Assert.AreEqual(configuration.PreferredHeight, packedSheet[0].Height);
            Assert.AreEqual(4, packedSheet[0].Rectangles.Count);
        }

        [Test]
        public void TestPackRectanglesWithoutRotate()
        {
            var configuration = new TexturePacker.PackingConfiguration
            {
                CanRotate = false,
                PreferredWidth = 100,
                PreferredHeight = 100
            };

            var packedSheet = TexturePacker.PackRectangles(configuration,
                new List<Rectangle>
                {
                    new Rectangle(0, 0, 20, 30), // 4
                    new Rectangle(0, 0, 70, 20), // 2
                    new Rectangle(0, 0, 20, 60), // 3
                    new Rectangle(0, 0, 80, 50), // 1
                });

            Assert.AreEqual(configuration.PreferredWidth, packedSheet[0].Width);
            Assert.AreEqual(configuration.PreferredHeight, packedSheet[0].Height);
            Assert.AreEqual(4, packedSheet[0].Rectangles.Count);
        }
    
    }
}
