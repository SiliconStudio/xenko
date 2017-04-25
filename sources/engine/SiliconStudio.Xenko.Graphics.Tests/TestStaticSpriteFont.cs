// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestStaticSpriteFont : TestSpriteFont
    {
        public TestStaticSpriteFont()
            : base("StaticFonts/", "sta")
        {
            CurrentVersion = 6; // Font names & sizes changed slightly
        }

        public static void Main()
        {
            using (var game = new TestStaticSpriteFont())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestStaticSpriteFont()
        {
            RunGameTest(new TestStaticSpriteFont());
        }
    }
}
