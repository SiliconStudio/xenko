// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
